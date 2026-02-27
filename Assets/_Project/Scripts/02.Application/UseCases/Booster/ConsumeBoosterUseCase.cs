using Cysharp.Threading.Tasks;
using Infrastructure.Booster;

namespace App.Booster
{
    public sealed class ConsumeBoosterUseCase
    {
        private readonly IBoosterInventoryRepository _repo;

        public ConsumeBoosterUseCase(IBoosterInventoryRepository repo)
        {
            _repo = repo;
        }

        public async UniTask<bool> ExecuteAsync(BoosterId id, int amount = 1)
        {
            var inv = await _repo.LoadAsync();
            var ok = inv.TryConsume(id, amount);
            if (!ok)
                return false;

            await _repo.SaveAsync(inv);
            return true;
        }
    }
}