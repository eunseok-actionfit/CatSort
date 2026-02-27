using Domain;


public readonly struct MovedCatInfo
{
    public CatId CatId { get; }
    public int   ToColumnIndex { get; }
    public int   ToSlotIndex   { get; }

    public MovedCatInfo(CatId catId, int toColumnIndex, int toSlotIndex)
    {
        CatId         = catId;
        ToColumnIndex = toColumnIndex;
        ToSlotIndex   = toSlotIndex;
    }
}