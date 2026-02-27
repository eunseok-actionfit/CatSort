using Domain;

namespace App.Results
{
    public sealed class AddColumnResult
    {
        public BoardStateDto Board { get; }
        public ColumnId      NewColumnId { get; }

        public AddColumnResult(BoardStateDto board, ColumnId newColumnId)
        {
            Board       = board;
            NewColumnId = newColumnId;
        }
    }
}