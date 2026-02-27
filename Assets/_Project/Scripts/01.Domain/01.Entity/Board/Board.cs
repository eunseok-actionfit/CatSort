using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain
{
    public sealed class Board
    {
        private readonly Dictionary<ColumnId, Column> _columnsById;
        private readonly List<Column> _columnsInOrder;
        public IReadOnlyCollection<Column> Columns => _columnsInOrder;

        private readonly List<ColumnLockRule> _lockRules;

        public Board(
            IEnumerable<Column> columnsInOrder,
            IEnumerable<ColumnLockRule> lockRules = null)
        {
            if (columnsInOrder == null)
                throw new ArgumentNullException(nameof(columnsInOrder));

            _columnsInOrder = columnsInOrder.ToList();
            if (_columnsInOrder.Count == 0)
                throw new ArgumentException("Board must have at least one column.", nameof(columnsInOrder));

            _columnsById = _columnsInOrder.ToDictionary(c => c.Id, c => c);

            _lockRules = lockRules?.ToList() ?? new List<ColumnLockRule>();

            ReevaluateLocks();

            // 초기: "완성된 컬럼"에 있는 고양이만 Completed 동기화
            SyncCompletedByColumn();
        }

        public bool TryGetColumn(ColumnId id, out Column column)
            => _columnsById.TryGetValue(id, out column);

        public Column GetColumn(ColumnId id)
        {
            if (!_columnsById.TryGetValue(id, out var column))
                throw new InvalidOperationException($"Column with id {id} not found.");

            return column;
        }

        public bool CanMoveCat(ColumnId from, ColumnId to)
        {
            if (!_columnsById.TryGetValue(from, out var fromCol)) return false;
            if (!_columnsById.TryGetValue(to, out var toCol)) return false;

            // 1) from 금지
            if (fromCol.BehaviorType == ColumnBehaviorType.Hidden) return false;
            if (fromCol.BehaviorType == ColumnBehaviorType.DestinationOnly) return false;

            // 2) to 금지
            if (toCol.BehaviorType == ColumnBehaviorType.Hidden) return false;

            // 3) 기존 규칙 처리
            if (fromCol.IsEmpty) return false;
            if (toCol.IsFull) return false;

            if (toCol.IsEmpty) return true;

            if (!fromCol.TryPeek(out var fromTop) || !toCol.TryPeek(out var toTop)) return false;
            if (fromTop.IsHidden || toTop.IsHidden) return false;

            return fromTop.Color == toTop.Color;
        }

        public MoveResult MoveCat(ColumnId from, ColumnId to)
        {
            if (from.Equals(to))
                return MoveResult.Invalid();

            if (!_columnsById.TryGetValue(from, out var fromCol))
                return MoveResult.Invalid();

            if (!_columnsById.TryGetValue(to, out var toCol))
                return MoveResult.Invalid();

            // 이동 전: 목적지 컬럼이 완성이었는지(새로 완성 감지용)
            bool toWasComplete = toCol.IsUniformAndFull();

            var moved = MoveCatsOfSameColor(fromCol, toCol);

            if (moved.Count == 0)
                return MoveResult.Invalid();

            fromCol.UnhideTopCat();

            ReevaluateLocks();

            // 이동 후: 목적지 컬럼이 새로 완성되면 그 컬럼 고양이만 Completed=true
            bool toIsComplete = toCol.IsUniformAndFull();
            if (!toWasComplete && toIsComplete)
                SetColumnCatsCompleted(toCol, true);

            // 출발 컬럼은 이동으로 인해 "완성 깨짐"이 일어날 수 있음
            // (완성된 컬럼에서 꺼냈으면 깨질 수 있으니 동기화)
            bool fromIsCompleteNow = fromCol.IsUniformAndFull();
            if (!fromIsCompleteNow)
                SetColumnCatsCompleted(fromCol, false);

            // 보드 전체 클리어 판정은 그대로(게임 결과용)
            bool levelCompleted = IsLevelCompleted();

            return levelCompleted
                ? MoveResult.Completed(moved)
                : MoveResult.Success(moved);
        }

        /// <summary>
        /// 정방향 Move에서 이동했던 고양이들을 정확하게 되돌린다.
        /// </summary>
        public void UndoMove(PerformedMove performed)
        {
            var fromCol = GetColumn(performed.Move.From);
            var toCol   = GetColumn(performed.Move.To);

            // Undo 전 상태 기억: "깨지는지/복구되는지" 판단
            bool fromWasComplete = fromCol.IsUniformAndFull();
            bool toWasComplete   = toCol.IsUniformAndFull();

            for (int i = 0; i < performed.MovedCount; i++)
            {
                var cat = toCol.Pop();
                fromCol.Push(cat);
            }

            ReevaluateLocks();

            // Undo 후 재평가
            bool fromIsCompleteNow = fromCol.IsUniformAndFull();
            bool toIsCompleteNow   = toCol.IsUniformAndFull();

            // 1) toCol은 되돌리면서 "완성이 깨질 수" 있음 -> 깨졌으면 Completed=false
            if (toWasComplete && !toIsCompleteNow)
                SetColumnCatsCompleted(toCol, false);

            // 2) fromCol은 되돌리면서 "완성이 새로 생길 수" 있음 -> 새로 완성되면 Completed=true
            if (!fromWasComplete && fromIsCompleteNow)
                SetColumnCatsCompleted(fromCol, true);

            // 3) 그 외 케이스(이미 완성이었고 그대로, 이미 비완성이고 그대로)는 유지
        }

        public int GetMovableCatCount(ColumnId from, ColumnId to)
        {
            if (!CanMoveCat(from, to))
                return 0;

            var fromCol = GetColumn(from);
            var toCol   = GetColumn(to);

            var runCount = fromCol.GetMovableRunCountFromTop();
            if (runCount <= 0)
                return 0;

            return Math.Min(runCount, toCol.RemainingCapacity);
        }

        private IReadOnlyList<MovedCat> MoveCatsOfSameColor(Column fromCol, Column toCol)
        {
            var moved = new List<MovedCat>();

            if (!fromCol.TryPeek(out var firstTop))
                return moved;

            var moveColor = firstTop.Color;
            int freeSlots = toCol.RemainingCapacity;

            while (freeSlots > 0 && !fromCol.IsEmpty)
            {
                if (!fromCol.TryPeek(out var top)
                    || top.IsHidden
                    || top.Color != moveColor) { break; }

                var cat = fromCol.Pop();
                toCol.Push(cat);
                moved.Add(new MovedCat(cat.Id, fromCol.Id, toCol.Count - 1));

                freeSlots--;
            }

            return moved;
        }

        private void ReevaluateLocks()
        {
            if (_lockRules.Count == 0)
                return;

            foreach (var rule in _lockRules)
            {
                if (!_columnsById.TryGetValue(rule.TargetColumnId, out var targetCol))
                    continue;

                bool hasUnlocker = _columnsInOrder.Any(col =>
                {
                    if (col.Id.Equals(rule.TargetColumnId))
                        return false;

                    if (!col.IsUniformAndFull())
                        return false;

                    if (!col.TryPeek(out var top))
                        return false;

                    return top.Color == rule.RequiredColor;
                });

                if (hasUnlocker) targetCol.SetUnlocked();
            }
        }

        public bool IsLevelCompleted()
        {
            return _columnsInOrder.All(c =>
                c.IsEmpty || c.IsUniformAndFull()
            );
        }

        private void SetColumnCatsCompleted(Column col, bool completed)
        {
            foreach (var cat in col.Cats)
                cat.SetCompleted(completed);
        }

        private void SyncCompletedByColumn()
        {
            foreach (var col in _columnsInOrder)
            {
                bool isComplete = col.IsUniformAndFull();
                SetColumnCatsCompleted(col, isComplete);
            }
        }

        public Column AddNewColumn(
            int capacity,
            ColumnBehaviorType behaviorType = ColumnBehaviorType.Normal,
            CatColor? lockColor = null)
        {
            var newColumn = new Column(
                id: ColumnId.New(),
                cats: Array.Empty<Cat>(),
                capacity: capacity,
                behaviorType: behaviorType,
                lockColor: lockColor
            );

            _columnsInOrder.Add(newColumn);
            _columnsById.Add(newColumn.Id, newColumn);

            ReevaluateLocks();

            // 새 컬럼은 비어있으니 Completed=false가 맞고,
            // 기존 컬럼들의 완료 상태는 유지되게 "컬럼 단위 동기화"만 수행
            SyncCompletedByColumn();

            return newColumn;
        }
    }
}
