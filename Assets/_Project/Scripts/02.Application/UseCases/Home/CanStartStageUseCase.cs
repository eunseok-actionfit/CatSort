using Cysharp.Threading.Tasks;
using Domain;
using Domain.Economy;
using Domain.StageCatalog;
using Infrastructure;
using Infrastructure.PlayerEconomy;

namespace App.Home
{
    public sealed class CanStartStageUseCase 
    {
        private readonly IPlayerProgressRepository _progressRepository;
        private readonly IStageCatalog _catalog;
        private readonly IPlayerEconomyRepository _economyRepository;
        private readonly ITimeProvider _timeProvider;
        private readonly LivesConfig _config;

        public CanStartStageUseCase(
            IPlayerProgressRepository progressRepository,
            IStageCatalog catalog,
            IPlayerEconomyRepository economyRepository,
            ITimeProvider timeProvider,
            LivesConfig config)
        {
            _progressRepository = progressRepository;
            _catalog            = catalog;
            _economyRepository  = economyRepository;
            _timeProvider       = timeProvider;
            _config             = config;
        }

        public async UniTask<bool> CanStartAsync(int stageId)
        {
            // 1. 카탈로그 상 유효한 스테이지인지
            if (!_catalog.IsValid(stageId))
                return false;

            // 2. 플레이어 진행도 상 해금 여부
            var progress = await _progressRepository.LoadAsync();
            if (!progress.IsStageUnlocked(stageId))
                return false;

            // 3. 라이프 갱신 후 1개 이상인지
            var economy = await _economyRepository.LoadAsync();

            economy.TickLives(
                _timeProvider.UtcNow,
                _config.MaxLives,
                _config.RegenInterval
            );
            await _economyRepository.SaveAsync(economy);

            return economy.Lives >= 0;
        }
    }
}