using Cysharp.Threading.Tasks;
using Domain;
using Domain.Economy;
using Infrastructure.PlayerEconomy;


namespace App.Economy.Lives
{
    public sealed class RefreshLivesUseCase
    {
        private readonly IPlayerEconomyRepository _economyRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly LivesConfig _config;

        public RefreshLivesUseCase(
            IPlayerEconomyRepository economyRepository,
            ITimeProvider timeProvider,
            LivesConfig config)
        {
            _economyRepository = economyRepository;
            _timeProvider      = timeProvider;
            _config            = config;
        }

        public async UniTask<PlayerEconomy> ExecuteAsync()
        {
            var economy = await _economyRepository.LoadAsync();

            economy.TickLives(
                _timeProvider.UtcNow,
                _config.MaxLives,
                _config.RegenInterval
            );

            await _economyRepository.SaveAsync(economy);
            return economy;
        }
    }
}