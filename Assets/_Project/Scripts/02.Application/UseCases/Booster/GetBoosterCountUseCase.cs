using Cysharp.Threading.Tasks;
using Infrastructure.Booster;


namespace App.Booster
{
    public sealed class GetBoosterCountUseCase
    {
        private readonly IBoosterInventoryRepository _repo;

        public GetBoosterCountUseCase(IBoosterInventoryRepository repo)
        {
            _repo = repo;
        }

        public async UniTask<int> ExecuteAsync(BoosterId id)
        {
            var inv = await _repo.LoadAsync();
            return inv.GetCount(id);
        }
    }
}