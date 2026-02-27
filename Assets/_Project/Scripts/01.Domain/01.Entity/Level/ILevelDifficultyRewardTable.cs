namespace Domain
{
    public interface ILevelDifficultyRewardTable
    {
        int GetCoinReward(LevelDifficulty difficulty);
    }
}


