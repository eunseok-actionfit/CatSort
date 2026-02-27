using System;


namespace Domain
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }
}

