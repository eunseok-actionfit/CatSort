using App.Results;
using Domain;


namespace App.Gameplay
{
    public sealed class UndoMoveUseCase
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IMoveHistoryRepository _historyRepository;

        public UndoMoveUseCase(
            IBoardRepository boardRepository,
            IMoveHistoryRepository historyRepository)
        {
            _boardRepository   = boardRepository;
            _historyRepository = historyRepository;
        }

        public UndoMoveResponse Execute()
        {
            var board   = _boardRepository.GetCurrentBoard();
            var history = _historyRepository.GetCurrentHistory();

            // 더 이상 되돌릴 게 없으면 바로 실패 응답
            if (!history.CanUndo)
            {
                var state = BoardMapper.ToDto(board);
                return new UndoMoveResponse(
                    succeeded: false,
                    state: state,
                    canUndoMore: false,
                    moveResult: null
                );
            }

            // 마지막 Move 꺼내기 (Pop)
            var lastPerformedMove = history.UndoLast();

            // 규칙(색/숨김/용량 등) 체크 없이, 도메인 MoveCat만 호출해서 반대로 되돌림
            board.UndoMove(lastPerformedMove);

            // // 이론상 Undo가 Invalid 나오는 경우는 이상 상황이므로, 히스토리 복구 후 실패 처리
            // if (undoResult.ResultType == MoveResultType.InvalidMove)
            // {
            //     // 정책: Undo 실패하면 history도 되돌려놓는다.
            //     history.Record(lastMove);
            //
            //     var state = BoardMapper.ToDto(board);
            //     return new UndoMoveResponse(
            //         succeeded: false,
            //         state: state,
            //         canUndoMore: history.CanUndo,
            //         moveResult: null
            //     );
            // }

            // 정상적으로 되돌아간 경우 저장
            _boardRepository.Save(board);
            _historyRepository.Save(history);

            var dto = BoardMapper.ToDto(board);

            return new UndoMoveResponse(
                succeeded: true,
                state: dto,
                canUndoMore: history.CanUndo,
                moveResult: null
            );
        }
    }
}
