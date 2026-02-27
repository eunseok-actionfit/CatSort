using Cysharp.Threading.Tasks;
using Domain;


namespace Infrastructure
{
    public interface ISettingsRepository
    {
        UniTask<UserSettings> LoadAsync();
        UniTask SaveAsync(UserSettings data);
    }
}


