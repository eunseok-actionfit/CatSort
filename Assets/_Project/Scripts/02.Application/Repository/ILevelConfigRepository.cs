using System.Threading;
using Cysharp.Threading.Tasks;


namespace Domain
{
    public interface ILevelConfigRepository
    {
        UniTask<LevelConfig> LoadAsync(int levelId, int variant = 0, CancellationToken ct = default);
    }
}