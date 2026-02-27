using AES.Tools;
using AES.Tools.TBC.Collections;
using App.Mvvm;
using DG.Tweening;
using Presentation.Slot;
using UnityEngine;

namespace Presentation.Mvvm
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MonoContext))]
    public sealed class SlotView : ContextListenerBase
    {
        [Header("Visual")]
        [SerializeField] private SpriteRenderer bg;
        [SerializeField] private SpriteRenderer icon;
        [SerializeField] private SlotSpriteSet spriteSet;

        [Header("Cat")]
        [SerializeField] private MonoContext catPrefab;
        [SerializeField] private Transform catRoot;
        
        public Transform CatRoot => catRoot;

        private SlotViewModel _vm;
        private CompositeDisposable _disposables = new();

        private Color _defaultBgColor = Color.white;

        public int SlotIndex
        {
            get
            {
                if (_vm == null)
                    return -1;

                return _vm.SlotIndex;
            }
        }

        private void Awake()
        {
            if (catRoot == null)
                catRoot = transform;

            if (bg != null)
                _defaultBgColor = bg.color;
        }

        protected override void OnContextAvailable(IBindingContext ctx)
        {
            _disposables.Dispose();
            _disposables = new CompositeDisposable();

            _vm = ctx.GetValue() as SlotViewModel;
            if (_vm == null)
                return;

            // 슬롯 비주얼 (Top/Middle/Bottom 등)
            ctx.SubscribeProperty<SlotViewModel, SlotVisualKind>(
                    vm => vm.VisualKind,
                    SetupVisual)
                .AddTo(_disposables);

            // 하이라이트
            ctx.SubscribeProperty<SlotViewModel, bool>(
                    vm => vm.Highlight,
                    SetHighlight)
                .AddTo(_disposables);

            // 락 상태
            ctx.SubscribeProperty<SlotViewModel, bool>(
                    vm => vm.Locked,
                    SetLocked)
                .AddTo(_disposables);

            // 슬롯에 Cat이 들어올 때 처리
            ctx.SubscribeProperty<SlotViewModel, CatViewModel>(
                    vm => vm.Cat,
                    OnCatChanged)
                .AddTo(_disposables);
            
            ctx.SubscribeProperty<SlotViewModel, bool>(
                    vm => vm.IsDimmed,
                    OnCompleted)
                .AddTo(_disposables);
        }

        private void OnCompleted(bool completed)
        {
            var targetColor = completed ? Color.gray7 : Color.white;
            bg.DOColor(targetColor, 1.5f)
                .SetEase(Ease.OutExpo);
        }

        protected override void OnContextUnavailable()
        {
            _disposables.Dispose();
            _disposables = new CompositeDisposable();
            _vm = null;

            // 기본 상태로 복구
            if (bg != null)
                bg.color = _defaultBgColor;
        }

        private void OnCatChanged(CatViewModel catVm)
        {
            if (catVm == null)
            {
                // 필요하면 여기서 기존 CatView 정리
                return;
            }

            var catView = CatViewRegistry.GetOrCreate(catVm, catPrefab, catRoot);
            if (catView == null)
            {
                Debug.LogWarning($"CatView 생성 실패: CatId={catVm.CatId}");
                return;
            }

            catView.transform.SetParent(catRoot, worldPositionStays: true);

            // var targetPos = catRoot.localPosition;
            // catVm.RequestMove(targetPos);
        }

        public void SetupVisual(SlotVisualKind kind)
        {
            if (bg == null || spriteSet == null)
                return;

            bg.sprite = spriteSet.GetSprite(false, kind);
            bg.color = _defaultBgColor;
        }

        public void SetHighlight(bool on)
        {
            if (bg == null)
                return;

            // 락 상태면 하이라이트 무시
            if (_vm != null && _vm.Locked.Value)
                return;

            bg.color = on ? new Color(1f, 1f, 0.8f, 1f) : _defaultBgColor;
        }

        private void SetLocked(bool locked)
        {
            if (bg == null)
                return;

            // 간단히: 락이면 살짝 어둡게
            if (locked)
            {
                bg.color = Color.gray;
            }
            else
            {
                // 락 해제 시, 현재 하이라이트 여부에 맞게 색 복원
                bool highlighted = _vm != null && _vm.Highlight.Value;
                SetHighlight(highlighted);
            }

            // 필요하면 icon.enabled, 별도 락 아이콘 등 여기서 처리
        }
        
    }
}
