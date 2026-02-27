using Cysharp.Threading.Tasks;
using Infrastructure;


namespace App.Gameplay
{
    public sealed class SaveLevelClearProgressUseCase
    {
        private readonly IPlayerProgressRepository _progressRepository;

        public SaveLevelClearProgressUseCase(IPlayerProgressRepository progressRepository)
        {
            _progressRepository = progressRepository;
        }

        public async UniTask ExecuteAsync(int levelId)
        {
            var progress = await _progressRepository.LoadAsync();

            if (levelId > progress.LastClearedLevel)
            {
                progress.UpdateLastCleared(levelId);
                await _progressRepository.SaveAsync(progress);
            }
        }
    }
}