using System;
using AES.Tools;


namespace Domain
{
    [Serializable]
    public sealed class CatConfig
    {
        public CatColor Color;
        public bool     IsHidden;
        
        public SerializableGuid Id;

        public CatConfig() {}

        public CatConfig(CatColor color, bool isHidden)
        {
            Color    = color;
            IsHidden = isHidden;
        }
    }
    
    [Serializable]
    public sealed class ColumnConfig
    {
        public int Capacity;
        public ColumnBehaviorType BehaviorType = ColumnBehaviorType.Normal;
        public CatColor LockColor = CatColor.None; // BehaviorType == Locked 일 때만 의미
        
        public SerializableGuid Id = SerializableGuid.Empty;

        public CatConfig[] CatsFromBottomToTop;

        public ColumnConfig() {}

        public ColumnConfig(
            ColumnId id,
            int capacity,
            CatConfig[] cats,
            ColumnBehaviorType behaviorType = ColumnBehaviorType.Normal,
            CatColor lockColor = CatColor.None)
        {
            Id                  = id.Value;
            Capacity            = capacity;
            CatsFromBottomToTop = cats;
            BehaviorType        = behaviorType;
            LockColor           = lockColor;
        }
    }
    
    [Serializable]
    public sealed class LevelConfig
    {
        public string LevelId;

        // 난이도
        public LevelDifficulty Difficulty = LevelDifficulty.Normal;

        // 레이아웃
        public BoardLayoutType LayoutType;
        public float SpacingX;
        public float SpacingY;
        public int   GridRowCount;
        public float ArcRadius;
        public float ArcStartAngle;
        public float ArcEndAngle;

        // 컬럼들
        public ColumnConfig[] Columns;

        public LevelConfig() {}
    
        public LevelConfig(
            string levelId,
            LevelDifficulty difficulty,    
            BoardLayoutType layoutType,
            int gridRowCount,
            float spacingX,
            float spacingY,
            float arcStartAngle,
            float arcEndAngle,
            float arcRadius,
            ColumnConfig[] columns)
        {
            LevelId       = levelId;
            Difficulty    = difficulty;    
            LayoutType    = layoutType;
            GridRowCount  = gridRowCount;
            SpacingX      = spacingX;
            SpacingY      = spacingY;
            ArcStartAngle = arcStartAngle;
            ArcEndAngle   = arcEndAngle;
            ArcRadius     = arcRadius;
            Columns       = columns;
        }
    }

}