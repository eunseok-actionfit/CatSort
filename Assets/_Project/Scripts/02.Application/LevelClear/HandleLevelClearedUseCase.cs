using App.Gameplay;
using Cysharp.Threading.Tasks;
using Domain;


namespace App
{
    public sealed class HandleLevelClearedUseCase
    {
        private readonly SaveLevelClearProgressUseCase _saveClearProgress;
        private readonly GrantLevelClearRewardsUseCase _grantRewards;

        public HandleLevelClearedUseCase(
            SaveLevelClearProgressUseCase saveClearProgress,
            GrantLevelClearRewardsUseCase grantRewards)
        {
            _saveClearProgress = saveClearProgress;
            _grantRewards = grantRewards;
        }

        public async UniTask<Results.HandleLevelClearedResult> ExecuteAsync(int levelId, LevelDifficulty difficulty)
        {
            await _saveClearProgress.ExecuteAsync(levelId);

            var reward = await _grantRewards.ExecuteAsync(difficulty);

            return new Results.HandleLevelClearedResult(reward.Coin, reward.Lives);
            // 필요하면 boosters도 결과에 포함시키면 됨
        }
    }
}