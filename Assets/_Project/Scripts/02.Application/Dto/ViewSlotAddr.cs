using System;


namespace App
{
    /// <summary>
    /// 뷰/FX 계층에서 사용하는 슬롯 주소 (ColumnIndex + SlotIndex).
    /// </summary>
    public readonly struct ViewSlotAddr : IEquatable<ViewSlotAddr>
    {
        public readonly int ColumnIndex;
        public readonly int SlotIndex;

        public ViewSlotAddr(int columnIndex, int slotIndex)
        {
            ColumnIndex = columnIndex;
            SlotIndex   = slotIndex;
        }

        public bool Equals(ViewSlotAddr other) =>
            ColumnIndex == other.ColumnIndex && SlotIndex == other.SlotIndex;

        public override bool Equals(object obj) =>
            obj is ViewSlotAddr other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(ColumnIndex, SlotIndex);

        public override string ToString() =>
            $"C{ColumnIndex}-S{SlotIndex}";
    }
}