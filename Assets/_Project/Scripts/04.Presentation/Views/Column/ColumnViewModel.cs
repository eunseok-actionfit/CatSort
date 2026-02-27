// ColumnViewModel.cs
using System;
using System.Collections.Generic;
using System.Linq;
using AES.Tools;
using Domain;
using Presentation;
using UnityEngine;

namespace App.Mvvm
{
    public sealed class ColumnViewModel
    {
        public ColumnId ColumnId { get; }
        public int ColumnIndex { get; private set; }

        public int Capacity { get; set; }

        public ObservableList<SlotViewModel> Slots { get; } = new();

        public ObservableProperty<int> MovableRunCountFromTop { get; } = new();
        public ObservableProperty<bool> Selected { get; } = new();

        public ObservableProperty<bool> IsComplete { get; } = new();
        public ObservableProperty<Vector3> LocalPosition { get; } = new();

        public ObservableProperty<ColumnBehaviorType> BehaviorType { get; } = new();
        public ObservableProperty<bool> IsLocked { get; } = new();
        public ObservableProperty<bool> IsDestinationOnly { get; } = new();
        public ObservableProperty<CatColor> LockColor { get; } = new();

        public bool IsBusy => Slots.Any(x => x.IsBusy);
        
        public ObservableProperty<int> JumpSeed { get; } = new(-1);

        private readonly Dictionary<CatId, int> _prevSlotByCat = new();

        public ColumnViewModel(ColumnId id, int columnIndex, int capacity)
        {
            ColumnId = id;
            ColumnIndex = columnIndex;
            Capacity = capacity;

            for (int i = 0; i < capacity; i++)
                Slots.Add(new SlotViewModel(this, i));
        }

        public void SetIndex(int index) => ColumnIndex = index;

        public void SetCapacity(int newCapacity)
        {
            if (newCapacity < 0) newCapacity = 0;

            if (newCapacity == Capacity && newCapacity == Slots.Count)
                return;

            Capacity = newCapacity;

            while (Slots.Count < Capacity)
            {
                int newIndex = Slots.Count;
                Slots.Add(new SlotViewModel(this, newIndex));
            }

            while (Slots.Count > Capacity)
            {
                int lastIndex = Slots.Count - 1;
                ClearSlot(Slots[lastIndex]);
                Slots.RemoveAt(lastIndex);
            }
        }

        private static void ClearSlot(SlotViewModel slot)
        {
            var cat = slot.Cat.Value;
            if (cat == null) return;

            cat.Selected.Value = false;
            slot.Cat.Value = null;
        }

        public void ApplyState(
            ColumnStateDto dto,
            Func<CatStateDto, CatViewModel> catResolver,
            HashSet<CatId> usedCatIds)
        {
            var prev = new Dictionary<CatId, int>(_prevSlotByCat);

            IsComplete.Value = dto.IsComplete;
            MovableRunCountFromTop.Value = dto.MovableRunCountFromTop;

            IsLocked.Value = dto.BehaviorType == ColumnBehaviorType.Hidden;
            IsDestinationOnly.Value = dto.BehaviorType == ColumnBehaviorType.DestinationOnly;

            LockColor.Value = (CatColor)dto.LockColor;
            BehaviorType.Value = dto.BehaviorType;

            int catCount = dto.CatsFromBottomToTop.Count;
            int max = Mathf.Min(Capacity, catCount);

            bool columnComplete = dto.IsComplete;

            for (int i = 0; i < max; i++)
            {
                var catDto = dto.CatsFromBottomToTop[i];
                var slotVm = Slots[i];

                slotVm.ApplyState(catDto, catResolver, usedCatIds);

                var catVm = slotVm.Cat.Value;
                if (catVm != null)
                    catVm.IsOnDestinationOnlyColumn.Value = IsDestinationOnly.Value;

                slotVm.IsComplete.Value = columnComplete && catVm != null;
            }

            for (int i = max; i < Slots.Count; i++)
            {
                var catVm = Slots[i].Cat.Value;
                if (catVm != null)
                    catVm.IsOnDestinationOnlyColumn.Value = IsDestinationOnly.Value;

                Slots[i].Cat.Value = null;
                Slots[i].IsComplete.Value = false;
            }

            _prevSlotByCat.Clear();
            for (int i = 0; i < Slots.Count; i++)
            {
                var cat = Slots[i].Cat.Value;
                if (cat == null) continue;
                _prevSlotByCat[cat.CatId] = i;
            }

            var moved = new List<(CatId catId, int toIndex)>();
            foreach (var kv in _prevSlotByCat)
            {
                var catId = kv.Key;
                var toIdx = kv.Value;
                if (!prev.TryGetValue(catId, out var fromIdx) || fromIdx != toIdx)
                    moved.Add((catId, toIdx));
            }

            if (moved.Count > 0)
            {
                moved.Sort((a, b) => b.toIndex.CompareTo(a.toIndex));
                float step = CatAnimationConfig.JumpStepDelay;

                for (int order = 0; order < moved.Count; order++)
                {
                    int reverseOrder = moved.Count - 1 - order;
                    float delay = reverseOrder * step;

                    var catVm = Slots[moved[order].toIndex].Cat.Value;
                    if (catVm != null)
                        catVm.SetMoveDelay(delay);

                    EventBus<CatMoveRequested>.Raise(
                        new CatMoveRequested(
                            moved[order].catId,
                            new ViewSlotAddr(ColumnIndex, moved[order].toIndex),
                            delay
                        )
                    );
                }
            }
        }

        public void UpdateSelection(bool isSelected, int runFromTop)
        {
            Selected.Value = isSelected;

            float step = CatAnimationConfig.SelectionStepDelay;

            int offOrder = 0;
            for (int i = 0; i < Slots.Count; i++)
            {
                var cat = Slots[i].Cat.Value;
                if (cat == null) continue;
                if (!cat.Selected.Value) continue;

                float delay = offOrder * step;

                EventBus<CatSelectRequested>.Raise(
                    new CatSelectRequested(cat.CatId, false, delay)
                );

                offOrder++;
            }

            if (!isSelected || runFromTop <= 0)
                return;

            // 입력(선택 ON)마다 컬럼 seed 갱신 (Random은 ViewModel에서 "seed만")
            JumpSeed.Value = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

            int onOrder = 0;
            for (int i = Slots.Count - 1; i >= 0 && onOrder < runFromTop; i--)
            {
                var cat = Slots[i].Cat.Value;
                if (cat == null) continue;

                // 선택될 고양이들은 전부 같은 seed를 받는다 (컬럼 단위 동일)
                cat.JumpSeed.Value = JumpSeed.Value;

                float delay = onOrder * step;

                EventBus<CatSelectRequested>.Raise(
                    new CatSelectRequested(cat.CatId, true, delay)
                );

                onOrder++;
            }
        }
    }
}
