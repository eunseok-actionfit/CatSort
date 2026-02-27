using System;
using System.Collections.Generic;
using Presentation;


public sealed class BoosterBarViewModel
{
    public BoosterViewModel Undo      { get; }
    public BoosterViewModel Shuffle   { get; }
    public BoosterViewModel AddColumn { get; }

    private readonly Dictionary<BoosterId, BoosterViewModel> _byId;

    public BoosterBarViewModel( Func<BoosterId, BoosterViewModel> factory)
    {
        Undo      = factory.Invoke(BoosterId.Undo);
        Shuffle   = factory.Invoke(BoosterId.Shuffle);
        AddColumn = factory.Invoke(BoosterId.AddColumn);

        _byId = new Dictionary<BoosterId, BoosterViewModel>
        {
            { BoosterId.Undo, Undo },
            { BoosterId.Shuffle, Shuffle },
            { BoosterId.AddColumn, AddColumn },
        };
    }

    public BoosterViewModel Get(BoosterId id) => _byId[id];
}