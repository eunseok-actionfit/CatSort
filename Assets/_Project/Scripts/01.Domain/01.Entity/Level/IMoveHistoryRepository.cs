namespace Domain
{
    public interface IMoveHistoryRepository
    {
        MoveHistory GetCurrentHistory();
        void Save(MoveHistory history);
    }
}