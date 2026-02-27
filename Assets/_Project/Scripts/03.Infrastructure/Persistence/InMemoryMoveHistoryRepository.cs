using Domain;


namespace Infrastructure
{
    /// <summary>
    /// 한 게임 세션(혹은 한 씬) 동안만 유효한 In-Memory MoveHistory 저장소.
    /// </summary>
    public sealed class InMemoryMoveHistoryRepository : IMoveHistoryRepository
    {
        // "현재 레벨"의 move history 하나를 들고 있는다고 가정
        private MoveHistory _current = new();

        public MoveHistory GetCurrentHistory()
        {
            // 항상 null 아닌 인스턴스를 돌려주도록 보장
            return _current ??= new MoveHistory();
        }

        public void Save(MoveHistory history)
        {
            // null 방어
            _current = history ?? new MoveHistory();
        }
        
    }
}