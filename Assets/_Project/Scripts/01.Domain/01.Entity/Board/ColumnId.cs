using System;
using AES.Tools;


namespace Domain
{
    [Serializable]
    public readonly struct ColumnId : IEquatable<ColumnId>
    {
        public readonly SerializableGuid Value;
        public ColumnId(Guid value) => Value = value;
        public override string ToString() => Value.ToString();
        
        public bool IsEmpty => Value == SerializableGuid.Empty;

        public bool Equals(ColumnId other) => Value.Equals(other.Value);

        public override bool Equals(object obj) => obj is ColumnId other && Equals(other);

        public override int GetHashCode() => Value.GetHashCode();
        
        public static ColumnId Empty => new(Guid.Empty);
        public static ColumnId New() => new ColumnId(Guid.NewGuid());
    }
}


