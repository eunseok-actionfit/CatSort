namespace Presentation.Input
{
    public interface IBoardInteractionService
    {
        void OnEmptyTap();
        void OnColumnTapped(int columnIndex);
        void OnUndoRequested();
    }
}