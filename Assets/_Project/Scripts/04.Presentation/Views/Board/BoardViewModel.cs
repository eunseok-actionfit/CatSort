// BoardViewModel.cs (수정본: "컬럼 완성 예약" + AllFxDone 후 발사 + 아래부터 딜레이)
using System;
using System.Collections.Generic;
using System.Linq;
using AES.Tools;
using App.Gameplay;
using App.State;
using Domain;
using Lofelt.NiceVibrations;
using Presentation;
using UnityEngine;

namespace App.Mvvm
{
    public sealed class BoardViewModel
    {
        public ObservableList<ColumnViewModel> Columns { get; } = new();

        public ObservableProperty<GameStatus> Status { get; } = new();
        public ObservableProperty<GameStatus> PendingOutcome { get; } = new(GameStatus.Playing);

        public ObservableProperty<bool> AllFxDone { get; } = new(true);

        public ObservableProperty<BoardInputMode> InputMode { get; } = new(BoardInputMode.Normal);
        public ObservableList<int> AllowedTargetColumns { get; } = new();
        public event Action<int> TargetColumnSelected;

        public bool IsAnyBusy => Columns.Any(c => c.IsBusy);

        public ObservableProperty<bool> CanUndo { get; } = new();

        private int? _selectedColumnIndex;
        public ObservableProperty<int> SelectedColumnIndex { get; } = new(-1);

        public ObservableProperty<BoardLayoutState> Layout { get; } =
            new(new BoardLayoutState
            {
                LayoutType = BoardLayoutType.Grid,
                SpacingX = 2f,
                SpacingY = 2.5f,
                GridRowCount = 3,
                ArcRadius = 5f,
                ArcStartAngle = -45f,
                ArcEndAngle = 45f,
            });

        private readonly Dictionary<ColumnId, ColumnViewModel> _columnCache = new();
        private readonly Dictionary<CatId, CatViewModel> _catCache = new();
        private readonly HashSet<CatId> _catSubscribed = new();
        
        private readonly HashSet<ColumnId> _pendingColumnCompleteFx = new();
        private readonly Dictionary<ColumnId, bool> _prevColumnComplete = new();

        private readonly MoveCatUseCase _moveCatUseCase;
        private readonly UndoMoveUseCase _undoMoveUseCase;
        private readonly AddColumnUseCase _addColumnUseCase;

        public Command UndoCommand { get; }
        public Command<int> ColumnTapCommand { get; }
        public Command EmptyTapCommand { get; }
        public Command AddcolumnCommand { get; }

        public BoardViewModel(
            MoveCatUseCase moveCatUseCase,
            UndoMoveUseCase undoMoveUseCase,
            AddColumnUseCase addColumnUseCase)
        {
            _moveCatUseCase = moveCatUseCase;
            _undoMoveUseCase = undoMoveUseCase;
            _addColumnUseCase = addColumnUseCase;

            UndoCommand = new Command(
                execute: OnUndoRequested,
                canExecute: () => Status.Value == GameStatus.Playing && CanUndo.Value
            );

            ColumnTapCommand = new Command<int>(
                execute: OnColumnTapped,
                canExecute: _ => Status.Value == GameStatus.Playing
            );

            EmptyTapCommand = new Command(
                execute: OnEmptyTap,
                canExecute: () => Status.Value == GameStatus.Playing
            );

            AddcolumnCommand = new Command(
                execute: OnAddColumnRequested,
                canExecute: () => Status.Value == GameStatus.Playing
            );

            Status.OnValueChanged += status =>
            {
                if (status != GameStatus.Playing)
                    SetSelectedColumn(null);
            };

            SelectedColumnIndex.OnValueChanged += _ =>
            {
                HapticPatterns.PlayPreset(HapticPatterns.PresetType.Selection);
            };
        }

        private void OnAddColumnRequested()
        {
            var result = _addColumnUseCase.Execute();
            ApplyState(result.Board);

            PendingOutcome.Value = GameStatus.Playing;

            RecalcAllFxDone();
        }

        public void ResetBoard()
        {
            Columns.Clear();
            _columnCache.Clear();
            _catCache.Clear();
            _catSubscribed.Clear();

            _pendingColumnCompleteFx.Clear();
            _prevColumnComplete.Clear();

            PendingOutcome.Value = GameStatus.Playing;
            AllFxDone.Value = true;
        }

        #region Layout (기존 그대로)
        public void ApplyLayout(BoardLayoutDto layoutDto)
        {
            Layout.Value = BoardLayoutState.FromDto(layoutDto);
            RelayoutColumns();
        }

        private void RelayoutColumns()
        {
            var cols = Columns;
            if (cols.Count == 0) return;

            var layout = Layout.Value;
            switch (layout.LayoutType)
            {
                case BoardLayoutType.Grid: LayoutColumnsGrid(in layout); break;
                case BoardLayoutType.Arc:  LayoutColumnsArc(in layout);  break;
            }
        }

        private void LayoutColumnsGrid(in BoardLayoutState layout)
        {
            var cols = Columns;
            int count = cols.Count;
            if (count == 0) return;

            float spacingX = layout.SpacingX;
            float spacingY = layout.SpacingY;
            int rowCount = Mathf.Max(1, layout.GridRowCount);

            int rows = Mathf.Min(rowCount, count);
            int basePerRow = count / rows;
            int extra = count % rows;

            int[] colsInRow = new int[rows];
            int maxCols = 0;
            for (int r = 0; r < rows; r++)
            {
                int c = basePerRow + (r < extra ? 1 : 0);
                colsInRow[r] = c;
                if (c > maxCols) maxCols = c;
            }

            float centerY = (rows - 1) * 0.5f;
            int index = 0;

            for (int colIndex = 0; colIndex < maxCols && index < count; colIndex++)
            {
                for (int row = 0; row < rows && index < count; row++)
                {
                    int colsThisRow = colsInRow[row];
                    if (colIndex >= colsThisRow) continue;

                    float centerX = (colsThisRow - 1) * 0.5f;
                    int rowIndex = rows - 1 - row;
                    float y = (rowIndex - centerY) * spacingY;
                    float x = (colIndex - centerX) * spacingX;

                    cols[index].LocalPosition.Value = new Vector3(x, y, 0f);
                    index++;
                }
            }
        }

        private void LayoutColumnsArc(in BoardLayoutState layout)
        {
            var cols = Columns;
            int count = cols.Count;
            if (count == 0) return;

            float radius = layout.ArcRadius;
            float startAngle = layout.ArcStartAngle;
            float endAngle = layout.ArcEndAngle;

            if (count == 1)
            {
                float rad = startAngle * Mathf.Deg2Rad;
                cols[0].LocalPosition.Value = new Vector3(
                    Mathf.Cos(rad) * radius,
                    Mathf.Sin(rad) * radius,
                    0f
                );
                return;
            }

            float step = (endAngle - startAngle) / (count - 1);

            for (int i = 0; i < count; i++)
            {
                float angleDeg = startAngle + step * i;
                float rad = angleDeg * Mathf.Deg2Rad;

                float x = Mathf.Cos(rad) * radius;
                float y = Mathf.Sin(rad) * radius;

                cols[i].LocalPosition.Value = new Vector3(x, y, 0f);
            }
        }
        #endregion

        #region ApplyState
        public void ApplyState(BoardStateDto state)
        {
            EnsureColumns(state);

            var usedCatIds = new HashSet<CatId>();

            for (int i = 0; i < state.Columns.Count; i++)
            {
                var colDto = state.Columns[i];
                var colVm = Columns[i];

                colVm.ApplyState(colDto, ResolveOrCreateCat, usedCatIds);
            }

           
            DetectColumnCompletionTransitions(state);

            RelayoutColumns();
            CleanupUnusedCats(usedCatIds);

            RecalcAllFxDone();
        }

        private void DetectColumnCompletionTransitions(BoardStateDto state)
        {
            for (int i = 0; i < state.Columns.Count; i++)
            {
                var colDto = state.Columns[i];

                bool prev = _prevColumnComplete.TryGetValue(colDto.ColumnId, out var p) && p;
                bool now  = colDto.IsComplete;

                _prevColumnComplete[colDto.ColumnId] = now;

                if (!prev && now)
                    _pendingColumnCompleteFx.Add(colDto.ColumnId);

                if (prev && !now)
                {
                    _pendingColumnCompleteFx.Remove(colDto.ColumnId);

                    var col = Columns.FirstOrDefault(c => c.ColumnId.Equals(colDto.ColumnId));
                    if (col != null)
                        foreach (var s in col.Slots) s.IsDimmed.Value = false;
                }
            }
        }

        private void EnsureColumns(BoardStateDto state)
        {
            int oldCount = Columns.Count;
            int newCount = state.Columns.Count;

            if (oldCount == newCount)
            {
                bool sameOrder = true;
                for (int i = 0; i < newCount; i++)
                {
                    if (!Columns[i].ColumnId.Equals(state.Columns[i].ColumnId))
                    {
                        sameOrder = false;
                        break;
                    }
                }

                if (sameOrder)
                {
                    for (int i = 0; i < newCount; i++)
                    {
                        var colDto = state.Columns[i];
                        var colVm = Columns[i];

                        colVm.SetIndex(i);
                        colVm.SetCapacity(colDto.Capacity);
                    }
                    return;
                }
            }

            int minCount = Math.Min(oldCount, newCount);
            bool prefixSame = true;

            for (int i = 0; i < minCount; i++)
            {
                if (!Columns[i].ColumnId.Equals(state.Columns[i].ColumnId))
                {
                    prefixSame = false;
                    break;
                }
            }

            if (prefixSame)
            {
                for (int i = 0; i < minCount; i++)
                {
                    var colDto = state.Columns[i];
                    var colVm = Columns[i];

                    colVm.SetIndex(i);
                    colVm.SetCapacity(colDto.Capacity);
                }

                if (newCount > oldCount)
                {
                    for (int i = oldCount; i < newCount; i++)
                    {
                        var colDto = state.Columns[i];

                        if (!_columnCache.TryGetValue(colDto.ColumnId, out var colVm))
                        {
                            colVm = new ColumnViewModel(colDto.ColumnId, i, colDto.Capacity);
                            _columnCache[colDto.ColumnId] = colVm;
                        }
                        else
                        {
                            colVm.SetIndex(i);
                            colVm.SetCapacity(colDto.Capacity);
                        }

                        Columns.Add(colVm);
                    }
                }
                else if (newCount < oldCount)
                {
                    for (int i = oldCount - 1; i >= newCount; i--)
                    {
                        var removedId = Columns[i].ColumnId;
                        Columns.RemoveAt(i);
                        _columnCache.Remove(removedId);

                        _pendingColumnCompleteFx.Remove(removedId);
                        _prevColumnComplete.Remove(removedId);
                    }
                }

                return;
            }

            var aliveColumnIds = new HashSet<ColumnId>();
            foreach (var colDto in state.Columns)
            {
                aliveColumnIds.Add(colDto.ColumnId);

                if (!_columnCache.TryGetValue(colDto.ColumnId, out var colVm))
                {
                    colVm = new ColumnViewModel(colDto.ColumnId, 0, colDto.Capacity);
                    _columnCache[colDto.ColumnId] = colVm;
                }
                else
                {
                    colVm.SetCapacity(colDto.Capacity);
                }
            }

            var deadColumnIds = _columnCache.Keys
                .Where(id => !aliveColumnIds.Contains(id))
                .ToList();

            foreach (var deadId in deadColumnIds)
            {
                _columnCache.Remove(deadId);
                _pendingColumnCompleteFx.Remove(deadId);
                _prevColumnComplete.Remove(deadId);
            }

            Columns.Clear();

            for (int i = 0; i < state.Columns.Count; i++)
            {
                var colDto = state.Columns[i];
                var colVm = _columnCache[colDto.ColumnId];

                colVm.SetIndex(i);
                Columns.Add(colVm);
            }
        }

        private CatViewModel ResolveOrCreateCat(CatStateDto dto)
        {
            if (!_catCache.TryGetValue(dto.CatId, out var vm))
            {
                vm = new CatViewModel(dto.CatId);
                _catCache[dto.CatId] = vm;
            }

            vm.ApplyState(dto);

            if (_catSubscribed.Add(dto.CatId))
            {
                vm.IsMoving.OnValueChanged += _ => RecalcAllFxDone();
                vm.MotionState.OnValueChanged += _ => RecalcAllFxDone();
                vm.IsCompletingAnim.OnValueChanged += _ => RecalcAllFxDone(); 
            }

            return vm;
        }

        private void CleanupUnusedCats(HashSet<CatId> usedCatIds)
        {
            var toRemove = new List<CatId>();

            foreach (var kvp in _catCache)
            {
                if (!usedCatIds.Contains(kvp.Key))
                    toRemove.Add(kvp.Key);
            }

            foreach (var catId in toRemove)
            {
                var vm = _catCache[catId];
                vm.MarkRemoved();
                _catCache.Remove(catId);
                _catSubscribed.Remove(catId);
            }
        }

        private void RecalcAllFxDone()
        {
            if (IsAnyBusy)
            {
                AllFxDone.Value = false;
                return;
            }

            bool catDone = _catCache.Values.All(c => !c.IsBusy);
            if (!catDone)
            {
                AllFxDone.Value = false;
                return;
            }

  
            if (_pendingColumnCompleteFx.Count > 0)
            {
                AllFxDone.Value = false;   
                FirePendingColumnCompleteFx(); 
                return;
            }

            // pending도 없으면 진짜로 끝난 것
            AllFxDone.Value = true;
        }


        private void FirePendingColumnCompleteFx()
        {
            if (_pendingColumnCompleteFx.Count == 0)
                return;

            AllFxDone.Value = false;
            
            // 이번 프레임에 처리할 목록 스냅샷 후 비움
            var targets = _pendingColumnCompleteFx.ToList();
            _pendingColumnCompleteFx.Clear();

            foreach (var colId in targets)
            {
                var col = Columns.FirstOrDefault(c => c.ColumnId.Equals(colId));
                if (col == null) continue;
                if (!col.IsComplete.Value) continue; // Undo로 깨진 경우 방지

                StartCompleteAnimForColumn(col);
            }
        }

        private void StartCompleteAnimForColumn(ColumnViewModel col)
        {
            for (int i = 0; i < col.Slots.Count; i++)
                col.Slots[i].IsDimmed.Value = (col.Slots[i].Cat.Value != null);

            float step = CatAnimationConfig.CompletionStepDelay;

            int order = 0;
            for (int i = 0; i < col.Slots.Count; i++) // 0이 bottom이면 아래부터
            {
                var cat = col.Slots[i].Cat.Value;
                if (cat == null) continue;

                float delay = step * order;
                cat.RequestCompleteAnim(delay);
                order++;
            }
        }
        #endregion

        #region Input Handling (기존 그대로)
        public void BeginTargeting(IEnumerable<int> allowedColumns)
        {
            AllowedTargetColumns.Clear();
            foreach (var idx in allowedColumns.Distinct())
                AllowedTargetColumns.Add(idx);

            InputMode.Value = BoardInputMode.Targeting;
            SetSelectedColumn(null);
        }

        public void EndTargeting()
        {
            InputMode.Value = BoardInputMode.Normal;
            AllowedTargetColumns.Clear();
            SetSelectedColumn(null);
        }

        private bool IsAllowedTarget(int columnIndex)
            => AllowedTargetColumns.Count == 0 || AllowedTargetColumns.Contains(columnIndex);

        private void OnEmptyTap() => SetSelectedColumn(null);

        private void OnColumnTapped(int tappedIndex)
        {
            if (Status.Value != GameStatus.Playing)
                return;

            if (tappedIndex < 0 || tappedIndex >= Columns.Count)
                return;

            var tappedCol = Columns[tappedIndex];

            if (tappedCol.IsLocked.Value)
            {
                OnEmptyTap();
                return;
            }

            if (tappedCol.IsBusy)
                return;

            var selected = _selectedColumnIndex;

            if (selected is { } selIdx &&
                selIdx >= 0 && selIdx < Columns.Count &&
                Columns[selIdx].IsBusy)
                return;

            if (IsColumnComplete(tappedIndex))
            {
                SetSelectedColumn(null);
                return;
            }

            if (InputMode.Value == BoardInputMode.Targeting)
            {
                if (!IsAllowedTarget(tappedIndex))
                    return;

                TargetColumnSelected?.Invoke(tappedIndex);
                return;
            }

            if (selected is null && tappedCol.IsDestinationOnly.Value)
                return;

            if (selected is null)
            {
                SetSelectedColumn(tappedIndex);
                return;
            }

            if (IsColumnComplete(selected.Value))
            {
                SetSelectedColumn(null);
                return;
            }

            if (selected == tappedIndex)
            {
                SetSelectedColumn(null);
                return;
            }

            if (!TryGetColumnId(selected.Value, out var fromId) ||
                !TryGetColumnId(tappedIndex, out var toId))
            {
                SetSelectedColumn(null);
                return;
            }

            var response = _moveCatUseCase.Execute(fromId, toId);

            ApplyState(response.State);

            PendingOutcome.Value = response.Status;
            CanUndo.Value = response.CanUndo;

            SetSelectedColumn(null);

            var moveResult = response.MoveResult;

            switch (moveResult.ResultType)
            {
                case MoveResultType.Success:
                    return;

                case MoveResultType.InvalidMove when !IsColumnComplete(tappedIndex):
                    SetSelectedColumn(tappedIndex);
                    break;
            }
        }

        private void OnUndoRequested()
        {
            if (Status.Value != GameStatus.Playing)
                return;

            if (!CanUndo.Value)
                return;

            var response = _undoMoveUseCase.Execute();

            ApplyState(response.State);
            CanUndo.Value = response.CanUndoMore;

            PendingOutcome.Value = GameStatus.Playing;

            SetSelectedColumn(null);
        }

        private void SetSelectedColumn(int? index)
        {
            _selectedColumnIndex = index;
            SelectedColumnIndex.Value = index ?? -1;

            for (int i = 0; i < Columns.Count; i++)
            {
                var col = Columns[i];

                bool isSelected = index.HasValue &&
                                  index.Value == i &&
                                  !col.IsLocked.Value &&
                                  !col.IsDestinationOnly.Value;

                int run = isSelected ? col.MovableRunCountFromTop.Value : 0;
                col.UpdateSelection(isSelected, run);
            }
        }

        private bool IsColumnComplete(int index)
            => index >= 0 && index < Columns.Count && Columns[index].IsComplete.Value;

        private bool TryGetColumnId(int index, out ColumnId id)
        {
            id = default;
            if (index < 0 || index >= Columns.Count)
                return false;

            id = Columns[index].ColumnId;
            return true;
        }
        #endregion

        public void Shuffle()
        {
            throw new NotImplementedException();
        }
    }
}
