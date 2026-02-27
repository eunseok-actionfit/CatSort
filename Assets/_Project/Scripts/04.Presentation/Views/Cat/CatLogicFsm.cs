// CatLogicFsm.cs (Completed 단일 상태 버전)
using AES.Tools;
using AES.Tools.StateMachine.Core;


internal sealed class CatLogicFsm : StatefulObject
{
    private readonly CatViewModel _vm;

    private readonly IState _idle      = new IdleState();
    private readonly IState _air       = new AirState();
    private readonly IState _landing   = new LandingState();
    private readonly IState _completed = new CompletedState();
    private readonly IState _eating = new EatingState();

    private readonly TriggerParameter _landingAnimDone   = new();
    private readonly TriggerParameter _startCompleteAnim = new();

    public StateMachine MachineRef => Machine;

    public CatLogicFsm(CatViewModel vm)
    {
        _vm = vm;

        Machine.CurrentState.Subscribe(s =>
        {
            _vm.MotionState.Value = Map(s);
        });

        BuildGraph();
        SetInitialState(_idle);
    }

    public void Tick(float now) => Machine.Update(now);

    public void NotifyLandingAnimDone() => _landingAnimDone.Fire();

    // "완료 애니 시작"은 보드가 AllFxDone 이후에 호출
    public void NotifyStartCompleteAnim() => _startCompleteAnim.Fire();

    private void BuildGraph()
    {
        var g = Machine.BuildGraph();

        // 완료된 고양이는 선택/점프 상태로 들어가면 안 됨
        g.FromAny()
            .To(_air)
            .When(_vm.IsMoving.IsTrue().Or(_vm.Selected.IsTrue().And(_vm.Completed.IsFalse())))
            .Named("Any->Air")
            .Add();
        

        g.From(_air).To(_landing)
            .When(_vm.IsMoving.IsFalse().And(_vm.Selected.IsFalse()))
            .Named("Air->Landing")
            .Add();

        g.From(_landing).To(_idle)
            .When(_landingAnimDone.AsTrigger())
            .Named("Landing->Idle")
            .Add();
        
        g.From(_idle).To(_completed)
            .When(_startCompleteAnim.AsTrigger())
            .Priority(100)
            .Named("Idle->Completed")
            .Add();
        
        g.From(_completed).To(_idle)
            .When(_vm.Completed.IsFalse())
            .Named("Completed->Idle(Undo)")
            .Add();
        
        g.From(_idle).To(_eating)
            .When(_vm.IsOnDestinationOnlyColumn.IsTrue())
            .Priority(90)
            .Named("Idle->Eating")
            .Add();
        
        g.From(_eating).To(_idle)
            .When(_vm.IsOnDestinationOnlyColumn.IsFalse())
            .Named("Eating->Idle")
            .Add();
    }

    private CatViewModel.CatMotionState Map(IState s)
    {
        if (s == _idle)      return CatViewModel.CatMotionState.Idle;
        if (s == _air)       return CatViewModel.CatMotionState.Air;
        if (s == _landing)   return CatViewModel.CatMotionState.Landing;
        if (s == _eating) return CatViewModel.CatMotionState.Eating;
        if (s == _completed) return CatViewModel.CatMotionState.Completed;
        return CatViewModel.CatMotionState.Idle;
    }

    private sealed class IdleState : IState { }
    private sealed class AirState : IState { }
    private sealed class LandingState : IState { }
    private sealed class CompletedState : IState { }
    private sealed class EatingState : IState { }
}

