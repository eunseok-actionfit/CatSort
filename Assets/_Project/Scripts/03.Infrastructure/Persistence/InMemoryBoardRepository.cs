using Domain;


namespace Infrastructure
{
    public sealed class InMemoryBoardRepository : IBoardRepository
    {
        private Board _currentBoard;

        public Board GetCurrentBoard()
        {
            if (_currentBoard == null)
                throw new System.Exception("Board not initialized. Call LoadLevelUseCase first.");

            return _currentBoard;
        }

        public void Save(Board board)
        {
            _currentBoard = board;
        }
    }
}