using AES.Tools;
using App.Mvvm;
using UnityEngine;


namespace Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MonoContext))]
    public class LevelEditorView : MonoBehaviour
    {
        private MonoContext _context;
        public LevelEditorViewModel _viewModel;
        public BoardCameraController cameraController;
        private void Awake()
        {
            if (_context == null)
                _context = GetComponent<MonoContext>();
        }
        
        
        public void SetViewModel(LevelEditorViewModel vm)
        {
            if (_context == null)
                _context = GetComponent<MonoContext>();

            _context.SetViewModel(vm);

            _viewModel = vm;
            _viewModel.RefreshPreviewRequest += RefreshPreview;
        }

        private void OnDestroy()
        {
            _viewModel.RefreshPreviewRequest -= RefreshPreview;
        }

        private void RefreshPreview() => cameraController.FitToBoard();
    }
}


