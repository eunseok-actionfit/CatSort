using System;
using Cysharp.Threading.Tasks;
using Domain;
using Domain.Economy;
using Infrastructure.PlayerEconomy;


namespace App.Economy.Lives
{
    public sealed class ConsumeLifeForStageUseCase
    {
        private readonly IPlayerEconomyRepository _economyRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly LivesConfig _config;

        public ConsumeLifeForStageUseCase(
            IPlayerEconomyRepository economyRepository,
            ITimeProvider timeProvider,
            LivesConfig config)
        {
            _economyRepository = economyRepository;
            _timeProvider      = timeProvider;
            _config            = config;
        }

        /// <summary>
        /// 라이프 1개 소비 시도. 실패 시 false.
        /// </summary>
        public async UniTask<bool> ExecuteAsync()
        {
            var economy = await _economyRepository.LoadAsync();
            
            if (economy.HasUnlimitedLives(DateTime.UtcNow))
                return true;

            var success = economy.TryConsumeLife(
                _timeProvider.UtcNow,
                _config.MaxLives,
                _config.RegenInterval,
                1
            );

            if (!success)
                return false;

            await _economyRepository.SaveAsync(economy);
            return true;
        }
    }
}