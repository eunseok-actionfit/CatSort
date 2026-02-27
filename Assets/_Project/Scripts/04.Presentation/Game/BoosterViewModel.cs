using AES.Tools;
using Cysharp.Threading.Tasks;
using App.Booster;


namespace Presentation
{
    public sealed class BoosterViewModel
    {
        public BoosterId Id { get; }

        public ObservableProperty<int> Count { get; } = new();

        private readonly GetBoosterCountUseCase _getCountUseCase;

        public BoosterViewModel(
            BoosterId id,
            GetBoosterCountUseCase getCountUseCase)
        {
            Id = id;
            _getCountUseCase = getCountUseCase;

            RefreshAsync().Forget();
        }

        public async UniTask RefreshAsync()
        {
            Count.Value = await _getCountUseCase.ExecuteAsync(Id);
        }
    }
}