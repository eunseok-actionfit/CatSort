using AES.Tools;
using App;
using Domain;

public readonly struct CatMoveRequested : IEvent
{
    public readonly CatId CatId;
    public readonly ViewSlotAddr To;
    public readonly float Delay;

    public CatMoveRequested(CatId catId, ViewSlotAddr to, float delay)
    {
        CatId = catId;
        To = to;
        Delay = delay;
    }
}