using System;


public readonly struct LivesStatus
{
    public readonly int Lives;
    public readonly TimeSpan NextRegenRemaining;
    public readonly bool IsUnlimited;
    public readonly TimeSpan UnlimitedRemaining;

    public LivesStatus(
        int lives,
        TimeSpan nextRegenRemaining,
        bool isUnlimited,
        TimeSpan unlimitedRemaining)
    {
        Lives = lives;
        NextRegenRemaining = nextRegenRemaining;
        IsUnlimited = isUnlimited;
        UnlimitedRemaining = unlimitedRemaining;
    }
}