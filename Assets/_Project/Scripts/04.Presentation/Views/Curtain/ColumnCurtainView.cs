using AES.Tools;
using AES.Tools.TBC.Collections;
using App.Mvvm;
using Domain;
using UnityEngine;

public sealed class ColumnCurtainView : ContextListenerBase
{
    [Header("Roots")]
    [SerializeField] private GameObject graphicsRoot;

    [Header("Animation")]
    [SerializeField] private CurtainGimmickController curtainController;

    [Header("Curtain Sprites (SpriteRenderer)")]
    [SerializeField] private SpriteRenderer headerRenderer;
    [SerializeField] private SpriteRenderer fabricRenderer;

    [Header("Color Mapping (SO)")]
    [SerializeField] private CurtainColorTable colorTable;

    private ColumnViewModel _vm;
    private CompositeDisposable _disposables = new();

    private bool _isInitialized;
    private bool _prevIsLocked;

    private void Awake()
    {
        if(graphicsRoot != null)
            graphicsRoot.SetActive(false);
    }

    protected override void OnContextAvailable(IBindingContext ctx)
    {
        _disposables.Dispose();
        _disposables = new CompositeDisposable();

        _vm = ctx.GetValue() as ColumnViewModel;
        if (_vm == null)
            return;

        _isInitialized = false;

        ctx.SubscribeProperty<ColumnViewModel, bool>(
                this,
                vm => vm.IsLocked,
                OnLockedChanged)
           .AddTo(_disposables);

        ctx.SubscribeProperty<ColumnViewModel, CatColor>(
                this,
                vm => vm.LockColor,
                OnColorChanged)
           .AddTo(_disposables);
        
        ctx.SubscribeProperty<ColumnViewModel, ColumnBehaviorType>(
                this,
                vm => vm.BehaviorType,
                OnBehaviorTypeChanged)
            .AddTo(_disposables);
    }

    protected override void OnContextUnavailable()
    {
        _disposables.Dispose();
        _vm = null;
        _isInitialized = false;
    }
    
    

    // ------------------------------
    // Lock / Unlock 처리
    // ------------------------------
    private void OnLockedChanged(bool isLocked)
    {
        if (_vm == null)
            return;

        if (!_isInitialized)
        {
            _isInitialized = true;
            _prevIsLocked  = isLocked;

            if (graphicsRoot != null)
                graphicsRoot.SetActive(isLocked);

            if (isLocked)
                ApplyColor(_vm.LockColor.Value);

            // 초기 상태가 Hidden + Locked 이면, 시작 시점에 한 번 리셋
            if (_vm.BehaviorType.Value == ColumnBehaviorType.Hidden &&
                isLocked &&
                curtainController != null)
            {
                curtainController.ResetTimeline();
            }

            return;
        } 

        if (isLocked == _prevIsLocked)
            return;

        bool wasLocked = _prevIsLocked;
        _prevIsLocked = isLocked;

        // 잠금 → 해제 + Hidden 타입이면 오픈 연출
        if (wasLocked &&
            !isLocked &&
            _vm.BehaviorType.Value == ColumnBehaviorType.Hidden &&
            curtainController != null)
        {
            curtainController.PlayOpen();
        }

        // 해제 → 잠금일 때는 그래픽 켜고 색 맞추기
        if (!wasLocked && isLocked)
        {
            if (graphicsRoot != null)
                graphicsRoot.SetActive(true);

            ApplyColor(_vm.LockColor.Value);

            // Hidden + 다시 잠금이면 닫힌 상태로 리셋
            if (_vm.BehaviorType.Value == ColumnBehaviorType.Hidden &&
                curtainController != null)
            {
                curtainController.ResetTimeline();
            }
        }
    }
    

    // ------------------------------
    // 색 변경 → 헤더/패브릭 스프라이트 적용
    // ------------------------------
    private void OnColorChanged(CatColor color)
    {
        if (!_isInitialized || _vm == null)
            return;

        if (!_vm.IsLocked.Value)
            return;

        ApplyColor(color);
    }
    private void OnBehaviorTypeChanged(ColumnBehaviorType behaviorType)
    {
        if (_vm == null)
            return;

        // 아직 초기화 전이면, OnLockedChanged 초기화 흐름에서 처리하게 둔다
        if (!_isInitialized)
            return;

        // Hidden 으로 바뀌는 순간마다 타임라인 리셋
        if (behaviorType == ColumnBehaviorType.Hidden &&
            curtainController != null)
        {
            curtainController.ResetTimeline();

            // 숨김 연출용 그래픽 온/오프 정리 (선택 사항)
            if (graphicsRoot != null)
                graphicsRoot.SetActive(_vm.IsLocked.Value);
        }
    }
    private void ApplyColor(CatColor color)
    {
        if (colorTable == null)
            return;

        if (!colorTable.TryGet(color, out var sprites))
            return;

        if (headerRenderer != null)
            headerRenderer.sprite = sprites.header;

        if (fabricRenderer != null)
            fabricRenderer.sprite = sprites.fabric;
    }
}