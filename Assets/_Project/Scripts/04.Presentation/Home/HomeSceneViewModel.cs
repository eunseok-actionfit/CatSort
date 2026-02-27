using System;
using AES.Tools;
using AES.Tools.VContainer;
using App.Home;
using App.Economy;
using App.Economy.Lives;
using App.Mvvm; // ConsumeLifeForStageUseCase
using Cysharp.Threading.Tasks;
using Domain;

namespace Presentation
{
    public sealed class HomeSceneViewModel
    {
        public ObservableProperty<int> Stage { get; }
        public Command StartGameCommand { get; }
        public Command RefillLivesWithCoinCommand { get; }
        
        public AsyncCommand WatchRewardAdForLifeCommand { get; }
        
        [Bindable]
        public LivesViewModel LivesVm { get; }
        [Bindable]
        public CoinViewModel CoinVM { get; }
        [Bindable]
        public ShopViewModel ShopVm { get; }

        public event Action OpenPopupRequested;
        public event Action ClosePopupRequested;

        private readonly ISceneFlow _sceneFlow;
        private readonly GetHomeDefaultStageUseCase _getDefaultStageUseCase;
        private readonly CanStartStageUseCase _canStartStageUseCase;
        private readonly ConsumeLifeForStageUseCase _consumeLifeForStageUseCase;
        private readonly RefillLivesWithCoinUseCase _refillLivesWithCoinUseCase;
        private readonly AddLifeFromRewardUseCase   _addLifeFromRewardUseCase;

        private bool _lastCanStartResult = false;

        public HomeSceneViewModel(
            ISceneFlow sceneFlow,
            GetHomeDefaultStageUseCase getDefaultStageUseCase,
            CanStartStageUseCase canStartStageUseCase,
            ConsumeLifeForStageUseCase consumeLifeForStageUseCase,
            LivesViewModel livesVm,
            CoinViewModel coinVM,
            RefillLivesWithCoinUseCase refillLivesWithCoinUseCase,
            AddLifeFromRewardUseCase addLifeFromRewardUseCase,
            ShopViewModel shopVm)
        {
            LivesVm = livesVm;
            CoinVM = coinVM;
            _refillLivesWithCoinUseCase = refillLivesWithCoinUseCase;
            _addLifeFromRewardUseCase = addLifeFromRewardUseCase;
            ShopVm = shopVm;

            _sceneFlow = sceneFlow;
            _getDefaultStageUseCase = getDefaultStageUseCase;
            _canStartStageUseCase = canStartStageUseCase;
            _consumeLifeForStageUseCase = consumeLifeForStageUseCase;

            Stage = new ObservableProperty<int>();

            StartGameCommand = new Command(
                () => StartGameAsync().Forget(),
                () => _lastCanStartResult
            );
            
            // 900코인으로 라이프 풀 채우기
            RefillLivesWithCoinCommand = new Command(
                () => RefillLivesWithCoinAsync().Forget(),
                () => CoinVM.Coin.Value >= RefillLivesWithCoinUseCase.DefaultCost
            );

            // 리워드 광고 보고 라이프 +1
            WatchRewardAdForLifeCommand = new AsyncCommand(
                _ => WatchRewardAdForLifeAsync(),
                _ => ADS.IsBound && ADS.IsReadyRewarded
            );


            IAP.PurchaseConfirmedByProductKey += _ =>
            {
                CoinVM.RefreshAsync().Forget();
                LivesVm.RefreshOnceAsync().Forget();
            };

            Initialize().Forget();
        }

        private async UniTaskVoid Initialize()
        {
            var defaultStage = await _getDefaultStageUseCase.ExecuteAsync();
            Stage.Value = defaultStage;

            _lastCanStartResult = await _canStartStageUseCase.CanStartAsync(Stage.Value);

            Stage.OnValueChanged += async newValue =>
            {
                _lastCanStartResult = await _canStartStageUseCase.CanStartAsync(newValue);
            };
        }

        private async UniTaskVoid StartGameAsync()
        {
            var success = await _consumeLifeForStageUseCase.ExecuteAsync();
            if (!success)
            {
                OpenPopupRequested?.Invoke();
                return;
            }

            var stage = Stage.Value;
            var stageArgs = new SceneStartArgs(stage);
            await _sceneFlow.GoGame(stageArgs, overlayKey: AssetKeys.UI_LoadingBoot);
        }
        
        private async UniTaskVoid RefillLivesWithCoinAsync()
        {
            const int Cost = RefillLivesWithCoinUseCase.DefaultCost;

            var success = await _refillLivesWithCoinUseCase.ExecuteAsync(Cost);
            if (!success)
                return;

            CoinVM.RefreshAsync().Forget();
            LivesVm.RefreshOnceAsync().Forget();
            
            _lastCanStartResult = await _canStartStageUseCase.CanStartAsync(Stage.Value);

            ClosePopupRequested?.Invoke();
        }

        // 리워드 광고 하트 +1 성공 시
        private async UniTask WatchRewardAdForLifeAsync()
        {
            if (!ADS.IsBound || !ADS.IsReadyRewarded)
                return;

            var rewarded = await ADS.ShowRewardedAsync(AdReason.RewardBtn);
            if (!rewarded)
                return;

            var success = await _addLifeFromRewardUseCase.ExecuteAsync(1);
            if (!success)
                return;

            LivesVm.RefreshOnceAsync().Forget();
            _lastCanStartResult = await _canStartStageUseCase.CanStartAsync(Stage.Value);

            ClosePopupRequested?.Invoke();
        }
    }
}
