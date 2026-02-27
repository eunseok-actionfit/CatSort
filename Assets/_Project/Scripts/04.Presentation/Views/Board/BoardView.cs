using System.Collections.Generic;
using AES.Tools;
using AES.Tools.TBC.Collections;
using App;
using App.Mvvm;
using UnityEngine;


namespace Presentation.Mvvm
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MonoContext))]
    public sealed class BoardView : ContextListenerBase
    {
        [Header("Column")]
        [SerializeField] private Transform columnRoot;

        [Header("Camera")]
        public BoardCameraController cameraController;

        private readonly List<ColumnView> _columns = new();

        private BoardViewModel _vm;
        private CompositeDisposable _disposables = new();

        #region Unity

        private void Awake()
        {
            if (columnRoot == null)
                columnRoot = transform;

            if (cameraController != null)
                cameraController.FitToBoard();
            
            RefreshColumnsCache();
        }

        private void RefreshColumnsCache()
        {
            _columns.Clear();

            if (columnRoot == null)
                columnRoot = transform;

            var cols = columnRoot.GetComponentsInChildren<ColumnView>(includeInactive: true);
            _columns.AddRange(cols);
        }

        public ColumnView GetColumnView(int columnIndex)
        {
            if (_columns.Count == 0)
                RefreshColumnsCache();

            for (int i = 0; i < _columns.Count; i++)
            {
                var col = _columns[i];
                if (col != null && col.ColumnIndex == columnIndex)
                    return col;
            }

            return null;
        }

        public SlotView GetSlotView(ViewSlotAddr addr)
        {
            var col = GetColumnView(addr.ColumnIndex);
            if (col == null) return null;

            return col.GetSlotView(addr.SlotIndex);
        }

        #endregion

        #region ContextListenerBase

        protected override void OnContextAvailable(IBindingContext ctx)
        {
            _disposables.Dispose();
            _disposables = new CompositeDisposable();

            _vm = ctx.GetValue() as BoardViewModel;
            if (_vm == null)
                return;

            // 2) 컬럼 리스트 변경될 때마다 카메라 맞추기
            _vm.Columns.OnListChanged += OnColumnsChanged;
            _disposables.Add(Disposable.Create(() => { _vm.Columns.OnListChanged -= OnColumnsChanged; }));
        }

        protected override void OnContextUnavailable()
        {
            _disposables.Dispose();
            _disposables = new CompositeDisposable();
            _vm = null;
        }

        #endregion

        #region Camera

        private void OnColumnsChanged()
        {
            FitCamera();
        }

        private void FitCamera()
        {
            if (cameraController == null)
                return;

            cameraController.FitToBoard();
        }

        #endregion
    }
}