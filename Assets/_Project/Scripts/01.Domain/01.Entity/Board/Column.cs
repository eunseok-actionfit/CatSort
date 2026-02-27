using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain
{

    public sealed class Column
    {
        private readonly Stack<Cat> _cats;

        public ColumnId Id { get; }
        public int      Capacity { get; }
        public ColumnBehaviorType BehaviorType { get; private set; }

        public bool IsLocked => BehaviorType == ColumnBehaviorType.Hidden;
        public bool IsDestinationOnly => BehaviorType == ColumnBehaviorType.DestinationOnly;

        public CatColor? LockColor { get; }
        
        public Column(
            ColumnId id,
            IEnumerable<Cat> cats,
            int capacity,
            ColumnBehaviorType behaviorType = ColumnBehaviorType.Normal,
            CatColor? lockColor = null)
        {
            Id = id;
            Capacity = capacity;
            BehaviorType = behaviorType;
            LockColor = lockColor;

            _cats = new Stack<Cat>(cats ?? Array.Empty<Cat>());
            if (_cats.Count > Capacity)
                throw new InvalidOperationException("Column over capacity.");
        }

        

        public bool IsEmpty => _cats.Count == 0;
        public bool IsFull  => _cats.Count >= Capacity;
        public int  Count   => _cats.Count;
        public int  RemainingCapacity => Capacity - _cats.Count;

        public IEnumerable<Cat> Cats => _cats; // top → bottom 순서

        public bool TryPeek(out Cat cat)
        {
            if (_cats.Count > 0)
            {
                cat = _cats.Peek();
                return true;
            }

            cat = null;
            return false;
        }

        public void Push(Cat cat)
        {
            if (IsFull)
                throw new InvalidOperationException("Column is full.");

            _cats.Push(cat);
        }

        public Cat Pop()
        {
            if (_cats.Count == 0)
                throw new InvalidOperationException("Column is empty.");

            return _cats.Pop();
        }

        public void UnhideTopCat()
        {
            if (!TryPeek(out var top))
                return;
                    
            var targetColor = top.Color;

            foreach (var cat in _cats)
            {
                if (cat.IsHidden && cat.Color == targetColor)
                {
                    cat.SetHidden(false);
                }
                else break;
            }
        }

        public bool IsUniformAndFull()
        {
            if (IsEmpty) return false;
            if (!IsFull) return false;

            var arr = _cats.ToArray();
            var firstColor = arr[0].Color;
            return firstColor != CatColor.None 
                   && arr.All(c => !c.IsHidden && c.Color == firstColor);
        }
        
        internal void SetUnlocked()
        {
            if (BehaviorType == ColumnBehaviorType.Hidden)
                BehaviorType = ColumnBehaviorType.Normal;
        }


        public int GetMovableRunCountFromTop()
        {
            if (_cats.Count == 0)
                return 0;

            var arr = _cats.ToArray(); // top → bottom
            var top = arr[0];
            if (top.IsHidden) return 0;

            int count = 1;
            for (int i = 1; i < arr.Length; i++)
            {
                if (arr[i].IsHidden || arr[i].Color != top.Color)
                    break;
                count++;
            }

            return count;
        }
    }
}
