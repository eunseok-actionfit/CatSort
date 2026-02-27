using Cysharp.Threading.Tasks;
using Infrastructure.PlayerEconomy;


namespace App.Economy.Coin
{
    public sealed class AddCoinUseCase
    {
        private readonly IPlayerEconomyRepository _repo;

        public AddCoinUseCase(IPlayerEconomyRepository repo)
        {
            _repo = repo;
        }

        public async UniTask ExecuteAsync(int amount)
        {
            if (amount <= 0) return;

            var economy = await _repo.LoadAsync();
            economy.AddCoin(amount);
            await _repo.SaveAsync(economy);
        }
    }
}