using App;
using Domain;


public struct BoardLayoutState
{
    public BoardLayoutType LayoutType;
    public float           SpacingX;
    public float           SpacingY;
    public int             GridRowCount;
    public float           ArcRadius;
    public float           ArcStartAngle;
    public float           ArcEndAngle;

    public static BoardLayoutState FromDto(BoardLayoutDto dto)
        => new()
        {
            LayoutType    = dto.LayoutType,
            SpacingX      = dto.SpacingX,
            SpacingY      = dto.SpacingY,
            GridRowCount  = dto.GridRowCount,
            ArcRadius     = dto.ArcRadius,
            ArcStartAngle = dto.ArcStartAngle,
            ArcEndAngle   = dto.ArcEndAngle,
        };
}