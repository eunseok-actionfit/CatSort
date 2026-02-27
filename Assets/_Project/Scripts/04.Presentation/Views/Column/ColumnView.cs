using AES.Tools;
using AES.Tools.TBC.Collections;
using App.Mvvm;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Domain;
using Presentation.Slot;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Presentation.Mvvm
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MonoContext))]
    public sealed class ColumnView : ContextListenerBase, IColumnIndexProvider
    {
        //======================================================================
        // Inspector
        //======================================================================

        [Header("Layout")]
        [SerializeField] private ColumnOrientation orientation = ColumnOrientation.Vertical;
        [SerializeField, Min(0.05f)] private float slotStep = 1.2f;

        [Header("Padding / Collider")]
        [SerializeField] private BoxCollider2D hitCollider;
        [SerializeField, Min(0f)] private float paddingTop = 1f;
        [SerializeField, Min(0f)] private float paddingBottom = 1.1f;
        [SerializeField, Min(0f)] private float paddingLeft = 0.2f;
        [SerializeField, Min(0f)] private float paddingRight = 0.2f;

        [Header("Visual")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private SpriteRenderer highlightBg;

        //======================================================================
        // 내부 상태
        //======================================================================

        private ColumnViewModel _vm;
        private CompositeDisposable _disposables = new CompositeDisposable();

        private EventBinding<CatMoveRequested> _moveBinding;
        private EventBinding<CatSelectRequested> _selectBinding;


        // SlotView 캐싱 (GetSlotView 성능 개선용)
        private SlotView[] _cachedSlotViews;

        //======================================================================
        // 인터페이스 구현
        //======================================================================

        public int ColumnIndex => _vm?.ColumnIndex ?? -1;

        


        //======================================================================
        // Unity
        //======================================================================

        private void Awake()
        {
            if (visualRoot == null)
                visualRoot = transform;
        }

        private void OnDestroy()
        {
            // 뷰 파괴 시 트윈 정리 (옵션)
            if (visualRoot != null)
                visualRoot.DOKill();

            _moveBinding?.Deregister();
            _moveBinding = null;
            
            _selectBinding?.Deregister();
            _selectBinding = null;
        }


        //======================================================================
        // ContextListenerBase
        //======================================================================

        protected override void OnContextAvailable(IBindingContext ctx)
        {
            _disposables.Dispose();
            _disposables = new CompositeDisposable();

            _vm = ctx.GetValue() as ColumnViewModel;
            if (_vm == null)
                return;

            _moveBinding?.Deregister();
            _moveBinding = new EventBinding<CatMoveRequested>(OnCatMoveRequested)
            {
                Owner = this,
                Name = $"ColumnView[{_vm.ColumnIndex}] CatMoveRequested"
            }.Register();
            
            _selectBinding?.Deregister();
            _selectBinding = new EventBinding<CatSelectRequested>(OnCatSelectRequested)
            {
                Owner = this,
                Name = $"ColumnView[{_vm.ColumnIndex}] CatSelectRequested"
            }.Register();

            // 선택 상태 → 하이라이트
            ctx.SubscribeProperty<ColumnViewModel, bool>(
                    this,
                    vm => vm.Selected,
                    selected => {
                        SetHighlight(selected);

                        // 선택 해제 시 다음 선택에서 새로 뽑도록 리셋
                        if (!selected && _vm != null)
                            _vm.JumpSeed.Value = -1;
                    })
                .AddTo(_disposables);

        }
        


        protected override void OnContextUnavailable()
        {
            _disposables.Dispose();
            _disposables = new CompositeDisposable();
            _vm = null;
        }

        //======================================================================
        // Public API
        //======================================================================

        public void SetHighlight(bool on)
        {
            if (highlightBg != null)
                highlightBg.enabled = on;
        }

        /// <summary>
        /// 현재 컬럼에서 slotIndex에 해당하는 SlotView를 반환한다.
        /// 내부적으로 캐시를 사용하여 빈번 호출 시 성능을 개선한다.
        /// </summary>
        public SlotView GetSlotView(int slotIndex)
        {
            if (slotIndex < 0)
                return null;

            EnsureSlotViewCache();

            for (int i = 0; i < _cachedSlotViews.Length; i++)
            {
                var slot = _cachedSlotViews[i];
                if (slot == null) continue;
                if (slot.SlotIndex == slotIndex)
                    return slot;
            }

            return null;
        }

        /// <summary>
        /// 슬롯 VM만 재배치: LocalPosition + VisualKind 설정
        /// </summary>
        public void RelayoutSlots()
        {
            if (_vm == null)
                return;

            int count = _vm.Slots.Count;

            for (int i = 0; i < count; i++)
            {
                var pos = orientation == ColumnOrientation.Vertical
                    ? new Vector3(0f, i * slotStep, 0f)
                    : new Vector3(i * slotStep, 0f, 0f);

                var slotVm = _vm.Slots[i];
                slotVm.LocalPosition.Value = pos;

                var kind = ResolveVisualKind(i, count, orientation);
                slotVm.VisualKind.Value = kind;
            }

            AdjustBackgroundAndCollider(count);
        }

        /// <summary>
        /// 컬럼 단위 Jump 세트 랜덤 결정
        /// </summary>
        public void AssignRandomJumpSet(int maxSetCount)
        {
            if (_vm == null)
                return;

            if (maxSetCount <= 0)
            {
                _vm.JumpSeed.Value = -1;
                return;
            }

            _vm.JumpSeed.Value = Random.Range(0, maxSetCount);
        }

        /// <summary>
        /// 잘못된 이동 FX용 (컬럼 전체 시퀀스 살짝 흔들기)
        /// </summary>
        public void PlayInvalidMoveFx()
        {
            if (visualRoot == null)
                visualRoot = transform;

            visualRoot
                .DOShakePosition(0.15f, 0.2f, 20)
                .SetUpdate(true);
        }

        //======================================================================
        // 내부 유틸
        //======================================================================

        private void EnsureSlotViewCache()
        {
            if (_cachedSlotViews != null && _cachedSlotViews.Length > 0)
                return;

            _cachedSlotViews = GetComponentsInChildren<SlotView>(includeInactive: true);
        }

        private void AdjustBackgroundAndCollider(int slotCount)
        {
            if (hitCollider == null || slotCount <= 0)
                return;

            bool vertical = orientation == ColumnOrientation.Vertical;
            float slotLength = (slotCount - 1) * slotStep;

            float width;
            float height;
            Vector2 offset;

            if (vertical)
            {
                height = slotLength + paddingTop + paddingBottom;
                width = 1f + paddingLeft + paddingRight;

                float centerX = (paddingRight - paddingLeft) * 0.5f;
                float centerY = (height * 0.5f) - paddingBottom;
                offset = new Vector2(centerX, centerY);
            }
            else
            {
                width = slotLength + paddingLeft + paddingRight;
                height = 1f + paddingTop + paddingBottom;

                float centerX = (width * 0.5f) - paddingLeft;
                float centerY = (paddingTop - paddingBottom) * 0.5f;
                offset = new Vector2(centerX, centerY);
            }

            hitCollider.size = new Vector2(width, height);
            hitCollider.offset = offset;
        }

        private static SlotVisualKind ResolveVisualKind(int index, int length, ColumnOrientation orientation)
        {
            if (orientation == ColumnOrientation.Horizontal) return SlotVisualKind.Top;
            if (length <= 0) return SlotVisualKind.Middle;
            if (index == 0) return SlotVisualKind.Bottom;
            if (index == length - 1) return SlotVisualKind.Top;
            return SlotVisualKind.Middle;
        }

        

        private void OnCatMoveRequested(CatMoveRequested e)
        {
            if (_vm == null) return;
            if (e.To.ColumnIndex != _vm.ColumnIndex) return;

            var slot = GetSlotView(e.To.SlotIndex);
            if (slot == null) return;

            var vm = _vm.Slots[e.To.SlotIndex].Cat.Value;
            if (vm == null || !vm.CatId.Equals(e.CatId)) return;

            if (!CatViewRegistry.TryGet(vm, out var catView) || catView == null)
                return;

            catView.PlayMoveToSlotRoot(slot.CatRoot, e.Delay).Forget();
        }
        
        private void OnCatSelectRequested(CatSelectRequested e)
        {
            if (_vm == null) return;

            // 해당 Cat이 이 Column에 존재할 때만 처리 (CatId->VM 검색)
            CatViewModel vm = null;
            for (int i = 0; i < _vm.Slots.Count; i++)
            {
                var c = _vm.Slots[i].Cat.Value;
                if (c != null && c.CatId.Equals(e.CatId))
                {
                    vm = c;
                    break;
                }
            }
            if (vm == null) return;

            if (!CatViewRegistry.TryGet(vm, out var view) || view == null)
                return;

            view.ApplySelectedWithDelay(e.Selected, e.Delay).Forget();
        }
    }
}