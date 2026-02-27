using System;
using AES.Tools;
using Cysharp.Threading.Tasks;
using App.Economy.Lives;

namespace Presentation
{
    public sealed class LivesViewModel : IDisposable
    {
        public ObservableProperty<int> CurrentLives { get; } = new();
        public ObservableProperty<TimeSpan> NextRegenRemaining { get; } = new();
        public ObservableProperty<bool> IsInfinityMode { get; } = new();

        private readonly GetLivesStatusUseCase _getLivesStatusUseCase;
        private bool _isRunning;

        public LivesViewModel(GetLivesStatusUseCase getLivesStatusUseCase)
        {
            _getLivesStatusUseCase = getLivesStatusUseCase;
            StartLoop().Forget();
        }

        private async UniTaskVoid StartLoop()
        {
            _isRunning = true;

            await UpdateOnce();

            while (_isRunning)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1));
                await UpdateOnce();
            }
        }

        private async UniTask UpdateOnce()
        {
            var status = await _getLivesStatusUseCase.ExecuteAsync();

            CurrentLives.Value = status.Lives;
            NextRegenRemaining.Value = status.NextRegenRemaining;
            IsInfinityMode.Value = status.IsUnlimited;
        }

        public UniTask RefreshOnceAsync() => UpdateOnce();

        public void Dispose()
        {
            _isRunning = false;
        }
    }
}