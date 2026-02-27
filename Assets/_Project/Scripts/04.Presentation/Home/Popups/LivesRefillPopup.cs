using AES.Tools.UI.Core.View;


namespace Presentation.Popups
{
    public class LivesRefillPopup : PopupViewBase
    {
        private HomeSceneViewModel _vm;

        public override void BindModelObject(object viewModel)
        {
            base.BindModelObject(viewModel);

            // 기존 구독 제거
            if (_vm != null)
                _vm.ClosePopupRequested -= On;

            _vm = viewModel as HomeSceneViewModel;
            if (_vm != null)
                _vm.ClosePopupRequested += On;
        }

        private void OnDestroy()
        {
            if (_vm != null)
                _vm.ClosePopupRequested -= On;
        }

        private void On()
        {
            // UIView.Hide() -> OnHideCompleted -> PopupManager.Instance.OnPopupClosed(this)
            Hide();
        }
    }
}