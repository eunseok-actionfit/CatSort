using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Domain;


namespace Infrastructure
{
    public sealed class SoLevelConfigRepository : ILevelConfigRepository
    {
        private readonly LevelDatabaseSO _database;

        public SoLevelConfigRepository(LevelDatabaseSO database)
        {
            _database = database;
        }

        public LevelConfig GetLevelConfig(int levelId)
        {
            var asset = _database.GetLevelAsset(levelId);
            if (asset == null)
                throw new System.Exception($"Level {levelId} not found in database.");

            var columns = asset.columns
                .Select(c =>
                {
                    var cats = c.catsFromBottomToTop
                        .Select(cat => new CatConfig(cat.color, cat.isHidden))
                        .ToArray();
                    

                    return new ColumnConfig(
                        ColumnId.New(), 
                        c.capacity,
                        cats,
                        c.behaviorType,
                        c.requiredColor
                    );
                })
                .ToArray();

            return new LevelConfig(
                asset.levelId.ToString(),
                LevelDifficulty.Normal,  
                asset.layoutType,
                asset.gridRowCount,
                asset.spacingX,
                asset.spacingY,
                asset.arcStartAngle,
                asset.arcEndAngle,
                asset.arcRadius,
                columns
            );
        }

        public UniTask<LevelConfig> LoadAsync(int levelId, int variant = 0, CancellationToken ct = default)
        {
            var asset = _database.GetLevelAsset(levelId);
            if (asset == null)
                throw new System.Exception($"Level {levelId} not found in database.");

            var columns = asset.columns
                .Select(c =>
                {
                    var cats = c.catsFromBottomToTop
                        .Select(cat => new CatConfig(cat.color, cat.isHidden))
                        .ToArray();
                    

                    return new ColumnConfig(
                        ColumnId.New(), 
                        c.capacity,
                        cats,
                        c.behaviorType,
                        c.requiredColor
                    );
                })
                .ToArray();

            var config = new LevelConfig(
                asset.levelId.ToString(),
                LevelDifficulty.Normal,  
                asset.layoutType,
                asset.gridRowCount,
                asset.spacingX,
                asset.spacingY,
                asset.arcStartAngle,
                asset.arcEndAngle,
                asset.arcRadius,
                columns
            );
            
            return UniTask.FromResult(config);
        }
    }
}