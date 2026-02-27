using AES.Tools;
using UnityEngine;


namespace Presentation.Settings
{
    public class SettingViewModel
    {
        public Command OpenPrivacyPolicyCommand { get; }
        public Command OpenTermsOfServiceCommand { get; }
        public AsyncCommand RestoreCommand { get; }

        public SettingViewModel()
        {
            OpenPrivacyPolicyCommand = new Command(
                () => Application.OpenURL(ContactUtil.Privacy_Policy_EN));
            
            OpenTermsOfServiceCommand = new Command(
                ContactUtil.Contact);

            RestoreCommand =  new AsyncCommand(
                _ => IAP.RestoreAsync(),
                _ => IAP.IsReady);
        }
    }
}