using AES.Tools.StateMachine.Core;


namespace App.State
{
    public enum GameFlowStateId
    {
        Playing,
        PendingFail,
        PendingClear,
        FailMenu,
        ContinueApplying,
        ContinueFailed,
        GameOver,
        ClearResult
    }

    public sealed class GameFlowFsm : StatefulObject
    {
        // ====== Inputs (외부에서 세팅) ======
        private readonly BoolParameter _pendingFail = new(false);
        private readonly BoolParameter _pendingClear = new(false);
        private readonly BoolParameter _allFxDone = new(true);

        private readonly TriggerParameter _continuePressed = new();
        private readonly TriggerParameter _continueApplied = new();
        private readonly TriggerParameter _continueFailed = new();
        private readonly TriggerParameter _confirmGameOver = new(); // “진짜끝” 확인

        // ====== States ======
        private readonly IState _playing = new S.Playing();
        private readonly IState _pendingFailState = new S.PendingFail();
        private readonly IState _pendingClearState = new S.PendingClear();
        private readonly IState _failMenu = new S.FailMenu();
        private readonly IState _continueApplying = new S.ContinueApplying();
        private readonly IState _continueFailedState = new S.ContinueFailed();
        private readonly IState _gameOver = new S.GameOver();
        private readonly IState _clearResult = new S.ClearResult();

        public GameFlowStateId CurrentId => Map(Machine.CurrentStateRaw);

        public GameFlowFsm()
        {
            BuildGraph();
            SetInitialState(_playing);
        }

        public void ResetToPlaying()
        {
            _pendingFail.Value = false;
            _pendingClear.Value = false;
            _allFxDone.Value = true;

            SetInitialState(_playing);
            Machine.Update();
        }

        // ====== API (입력) ======
        public void SetPendingOutcome(GameStatus status)
        {
            _pendingFail.Value = status == GameStatus.Failed;
            _pendingClear.Value = status == GameStatus.Cleared;
            Machine.Update();
        }

        public void ClearPendingOutcome()
        {
            _pendingFail.Value = false;
            _pendingClear.Value = false;
            Machine.Update();
        }

        public void SetAllFxDone(bool done)
        {
            _allFxDone.Value = done;
            Machine.Update();
        }

        public void FireContinuePressed() { _continuePressed.Fire(); Machine.Update(); }
        public void FireContinueApplied() { _continueApplied.Fire(); Machine.Update(); }
        public void FireContinueFailed()  { _continueFailed.Fire();  Machine.Update(); }
        public void FireConfirmGameOver() { _confirmGameOver.Fire(); Machine.Update(); }

        // ====== Graph ======
        private void BuildGraph()
        {
            var g = Machine.BuildGraph();

            // Playing -> PendingFail/Clear : 도메인 판정이 나왔을 때(확정은 아직)
            g.From(_playing).To(_pendingFailState)
                .When(_pendingFail.IsTrue())
                .Priority(10)
                .Named("Playing->PendingFail").Add();

            g.From(_playing).To(_pendingClearState)
                .When(_pendingClear.IsTrue())
                .Priority(10)
                .Named("Playing->PendingClear").Add();

            // PendingFail/Clear -> 확정 : 연출(착지 포함) 다 끝나면
            g.From(_pendingFailState).To(_failMenu)
                .When(_allFxDone.IsTrue())
                .Priority(10)
                .Named("PendingFail->FailMenu").Add();

            g.From(_pendingClearState).To(_clearResult)
                .When(_allFxDone.IsTrue())
                .Priority(10)
                .Named("PendingClear->ClearResult").Add();

            // FailMenu -> ContinueApplying : 이어하기 버튼
            g.From(_failMenu).To(_continueApplying)
                .When(_continuePressed.AsTrigger())
                .Priority(10)
                .Named("FailMenu->ContinueApplying").Add();

            // ContinueApplying -> Playing : 성공 / -> ContinueFailed : 실패
            g.From(_continueApplying).To(_playing)
                .When(_continueApplied.AsTrigger())
                .Priority(10)
                .Named("ContinueApplying->Playing").Add();

            g.From(_continueApplying).To(_continueFailedState)
                .When(_continueFailed.AsTrigger())
                .Priority(10)
                .Named("ContinueApplying->ContinueFailed").Add();

            // ContinueFailed -> GameOver : 확인
            g.From(_continueFailedState).To(_gameOver)
                .When(_confirmGameOver.AsTrigger())
                .Priority(10)
                .Named("ContinueFailed->GameOver").Add();
        }

        public static GameStatus ToGameStatus(GameFlowStateId id) => id switch
        {
            GameFlowStateId.ClearResult => GameStatus.Cleared,
            GameFlowStateId.FailMenu or GameFlowStateId.ContinueFailed or GameFlowStateId.GameOver => GameStatus.Failed,
            _ => GameStatus.Playing
        };

        private GameFlowStateId Map(IState s)
        {
            if (s == _playing) return GameFlowStateId.Playing;
            if (s == _pendingFailState) return GameFlowStateId.PendingFail;
            if (s == _pendingClearState) return GameFlowStateId.PendingClear;
            if (s == _failMenu) return GameFlowStateId.FailMenu;
            if (s == _continueApplying) return GameFlowStateId.ContinueApplying;
            if (s == _continueFailedState) return GameFlowStateId.ContinueFailed;
            if (s == _gameOver) return GameFlowStateId.GameOver;
            if (s == _clearResult) return GameFlowStateId.ClearResult;
            return GameFlowStateId.Playing;
        }

        private static class S
        {
            public sealed class Playing : IState { }
            public sealed class PendingFail : IState { }
            public sealed class PendingClear : IState { }
            public sealed class FailMenu : IState { }
            public sealed class ContinueApplying : IState { }
            public sealed class ContinueFailed : IState { }
            public sealed class GameOver : IState { }
            public sealed class ClearResult : IState { }
        }
    }
}
