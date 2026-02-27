using System;
using System.Collections.Generic;


namespace Domain
{
    /// <summary>
    /// 이동 결과 + 실제 이동 리스트.
    /// </summary>
    public sealed class MoveResult
    {
        public MoveResultType ResultType { get; }
        public IReadOnlyList<MovedCat> MovedCats { get; }

        public MoveResult(MoveResultType resultType, IReadOnlyList<MovedCat> movedCats)
        {
            ResultType = resultType;
            MovedCats = movedCats;
        }

        public static MoveResult Invalid()
            => new MoveResult(MoveResultType.InvalidMove, Array.Empty<MovedCat>());

        public static MoveResult Success(IReadOnlyList<MovedCat> moved)
            => new MoveResult(MoveResultType.Success, moved);

        public static MoveResult Completed(IReadOnlyList<MovedCat> moved)
            => new MoveResult(MoveResultType.Completed, moved);
    }
}