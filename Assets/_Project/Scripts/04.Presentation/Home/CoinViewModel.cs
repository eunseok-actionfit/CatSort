using AES.Tools;
using Cysharp.Threading.Tasks;
using App.Economy;
using App.Economy.Coin;


namespace Presentation
{
    public sealed class CoinViewModel
    {
        public ObservableProperty<long> Coin { get; } = new();

        private readonly GetCoinStatusUseCase _getCoinStatusUseCase;

        public CoinViewModel(GetCoinStatusUseCase getCoinStatusUseCase)
        {
            _getCoinStatusUseCase = getCoinStatusUseCase;
            RefreshAsync().Forget();
        }

        public async UniTaskVoid RefreshAsync()
        {
            Coin.Value = await _getCoinStatusUseCase.ExecuteAsync();
        }
    }
}