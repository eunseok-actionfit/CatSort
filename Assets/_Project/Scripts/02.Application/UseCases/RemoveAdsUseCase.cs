// RemoveAdsUseCase.cs
using AES.Tools;
using AES.Tools.VContainer;
using Cysharp.Threading.Tasks;
using Infrastructure.PlayerEconomy;


namespace App.Ads
{
    public sealed class RemoveAdsUseCase
    {
        private readonly IPlayerEconomyRepository _repo;

        public RemoveAdsUseCase(IPlayerEconomyRepository repo)
        {
            _repo = repo;
        }

        public async UniTask ExecuteAsync()
        {
            var eco = await _repo.LoadAsync();
            eco.SetRemoveAds(true);
            
            await _repo.SaveAsync(eco);
            
            ADS.SetRuntimeDisabled(true);
        }
    }
}