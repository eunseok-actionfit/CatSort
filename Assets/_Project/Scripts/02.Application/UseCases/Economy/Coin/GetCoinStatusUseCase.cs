using Cysharp.Threading.Tasks;
using Infrastructure.PlayerEconomy;


namespace App.Economy.Coin
{
    public sealed  class GetCoinStatusUseCase
    {
        private readonly IPlayerEconomyRepository _repo;

        public GetCoinStatusUseCase(IPlayerEconomyRepository repo)
        {
            _repo = repo;
        }

        public async UniTask<long> ExecuteAsync()
        {
            var economy = await _repo.LoadAsync();
            return economy.Coin;
        }
    }
}