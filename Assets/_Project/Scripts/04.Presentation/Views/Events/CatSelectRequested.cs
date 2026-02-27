using AES.Tools;
using Domain;

public readonly struct CatSelectRequested : IEvent
{
    public readonly CatId CatId;
    public readonly bool Selected;
    public readonly float Delay;

    public CatSelectRequested(CatId catId, bool selected, float delay)
    {
        CatId = catId;
        Selected = selected;
        Delay = delay;
    }
}