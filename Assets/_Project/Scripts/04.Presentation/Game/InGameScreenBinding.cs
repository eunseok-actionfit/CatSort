using System;
using AES.Tools;
using AES.Tools.StateMachine.Core;
using AES.Tools.TBC.Collections;
using AES.Tools.UI.UINavigtion;
using App.Mvvm;
using App.State;
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class InGameScreenBinding : ContextListenerBase, IStateMachineOwner
{
    [SerializeField] private UINavigator navigator;

    private GameSceneViewModel _vm;
    private CompositeDisposable _disposables = new();

    protected override void OnContextAvailable(IBindingContext ctx)
    {
        _disposables.Dispose();
        _disposables = new CompositeDisposable();

        _vm = ctx.GetValue() as GameSceneViewModel;
        if (_vm == null)
            return;

        ctx.SubscribeProperty<GameSceneViewModel, GameStatus>(
                this,
                vm => vm.BoardVm.Status,
                OnStatusChanged)
            .AddTo(_disposables);
    }

    protected override void OnContextUnavailable()
    {
        _disposables.Dispose();
        _disposables = new CompositeDisposable();
    }

    private void OnStatusChanged(GameStatus status)
    {
        switch (status)
        {
            case GameStatus.Cleared:
                navigator.Push<GameClearScreen>();
                break;
            case GameStatus.Failed:
                navigator.Push<GameFailScreen>();
                break;
            case GameStatus.Playing:
                navigator.GoRoot();
                break;
        }
    }
    
    public StateMachine Machine => _vm.DebugMachine;
}
