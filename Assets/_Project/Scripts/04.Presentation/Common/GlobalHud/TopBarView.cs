using AES.Tools.UI.Core.View;
using UnityEngine;


namespace Presentation
{
    public sealed class TopBarView : UIView
    {
        // [Header("Groups")]
        // [SerializeField] GameObject coinGroup;
        // [SerializeField] GameObject heartGroup;
        // [SerializeField] GameObject inviteGroup;
        // [SerializeField] GameObject snsGroup;
        // [SerializeField] GameObject settingsGroup;
        //
        // [Header("Texts")]
        // [SerializeField] TMP_Text coinText;
        // [SerializeField] TMP_Text heartText;
        //
        // [Header("Buttons")]
        // [SerializeField] AESButton inviteBtn;
        // [SerializeField] AESButton snsBtn;
        // [SerializeField] AESButton settingsBtn;

        // protected override void Bind(TopBarVM vm)
        // {
        //     // 숫자 바인딩
        //     Binder.BindText(coinText,  vm.Coin,  "{0}", true);
        //     Binder.BindText(heartText, vm.Heart, "{0}", true);
        //
        //     // 버튼 바인딩
        //     inviteBtn.Initialize(vm.InviteBtn);
        //     snsBtn.Initialize(vm.SnsBtn);
        //     settingsBtn.Initialize(vm.SettingsBtn);
        //
        //     // 마스크 변경 시 UI 토글
        //     vm.FeatureMask.SubscribeAndReplay(ApplyMask, true);
        // }

        void ApplyMask(TopBarFeature m)
        {
            // Set(coinGroup,   m.Has(TopBarFeature.Coin));
            // Set(heartGroup,  m.Has(TopBarFeature.Heart));
            // Set(inviteGroup, m.Has(TopBarFeature.Invite));
            // Set(snsGroup,    m.Has(TopBarFeature.Sns));
            // Set(settingsGroup, m.Has(TopBarFeature.Settings));
        }

        static void Set(GameObject go, bool on) { if (go) go.SetActive(on); }
    }

    static class TopBarFeatureExt
    {
        public static bool Has(this TopBarFeature mask, TopBarFeature f) => (mask & f) != 0;
    }
}