using System;
using System.Linq;
using App.Results;
using App.State;
using Domain;


namespace App.Gameplay
{
    public sealed class MoveCatUseCase
    {
        private readonly IBoardRepository _boardRepository;
        private readonly IMoveHistoryRepository _historyRepository;

        public MoveCatUseCase(
            IBoardRepository boardRepository,
            IMoveHistoryRepository historyRepository)
        {
            _boardRepository = boardRepository;
            _historyRepository = historyRepository;
        }

        /// <summary>
        /// 고양이 이동 도메인 로직을 수행하고,
        /// 도메인 이동 결과 + 최신 보드 상태 DTO를 반환한다.
        /// 프레젠테이션은 이 데이터를 기반으로 연출/레이아웃을 갱신한다.
        /// </summary>
        public MoveCatResponse Execute(ColumnId from, ColumnId to)
        {
            var board = _boardRepository.GetCurrentBoard();
            var history = _historyRepository.GetCurrentHistory();

            // 1. 기존 이동 체크 (쿼리용)
            var canMove = board.CanMoveCat(from, to);

            // 2. 못 움직이는 경우: 보드 변화/히스토리 기록 없음
            if (!canMove)
            {
                var invalid = MoveResult.Invalid();
                var dto = BoardMapper.ToDto(board);

                return new MoveCatResponse(
                    invalid,
                    dto,
                    GameStatus.Playing,
                    history.CanUndo,
                    false
                );
            }

            // 3. 실제 도메인 이동 수행
            var moveResult = board.MoveCat(from, to);

            if (moveResult.ResultType == MoveResultType.InvalidMove)
            {
                var dto = BoardMapper.ToDto(board);
                return new MoveCatResponse(
                    moveResult,
                    dto,
                    GameStatus.Playing,
                    history.CanUndo,
                    true
                );
            }

            var movedCount = moveResult.MovedCats.Count;

            var move = new Move(from, to, DateTime.UtcNow);
            var performed = new PerformedMove(move, movedCount);

            history.Record(performed);

            _boardRepository.Save(board);
            _historyRepository.Save(history);

            var stateDto = BoardMapper.ToDto(board);

            var status = GameStatus.Playing;

            if (moveResult.ResultType == MoveResultType.Completed) { status = GameStatus.Cleared; }
            else
            {
                // ========= 실패 조건 판정 =========
                // 1) 어떤 이동도 불가능하면 실패
                if (!HasAnyValidMove(board)) { status = GameStatus.Failed; }
                else
                {
                    // 2) 이동은 가능한데, 유일한 이동이 "마지막 이동의 Undo" 뿐이면 실패
                    var lastMove = history.Moves.FirstOrDefault(); // 가장 최근 PerformedMove

                    if (lastMove != null && !HasAnyNonUndoMove(board, lastMove)) { status = GameStatus.Failed; }
                }
            }

            return new MoveCatResponse(
                moveResult,
                stateDto,
                status,
                history.CanUndo,
                true
            );
        }


        /// <summary>
        /// 현재 보드에서 하나라도 유효한 이동이 있는지 검사
        /// </summary>
        private static bool HasAnyValidMove(Board board)
        {
            var columns = board.Columns.ToArray();

            foreach (var from in columns)
                foreach (var to in columns)
                {
                    if (from.Id.Equals(to.Id))
                        continue;

                    if (board.CanMoveCat(from.Id, to.Id))
                        return true;
                }

            return false;
        }

        /// <summary>
        /// 현재 보드에서 "마지막 수행한 이동을 되돌리는 경우" 이외의
        /// 다른 유효한 이동이 하나라도 있는지 검사
        /// </summary>
        private static bool HasAnyNonUndoMove(Board board, PerformedMove lastMove)
        {
            var columns = board.Columns.ToArray();

            foreach (var from in columns)
                foreach (var to in columns)
                {
                    if (from.Id.Equals(to.Id))
                        continue;

                    // 기본 규칙상 이동 불가면 스킵
                    if (!board.CanMoveCat(from.Id, to.Id))
                        continue;

                    // 이 방향으로 실제로 몇 마리가 움직일 수 있는지 계산
                    int movableCount = board.GetMovableCatCount(from.Id, to.Id);
                    if (movableCount <= 0)
                        continue;

                    // "이전 이동의 정확한 Undo" 인지:
                    //   1) 방향이 반대(from == last.To, to == last.From)
                    //   2) 이동 마릿수도 동일(movableCount == last.MovedCount)
                    bool isExactUndo =
                        from.Id.Equals(lastMove.Move.To) &&
                        to.Id.Equals(lastMove.Move.From) &&
                        movableCount == lastMove.MovedCount;

                    // 방향 + 마릿수까지 정확히 일치하는 경우만 Undo로 인정
                    if (!isExactUndo)
                        return true; // Undo 외에 다른 유효한 이동이 하나라도 있다
                }

            // 가능한 이동이 전부 "정확한 Undo" 뿐이다
            return false;
        }

    }
}