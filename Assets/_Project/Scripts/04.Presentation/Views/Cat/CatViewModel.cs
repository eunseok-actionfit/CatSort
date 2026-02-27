// CatViewModel.cs
using System;
using AES.Tools;
using AES.Tools.StateMachine.Core;
using App;
using Domain;

public sealed class CatViewModel
{
    public enum CatMotionState
    {
        Idle,
        Air,
        Landing,
        Eating,
        Completed,
    }

    public CatId CatId { get; }

    public ObservableProperty<CatColor> Color { get; } = new();
    public ObservableProperty<bool> Hidden { get; } = new();
    public ObservableProperty<bool> Selected { get; } = new();

    public ObservableProperty<bool> Completed { get; } = new();

    public ObservableProperty<CatMotionState> MotionState { get; } = new(CatMotionState.Idle);
    public ObservableProperty<bool> IsMoving { get; } = new();

    public ObservableProperty<bool> IsCompletingAnim { get; } = new(false);

    //  컬럼에서 전달받는 seed (점프셋 결정은 View에서)
    public ObservableProperty<int> JumpSeed { get; } = new(0);

    public bool CantSelect =>
        IsMoving.Value ||
       // MotionState.Value == CatMotionState.Landing ||
        IsCompletingAnim.Value;
    
    public bool IsBusy =>
        IsMoving.Value ||
        MotionState.Value == CatMotionState.Landing ||
        IsCompletingAnim.Value;

    public float MoveDelay { get; private set; }

    private readonly CatLogicFsm _fsm;
    public StateMachine DebugMachine => _fsm.MachineRef;

    public CatViewModel(CatId id)
    {
        CatId = id;
        _fsm = new CatLogicFsm(this);
    }

    public void ApplyState(CatStateDto dto)
    {
        Color.Value = (CatColor)dto.Color;
        Hidden.Value = dto.IsHidden;

        bool prev = Completed.Value;
        bool next = dto.IsCompleted;
        Completed.Value = next;

        if (prev && !next)
        {
            IsCompletingAnim.Value = false;
        }
    }

    public void Tick(float now) => _fsm.Tick(now);

    public void NotifyMoveStarted() => IsMoving.Value = true;
    public void NotifyArrived() => IsMoving.Value = false;
    public void NotifyLandingAnimDone() => _fsm.NotifyLandingAnimDone();

    public void SetMoveDelay(float delay) => MoveDelay = delay;

    public void RequestCompleteAnim(float delay)
    {
        if (!Completed.Value) return;
        if (IsCompletingAnim.Value) return;

        SetMoveDelay(delay);
        IsCompletingAnim.Value = true;

        _fsm.NotifyStartCompleteAnim();
    }

    public void NotifyCompleteAnimDone()
    {
        IsCompletingAnim.Value = false;
    }

    public ObservableProperty<bool> IsOnDestinationOnlyColumn { get; } = new();

    public event Action Removed;
    public void MarkRemoved() => Removed?.Invoke();
}
