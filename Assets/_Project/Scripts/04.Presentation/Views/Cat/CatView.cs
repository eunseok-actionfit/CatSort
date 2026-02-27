// CatView.cs (Completed 상태에서만 완료 애니, "진입 1회" 가드)
using AES.Tools;
using AES.Tools.StateMachine.Core;
using AES.Tools.TBC.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Domain;
using Presentation;
using Presentation.Mvvm;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;


[DisallowMultipleComponent]
[RequireComponent(typeof(MonoContext))]
public sealed class CatView : ContextListenerBase, IStateMachineOwner
{
    [Header("Spine")]
    [SerializeField] private SkeletonAnimation skeleton;
    [SerializeField] private CatSkinTable skinTable;
    [SerializeField] private CatSpinePlayer spinePlayer;

    [Header("Roots")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private GameObject normalRoot;
    [SerializeField] private GameObject hiddenRoot;

    [Header("Events")]
    public UnityEvent OnIdle;
    public UnityEvent OnHover;
    public UnityEvent OnJump;
    public UnityEvent OnLanding;
    public UnityEvent OnHide;
    public UnityEvent OnUnhide;

    private CatViewModel _vm;
    private CompositeDisposable _disposables = new();

    public StateMachine Machine => _vm?.DebugMachine;

    private bool? _prevHidden;
    private Tween _moveTween;


    private bool _playedCompleteOnceInCompletedState;

    private void Awake()
    {
        if (visualRoot == null) visualRoot = transform;
        if (hiddenRoot != null) hiddenRoot.SetActive(false);
        if (normalRoot != null) normalRoot.SetActive(true);
        if (skeleton == null) skeleton = GetComponentInChildren<SkeletonAnimation>(true);
    }

    private void Update()
    {
        if (_vm == null) return;
        _vm.Tick(Time.time);
    }

    protected override void OnContextAvailable(IBindingContext ctx)
    {
        _disposables.Dispose();
        _disposables = new CompositeDisposable();

        _vm = ctx.GetValue() as CatViewModel;
        if (_vm == null) return;

        CatViewRegistry.Register(_vm, this);

        ctx.SubscribeProperty<CatViewModel, CatColor>(this, vm => vm.Color, SetColor)
            .AddTo(_disposables);

        ctx.SubscribeProperty<CatViewModel, bool>(this, vm => vm.Hidden, OnHiddenChanged)
            .AddTo(_disposables);

        ctx.SubscribeProperty<CatViewModel, bool>(this, vm => vm.Selected, OnSelectedChanged)
            .AddTo(_disposables);

        _vm.MotionState.Subscribe(OnMotionStateChanged).AddTo(_disposables);

        _vm.Removed += OnRemoved;

        _playedCompleteOnceInCompletedState = false;
        OnMotionStateChanged(_vm.MotionState.Value);
    }

    protected override void OnContextUnavailable()
    {
        _disposables.Dispose();
        _disposables = new CompositeDisposable();
        DetachVm();
    }

    private void DetachVm()
    {
        if (_vm == null) return;

        _vm.Removed -= OnRemoved;

        _moveTween?.Kill();
        _moveTween = null;

        CatViewRegistry.Unregister(_vm, this);
        _vm = null;
    }

    private void SetColor(CatColor color)
    {
        if (skinTable == null || skeleton == null) return;
        if (!skinTable.TryGet(color, out var data)) return;

        var skel = skeleton.Skeleton;
        skel.SetSkin(data.skinKey);
        skel.SetupPoseSlots();
        skeleton.AnimationState.Apply(skel);
    }

    private void OnSelectedChanged(bool selected)
    {
        if (selected)
            OnHover?.Invoke();
    }

    private void OnHiddenChanged(bool hidden)
    {
        if (hidden)
        {
            normalRoot.SetActive(false);
            hiddenRoot?.SetActive(true);
            OnHide?.Invoke();
        }
        else if (_prevHidden.HasValue)
        {
            normalRoot.SetActive(true);
            OnUnhide?.Invoke();
        }

        _prevHidden = hidden;
    }

    private void OnMotionStateChanged(CatViewModel.CatMotionState state)
    {
        switch (state)
        {
            case CatViewModel.CatMotionState.Idle:
                _playedCompleteOnceInCompletedState = false;
                spinePlayer.PlayIdleLoop();
                OnIdle.Invoke();
                break;

            case CatViewModel.CatMotionState.Air:
                _playedCompleteOnceInCompletedState = false;
                spinePlayer.SetJumpSetFromSeed(_vm.JumpSeed.Value);

                spinePlayer.PlayJumpAndAirLoop();

                break;

            case CatViewModel.CatMotionState.Landing:
                _playedCompleteOnceInCompletedState = false;
                OnLanding?.Invoke();
                spinePlayer.PlayLand(() => { _vm.NotifyLandingAnimDone(); });
                break;

            case CatViewModel.CatMotionState.Completed:

                if (_playedCompleteOnceInCompletedState)
                    return;

                _playedCompleteOnceInCompletedState = true;
                PlayCompleteWithDelay().Forget();
                break;
            case CatViewModel.CatMotionState.Eating:
                _playedCompleteOnceInCompletedState = false;
                spinePlayer.PlayEatLoop();
                break;
        }
    }

    public void OnReachedTarget()
    {
        if (_vm.IsMoving.Value == false) return;
        _vm.NotifyArrived();
    }

    public async UniTaskVoid PlayMoveToSlotRoot(Transform targetSlotRoot, float delay)
    {
        if (_vm == null || targetSlotRoot == null)
            return;

        _vm.NotifyMoveStarted();

        if (delay > 0f)
            await UniTask.Delay((int)(delay * 1000f));

        transform.SetParent(targetSlotRoot, worldPositionStays: true);

        _moveTween?.Kill();
        _moveTween = null;

        _moveTween = transform
            .DOMove(targetSlotRoot.position, CatAnimationConfig.JumpDuration)
            .SetEase(Ease.OutQuad)
            .OnStart(() => OnJump?.Invoke())
            .OnComplete(OnReachedTarget);
    }

    private async UniTaskVoid PlayCompleteWithDelay()
    {
        var vm = _vm;
        if (vm == null) return;

        float delay = vm.MoveDelay;
        if (delay > 0f)
            await UniTask.Delay((int)(delay * 1000f));

        if (_vm == null || _vm != vm) return;

        spinePlayer.PlayComplete(() => {
            if (_vm != null) _vm.NotifyCompleteAnimDone();
        });
    }

    private void OnRemoved()
    {
        Destroy(gameObject);
    }

    public async UniTaskVoid ApplySelectedWithDelay(bool selected, float delay)
    {
        if (_vm == null) return;

        if (delay > 0f)
            await UniTask.Delay((int)(delay * 1000f));

        if (_vm == null) return;
        _vm.Selected.Value = selected;
    }
}