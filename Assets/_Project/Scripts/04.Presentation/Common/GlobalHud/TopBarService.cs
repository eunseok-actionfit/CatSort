// App.Services.UX.Hud.TopBar
using System.Threading;
using AES.Tools.UI_Regacy_.Core.Controller;
using Cysharp.Threading.Tasks;


namespace Presentation
{
    public sealed class TopBarService : ITopBar
    {
        readonly TopBarVM _vm;
        readonly IUIController _ui;

        public TopBarService(TopBarVM vm, IUIController ui)
        {
            _vm = vm;
            _ui = ui;
        }

        public void ShowMask(TopBarFeature mask) => _vm.FeatureMask.Value = mask;
    
        public UniTask ShowAsync(CancellationToken ct = default)
            => _ui.ShowAsync(UIWIndowId.TopBar, _vm, ct);
    }
}