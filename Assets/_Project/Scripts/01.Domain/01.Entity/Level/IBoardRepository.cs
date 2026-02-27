

namespace Domain
{
    public interface IBoardRepository
    {
        Board GetCurrentBoard();
        void Save(Board board);
    }
}