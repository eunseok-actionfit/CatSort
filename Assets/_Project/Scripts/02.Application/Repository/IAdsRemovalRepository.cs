using Cysharp.Threading.Tasks;

namespace Domain.Ads
{
    public interface IAdsRemovalRepository
    {
        UniTask<bool> GetRemovedAsync();
        UniTask SetRemovedAsync(bool removed);
    }
}