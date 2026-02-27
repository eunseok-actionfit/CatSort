using Cysharp.Threading.Tasks;
using Domain.Booster;

namespace Infrastructure.Booster
{
    public interface IBoosterInventoryRepository
    {
        UniTask<BoosterInventory> LoadAsync();
        UniTask SaveAsync(BoosterInventory inv);
    }
}