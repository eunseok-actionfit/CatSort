using AES.Tools;
using Cysharp.Threading.Tasks;
using Domain;


namespace Infrastructure
{
    public sealed class SettingsRepository : ISettingsRepository
    {
        private readonly IStorageService _storage;

        public SettingsRepository(IStorageService storage)
        {
            _storage = storage;
        }

        public async UniTask<UserSettings> LoadAsync()
        {
            var result = await _storage.LoadAsync<SettingsData>();
            var dto = result.Value ?? new SettingsData();

            return new UserSettings(
                dto.bgmVolume,
                dto.sfxVolume,
                dto.vibration,
                dto.language
            );
        }

        public async UniTask SaveAsync(UserSettings settings)
        {
            var dto = new SettingsData
            {
                bgmVolume = settings.BgmVolume,
                sfxVolume = settings.SfxVolume,
                vibration = settings.Vibration,
                language = settings.Language
            };

            await _storage.SaveAsync(dto);
        }
    }

}


