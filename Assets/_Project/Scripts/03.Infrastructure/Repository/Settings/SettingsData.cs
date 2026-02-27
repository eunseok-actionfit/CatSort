using System;
using AES.Tools;


namespace Infrastructure
{
    [Serializable]
    [SaveData("settings", UseSlot = false)]
    public sealed class SettingsData
    {
        public float bgmVolume = 0.8f;
        public float sfxVolume = 0.8f;
        public bool vibration = true;
        public string language = "ko-KR";
    }

}


