using System.Collections.Generic;
using Domain;


namespace App
{
    public sealed class ColumnStateDto
    {
        public ColumnId ColumnId;
        public int Capacity;
        public bool IsComplete;
        /// <summary>
        /// 아래에서 위로 쌓인 고양이들 순서.
        /// 인덱스 0 = 맨 아래, 마지막 = 맨 위
        /// </summary>
        public readonly List<CatStateDto> CatsFromBottomToTop = new();
        public int LockColor;
        // 도메인 Column.GetMovableRunCountFromTop() 결과
        public int MovableRunCountFromTop;
        
        public ColumnBehaviorType BehaviorType;  
    }
}


