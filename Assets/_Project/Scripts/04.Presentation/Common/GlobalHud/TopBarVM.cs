using System;
using AES.Tools;
using Cysharp.Threading.Tasks;


namespace Presentation
{
    public sealed class TopBarVM
    {
        public readonly Bindable<TopBarFeature> FeatureMask = new(canWrite:true);
        public readonly Bindable<int> Coin = new(canWrite:true);
        public readonly Bindable<int> Heart = new(canWrite:true);

       // public ButtonVM InviteBtn { get; }
       // public ButtonVM SnsBtn { get; }
       // public ButtonVM SettingsBtn { get; }

        public TopBarVM(
            Func<UniTask> onInvite,
            Func<UniTask> onSns,
            Func<UniTask> onSettings)
        {
            // InviteBtn = new ButtonVM(onInvite);
            // SnsBtn = new ButtonVM(onSns);
            // SettingsBtn = new ButtonVM(onSettings);
        }
    }
}