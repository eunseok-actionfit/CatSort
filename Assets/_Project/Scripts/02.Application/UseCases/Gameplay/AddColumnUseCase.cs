using App.Results;
using Domain;

namespace App.Gameplay
{
    /// <summary>
    /// 실제 게임 플레이 중 보드에 새로운 컬럼을 추가하는 유즈케이스.
    /// - 현재 보드를 가져온다.
    /// - Board 도메인에 컬럼 추가를 위임한다.
    /// - 보드를 저장하고 DTO를 반환해 프레젠테이션을 갱신한다.
    /// </summary>
    public sealed class AddColumnUseCase
    {
        private readonly IBoardRepository _boardRepository;

        public AddColumnUseCase(IBoardRepository boardRepository)
        {
            _boardRepository = boardRepository;
        }

        public AddColumnResult Execute(
            int capacity = 4,
            ColumnBehaviorType behaviorType = ColumnBehaviorType.Normal,
            CatColor? lockColor = null)
        {
            // 1) 현재 보드 가져오기
            var board = _boardRepository.GetCurrentBoard();

            // 2) 도메인 로직에 컬럼 추가 위임
            var newColumn = board.AddNewColumn(capacity, behaviorType, lockColor);

            // 3) 저장
            _boardRepository.Save(board);

            // 4) DTO 변환
            var stateDto = BoardMapper.ToDto(board);

            // 새로 추가된 컬럼의 Id도 같이 넘겨주면,
            // VM/뷰에서 하이라이트/애니메이션에 사용할 수 있음.
            return new AddColumnResult(stateDto, newColumn.Id);
        }
    }
}