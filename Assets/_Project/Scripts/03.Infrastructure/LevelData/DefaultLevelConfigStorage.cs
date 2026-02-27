using System.Threading;
using Cysharp.Threading.Tasks;
using Domain;


public class NoopLevelConfigRepository : ILevelConfigRepository
{
    public UniTask<LevelConfig> LoadAsync(int levelId, int variant = 0, CancellationToken ct = default)
    {
        var cfg = LevelConfigFactory.CreateDefaultConfig();
        return UniTask.FromResult(cfg);
    }

    public string[] GetAllLevelIds() => null;
}