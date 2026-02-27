using Domain;


namespace App
{
    public interface ILevelClearRewardTable
    {
        LevelClearRewardBundle GetReward(LevelDifficulty difficulty);
    }
}