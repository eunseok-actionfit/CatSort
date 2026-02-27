namespace Domain
{
    public sealed class UserSettings
    {
        public float BgmVolume { get; private set; }
        public float SfxVolume { get; private set; }
        public bool Vibration { get; private set; }
        public string Language { get; private set; }

        public UserSettings(
            float bgmVolume,
            float sfxVolume,
            bool vibration,
            string language)
        {
            BgmVolume = bgmVolume;
            SfxVolume = sfxVolume;
            Vibration = vibration;
            Language = language;
        }

        public void SetBgm(float v) => BgmVolume = Clamp01(v);
        public void SetSfx(float v) => SfxVolume = Clamp01(v);
        public void SetVibration(bool v) => Vibration = v;
        public void SetLanguage(string lang) => Language = lang;

        private static float Clamp01(float value)
        {
            if ((double)value < 0.0)
                return 0.0f;

            return (double)value > 1.0 ? 1f : value;
        }
    }
}