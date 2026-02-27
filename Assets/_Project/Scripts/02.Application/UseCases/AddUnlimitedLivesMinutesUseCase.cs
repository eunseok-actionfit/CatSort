using System;
using Cysharp.Threading.Tasks;
using Domain.Economy;
using Infrastructure.PlayerEconomy;


namespace App.Economy.Lives
{
    public sealed class AddUnlimitedLivesMinutesUseCase
    {
        private readonly IPlayerEconomyRepository _repo;
        private readonly LivesConfig _config;

        public AddUnlimitedLivesMinutesUseCase(IPlayerEconomyRepository repo, LivesConfig config)
        {
            _repo = repo;
            _config = config;
        }

        public async UniTask ExecuteAsync(int minutes)
        {
            if (minutes <= 0) return;

            var eco = await _repo.LoadAsync();
            eco.AddUnlimitedLivesMinutes(minutes, DateTime.UtcNow);
            await _repo.SaveAsync(eco);
        }
    }
}