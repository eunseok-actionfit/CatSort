
using Domain;


namespace App.Results
{
    public sealed class LoadLevelResult
    {
        public BoardLayoutDto Layout { get; }
        public BoardStateDto  State  { get; }
        
        public LevelDifficulty Difficulty { get; } 

        public LoadLevelResult(BoardLayoutDto layout, BoardStateDto state, LevelDifficulty difficulty)
        {
            Layout = layout;
            State  = state;
            Difficulty = difficulty;
        }
    }
}