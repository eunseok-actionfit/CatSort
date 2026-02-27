using System.Collections.Generic;


public readonly struct MovedCatPath
{
    public int ColumnIndex { get; }
    public int SlotIndex   { get; }

    public MovedCatPath(int columnIndex, int slotIndex)
    {
        ColumnIndex = columnIndex;
        SlotIndex   = slotIndex;
    }
}

public readonly struct MoveAnimationInfo
{
    public int FromColumnIndex { get; }
    public int ToColumnIndex   { get; }
    public IReadOnlyList<MovedCatPath> Cats { get; }

    public MoveAnimationInfo(int fromColumnIndex, int toColumnIndex, IReadOnlyList<MovedCatPath> cats)
    {
        FromColumnIndex = fromColumnIndex;
        ToColumnIndex   = toColumnIndex;
        Cats            = cats;
    }
}