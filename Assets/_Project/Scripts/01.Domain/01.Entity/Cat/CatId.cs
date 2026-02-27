using System;
using AES.Tools;


namespace Domain
{
    [Serializable]
    public readonly struct CatId : IEquatable<CatId>
    {
        public readonly SerializableGuid Value;
        public CatId(Guid value) => Value = value;
        public bool IsEmpty => Value == SerializableGuid.Empty;

        public override string ToString() => Value.ToString();

        public bool Equals(CatId other) => Value.Equals(other.Value);

        public override bool Equals(object obj) => obj is CatId other && Equals(other);

        public override int GetHashCode() => Value.GetHashCode();
        public static CatId New() => new CatId(Guid.NewGuid());
    }
}


