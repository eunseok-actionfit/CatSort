using AES.Tools;
using AES.Tools.StateMachine.Core;
using AES.Tools.VContainer;
using App;
using App.Booster;
using App.Economy;
using App.Economy.Coin;
using App.Gameplay;
using App.Mvvm;
using App.State;
using Cysharp.Threading.Tasks;
using Domain;
using Presentation;


public sealed class GameSceneViewModel
{
    public ObservableProperty<GameStatus> Status { get; } = new();
    public BoardViewModel BoardVm { get; }

    [Bindable]
    public int LevelId { get; private set; }
    public LevelDifficulty Difficulty { get; private set; }

    public Command RetryCommand { get; }
    public Command HomeCommand { get; }

    // Booster
    public Command UseUndoBoosterCommand { get; }
    public Command UseShuffleBoosterCommand { get; }
    public Command UseAddColumnBoosterCommand { get; }

    public Command FailContinueWithAddColumnCommand { get; }

    private readonly LoadLevelUseCase _loadLevelUseCase;
    private readonly HandleLevelClearedUseCase _handleLevelClearedUseCase;
    private readonly ConsumeBoosterUseCase _consumeBoosterUseCase;
    private readonly SpendCoinUseCase _spendCoinUseCase;

    [Bindable]
    public CoinViewModel CoinVm { get; }

    [Bindable]
    public BoosterBarViewModel BoosterBarViewModel { get; }

    private readonly ISceneFlow _sceneFlow;

    private readonly GameFlowFsm _flow = new();

    public StateMachine DebugMachine => _flow.Machine;

    public GameSceneViewModel(
        LoadLevelUseCase loadLevelUseCase,
        BoardViewModel boardVm,
        ISceneFlow sceneFlow,
        HandleLevelClearedUseCase handleLevelClearedUseCase,
        CoinViewModel coinVm,
        ConsumeBoosterUseCase consumeBoosterUseCase,
        BoosterBarViewModel boosterBarViewModel,
        SpendCoinUseCase spendCoinUseCase)
    {
        _loadLevelUseCase = loadLevelUseCase;
        BoardVm = boardVm;
        _sceneFlow = sceneFlow;
        _handleLevelClearedUseCase = handleLevelClearedUseCase;
        CoinVm = coinVm;

        _consumeBoosterUseCase = consumeBoosterUseCase;
        BoosterBarViewModel = boosterBarViewModel;
        _spendCoinUseCase = spendCoinUseCase;

        RetryCommand = new Command(OnRetry);

        HomeCommand = new Command(() => GoHomeAsync().Forget()
        );

        FailContinueWithAddColumnCommand = new Command(
            () => ContinueAfterFailWithAddColumnAsync().Forget(),
            () =>
                Status.Value == GameStatus.Failed &&
                CoinVm.Coin.Value >= Cost
        );

        // ====== FSM Inputs ======
        BoardVm.PendingOutcome.OnValueChanged += outcome => {
            _flow.SetPendingOutcome(outcome);
            SyncStatusFromFlow();
        };

        BoardVm.AllFxDone.OnValueChanged += done => {
            _flow.SetAllFxDone(done);
            SyncStatusFromFlow();
        };

        // ====== Booster Commands ======
        UseUndoBoosterCommand = new Command(
            execute: () => UseUndoBoosterAsync().Forget(),
            canExecute: () =>
                BoardVm.Status.Value == GameStatus.Playing &&
                BoardVm.CanUndo.Value &&
                BoosterBarViewModel.Undo.Count.Value > 0
        );

        UseShuffleBoosterCommand = new Command(
            execute: () => UseShuffleBoosterAsync().Forget(),
            canExecute: () =>
                BoardVm.Status.Value == GameStatus.Playing &&
                BoosterBarViewModel.Shuffle.Count.Value > 0
        );

        UseAddColumnBoosterCommand = new Command(
            execute: () => UseAddColumnBoosterAsync().Forget(),
            canExecute: () =>
                BoardVm.Status.Value == GameStatus.Playing &&
                BoosterBarViewModel.AddColumn.Count.Value > 0
        );
    }

    private void SyncStatusFromFlow()
    {
        var gameStatus = GameFlowFsm.ToGameStatus(_flow.CurrentId);

        // 커맨드/보드 입력이 BoardVm.Status를 많이 보므로 둘 다 동기화
        Status.Value = gameStatus;
        BoardVm.Status.Value = gameStatus;

        if (_flow.CurrentId == GameFlowStateId.ClearResult)
        {
            _handleLevelClearedUseCase.ExecuteAsync(LevelId, Difficulty).Forget();
            CoinVm.RefreshAsync().Forget();

            // 클리어 확정 이후에는 pending을 제거해도 됨(선택)
            BoardVm.PendingOutcome.Value = GameStatus.Playing;
        }
    }

    private void OnRetry() => _sceneFlow.GoGame(null).Forget();

    private async UniTaskVoid GoHomeAsync()
    {
        if(ADS.TryShowInterstitial(AdReason.StageClear));

        await _sceneFlow.GoHome();
    }

    private async UniTaskVoid UseUndoBoosterAsync()
    {
        var ok = await _consumeBoosterUseCase.ExecuteAsync(BoosterId.Undo, 1);
        if (!ok) return;

        BoardVm.UndoCommand.Execute();
        await BoosterBarViewModel.Undo.RefreshAsync();
    }

    private async UniTaskVoid UseShuffleBoosterAsync()
    {
        var ok = await _consumeBoosterUseCase.ExecuteAsync(BoosterId.Shuffle, 1);
        if (!ok) return;

        BoardVm.Shuffle();
        await BoosterBarViewModel.Shuffle.RefreshAsync();
    }

    private async UniTaskVoid UseAddColumnBoosterAsync()
    {
        var ok = await _consumeBoosterUseCase.ExecuteAsync(BoosterId.AddColumn, 1);
        if (!ok) return;

        BoardVm.AddcolumnCommand.Execute();
        await BoosterBarViewModel.AddColumn.RefreshAsync();
    }

    public async UniTask LoadLevelAsync(int levelId)
    {
        LevelId = levelId;

        var result = await _loadLevelUseCase.ExecuteAsync(levelId);
        Difficulty = result.Difficulty;

        _flow.ResetToPlaying();

        BoardVm.ApplyLayout(result.Layout);
        BoardVm.ApplyState(result.State);

        BoardVm.PendingOutcome.Value = GameStatus.Playing;
        BoardVm.Status.Value = GameStatus.Playing;
        Status.Value = GameStatus.Playing;

        // 초기 입력값 반영
        _flow.SetAllFxDone(BoardVm.AllFxDone.Value);
        _flow.SetPendingOutcome(BoardVm.PendingOutcome.Value);
        SyncStatusFromFlow();
    }

    private const int Cost = 900;

    private async UniTaskVoid ContinueAfterFailWithAddColumnAsync()
    {
        // FailMenu -> ContinueApplying
        _flow.FireContinuePressed();
        SyncStatusFromFlow();

        var ok = await _spendCoinUseCase.ExecuteAsync(Cost);

        if (!ok)
        {
            // ContinueApplying -> ContinueFailed
            _flow.FireContinueFailed();
            SyncStatusFromFlow();
            return;
        }

        CoinVm.RefreshAsync().Forget();

        BoardVm.AddcolumnCommand.Execute();

        // 이어하기 성공: 예약 결과/진행 상태 리셋 후 Playing 복귀
        BoardVm.PendingOutcome.Value = GameStatus.Playing;

        _flow.ClearPendingOutcome();
        _flow.FireContinueApplied();
        SyncStatusFromFlow();
    }
}