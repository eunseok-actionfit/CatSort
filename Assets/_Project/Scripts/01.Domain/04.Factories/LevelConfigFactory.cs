using System;


namespace Domain
{
    public static class LevelConfigFactory
    {
        public static LevelConfig CreateDefaultConfig()
        {
            // 기본 컬럼 3개, 모두 비어 있고 capacity = 4
            var columns = new[]
            {
                new ColumnConfig( ColumnId.New(), 4, Array.Empty<CatConfig>()),
                new ColumnConfig( ColumnId.New(), 4, Array.Empty<CatConfig>()),
                new ColumnConfig( ColumnId.New(), 4, Array.Empty<CatConfig>()),
            };

            return new LevelConfig(
                levelId: "New Level",
                difficulty: LevelDifficulty.Normal, 
                layoutType: BoardLayoutType.Grid,
                spacingX: 2f,
                spacingY: 5.5f,
                gridRowCount: 1,
                arcRadius: 5f,
                arcStartAngle: -45f,
                arcEndAngle: 45f,
                columns: columns
            );
        }
    }
}


