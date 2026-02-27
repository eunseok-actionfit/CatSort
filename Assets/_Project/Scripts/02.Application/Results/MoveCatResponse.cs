using App.State;
using Domain;


namespace App.Results
{
    public sealed class MoveCatResponse
    {
        public MoveResult    MoveResult { get; }
        public BoardStateDto State      { get; }
        public GameStatus    Status     { get; }
        public bool CanUndo { get; }
        public bool          CanMove    { get; } 


    
        public MoveCatResponse(
            MoveResult moveResult,
            BoardStateDto state,
            GameStatus status,
            bool canUndo,
            bool canMove)  
        {
            MoveResult = moveResult;
            State      = state;
            Status     = status;
            CanUndo    = canUndo;
            CanMove    = canMove;
        }
    }
}