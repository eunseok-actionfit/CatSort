#nullable enable
using System;
using System.Collections.Generic;
using AES.Tools;
using Domain;
using Presentation.Slot;
using UnityEngine;

namespace App.Mvvm
{
    public sealed class SlotViewModel
    {
        public ObservableProperty<CatViewModel?> Cat { get; } = new();

        public int SlotIndex { get; private set; }

        public ObservableProperty<bool> Highlight { get; } = new();
        public ObservableProperty<bool> Locked { get; } = new();

        public ObservableProperty<Vector3> LocalPosition { get; } = new(Vector3.zero);

        public ObservableProperty<SlotVisualKind> VisualKind { get; } =
            new(SlotVisualKind.Middle);

        // UI용(컬럼 완성 표시), Busy/완료 애니 판단에는 사용 금지
        public ObservableProperty<bool> IsComplete { get; } = new();
        public ObservableProperty<bool> IsDimmed { get; } = new();


        public ColumnViewModel Column { get; }

        public bool IsBusy => Cat.Value is { CantSelect: true };

        public SlotViewModel(ColumnViewModel column, int slotIndex = -1)
        {
            Column = column ?? throw new ArgumentNullException(nameof(column));
            SlotIndex = slotIndex;
        }

        public void ApplyState(
            CatStateDto? catDto,
            Func<CatStateDto, CatViewModel> catResolver,
            HashSet<CatId> usedCatIds)
        {
            if (catDto == null)
            {
                Cat.Value = null;
                return;
            }

            var dto = catDto;

            var catVm = catResolver(dto);
            catVm.ApplyState(dto); 

            Cat.Value = catVm;
            usedCatIds.Add(catVm.CatId);
        }
    }
}