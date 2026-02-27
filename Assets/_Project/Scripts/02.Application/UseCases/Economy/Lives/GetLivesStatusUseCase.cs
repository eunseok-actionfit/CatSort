using System;
using Cysharp.Threading.Tasks;
using Domain;
using Domain.Economy;
using Infrastructure.PlayerEconomy;


namespace App.Economy.Lives
{
    public sealed class GetLivesStatusUseCase
    {
        private readonly IPlayerEconomyRepository _repo;
        private readonly ITimeProvider _time;
        private readonly LivesConfig _config;

        public GetLivesStatusUseCase(
            IPlayerEconomyRepository repo,
            ITimeProvider time,
            LivesConfig config)
        {
            _repo = repo;
            _time = time;
            _config = config;
        }

        public async UniTask<LivesStatus> ExecuteAsync()
        {
            var now = DateTime.UtcNow;
            var economy = await _repo.LoadAsync();

            bool isUnlimited = economy.HasUnlimitedLives(now);
            TimeSpan unlimitedRemaining = isUnlimited
                ? economy.UnlimitedLivesEndUtc - now
                : TimeSpan.Zero;

            TimeSpan nextRegen = isUnlimited
                ? TimeSpan.Zero
                : economy.GetNextLifeRegenRemaining(now, _config.MaxLives, _config.RegenInterval);

            return new LivesStatus(
                economy.Lives,
                nextRegen,
                isUnlimited,
                unlimitedRemaining
            );
        }
    }
}