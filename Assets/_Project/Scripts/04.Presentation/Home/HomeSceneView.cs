using AES.Tools;
using AES.Tools.TBC.Collections;
using AES.Tools.UI.Managers;
using Presentation.Popups;
using UnityEngine;


namespace Presentation
{
    public class HomeSceneView : ContextListenerBase
    {
        [SerializeField] private PopupManager popupManager;

        private HomeSceneViewModel _vm;
        private CompositeDisposable _disposables = new();

        protected override void OnContextAvailable(IBindingContext context)
        {
            _disposables.Dispose();
            _disposables = new CompositeDisposable();

            _vm = context.GetValue() as HomeSceneViewModel;
            if (_vm == null)
                return;

            _vm.OpenPopupRequested += ShowOpenPopupOpen;
        }

        protected override void OnContextUnavailable()
        {
            _disposables.Dispose();

            if (_vm != null)
                _vm.OpenPopupRequested -= ShowOpenPopupOpen;
        }


        private async void ShowOpenPopupOpen()
        {
            var popup = await popupManager.EnqueueAsync<LivesRefillPopup>(_vm);
        }
    }
}