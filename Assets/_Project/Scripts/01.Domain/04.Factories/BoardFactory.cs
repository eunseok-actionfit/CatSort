using System;
using System.Collections.Generic;


namespace Domain
{
    public static class BoardFactory
    {
        public static Board CreateFromConfig(LevelConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var columns   = new List<Column>(config.Columns.Length);
            var lockRules = new List<ColumnLockRule>();

            foreach (var colConfig in config.Columns)
            {

                ColumnId columnId = !colConfig.Id.IsEmpty ? new ColumnId(colConfig.Id) :  ColumnId.New();
                
                var initialCats = new List<Cat>();

                for (int index = 0; index < colConfig.CatsFromBottomToTop.Length; index++)
                {
                    var catConfig = colConfig.CatsFromBottomToTop[index];

                    var catId = !catConfig.Id.IsEmpty ? new CatId(catConfig.Id) : CatId.New();

                    var color  = catConfig.Color;
                    var hidden = catConfig.IsHidden;

                    initialCats.Add(new Cat(catId, color, hidden));
                }

                var column = new Column(
                    columnId,
                    initialCats,
                    colConfig.Capacity,
                    colConfig.BehaviorType,
                    colConfig.LockColor);

                columns.Add(column);

                if (colConfig.BehaviorType == ColumnBehaviorType.Hidden &&
                    colConfig.LockColor != CatColor.None)
                {
                    lockRules.Add(new ColumnLockRule(
                        columnId,
                        colConfig.LockColor));
                }
            }

            return new Board(columns, lockRules);
        }

    }
}
