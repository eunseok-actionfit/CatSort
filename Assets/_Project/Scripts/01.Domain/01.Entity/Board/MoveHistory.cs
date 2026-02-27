using System.Collections.Generic;


namespace Domain
{
    public sealed class MoveHistory
    {
        private readonly Stack<PerformedMove> _moves = new();

        public IReadOnlyCollection<PerformedMove> Moves => _moves;

        public bool CanUndo => _moves.Count > 0;

        public void Record(PerformedMove move)
        {
            _moves.Push(move);
        }

        public PerformedMove UndoLast()
        {
            if (!CanUndo)
                throw new System.InvalidOperationException("No moves to undo");

            return _moves.Pop();
        }

        public void Clear()
        {
            _moves.Clear();
        }
    }
}