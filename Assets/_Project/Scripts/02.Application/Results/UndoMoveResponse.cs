using Domain;


namespace App.Results
{
    public sealed class UndoMoveResponse
    {
        public bool          Succeeded   { get; }
        public BoardStateDto State       { get; }
        public bool          CanUndoMore { get; }
        public MoveResult    MoveResult  { get; }

        public UndoMoveResponse(
            bool succeeded,
            BoardStateDto state,
            bool canUndoMore,
            MoveResult moveResult)
        {
            Succeeded   = succeeded;
            State       = state;
            CanUndoMore = canUndoMore;
            MoveResult  = moveResult;
        }

        // 실패용 헬퍼 (MoveResult 필요 없을 때)
        public static UndoMoveResponse Failed(BoardStateDto state, bool canUndoMore)
        {
            return new UndoMoveResponse(
                succeeded: false,
                state: state,
                canUndoMore: canUndoMore,
                moveResult: null
            );
        }
    }
}