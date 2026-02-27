using System;
using System.Collections.Generic;
using System.Linq;
using Domain;


namespace App
{
    public static class LevelConfigMapper
    {
        public static BoardLayoutDto ToLayoutDto(LevelConfig config)
        {
            return new BoardLayoutDto
            {
                LayoutType = config.LayoutType,
                GridRowCount = config.GridRowCount,
                SpacingX = config.SpacingX,
                SpacingY = config.SpacingY,
                ArcStartAngle = config.ArcStartAngle,
                ArcEndAngle = config.ArcEndAngle,
                ArcRadius = config.ArcRadius
            };
        }
        
        public static Board ToBoard(LevelConfig config)
        {
            var columns = new List<Column>();

            for (int i = 0; i < config.Columns.Length; i++)
            {
                var colCfg = config.Columns[i];
                var id = new ColumnId(Guid.NewGuid());

                // CatConfig → Cat
                var cats = colCfg.CatsFromBottomToTop
                    .Select(catCfg =>
                    {
                        var cat = new Cat(new CatId(Guid.NewGuid()),catCfg.Color);
                        if (catCfg.IsHidden)
                            cat.SetHidden(true);
                        return cat;
                    })
                    .ToArray();

                var col = new Column(id, cats, colCfg.Capacity);
                columns.Add(col);
            }

            return new Board(columns);
        }
        
        public static LevelConfig FromBoard(Board board, LevelConfig baseLayout)
        {
            var newColumns = board.Columns
                .Select(c =>
                {
                    var cats = c.Cats
                        .Reverse() // bottom→top 순서로 반환해야 함
                        .Select(cat =>
                            new CatConfig(cat.Color, cat.IsHidden))
                        .ToArray();

                    return new ColumnConfig(c.Id, c.Capacity, cats);
                })
                .ToArray();

            return new LevelConfig(
                baseLayout.LevelId,
                baseLayout.Difficulty, 
                baseLayout.LayoutType,
                baseLayout.GridRowCount,
                baseLayout.SpacingX,
                baseLayout.SpacingY,
                baseLayout.ArcStartAngle,
                baseLayout.ArcEndAngle,
                baseLayout.ArcRadius,
                newColumns
            );
        }
    }
}