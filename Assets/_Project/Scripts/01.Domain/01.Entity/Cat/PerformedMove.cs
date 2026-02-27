using System;

namespace Domain
{
    /// <summary>
    /// 한 번의 Move에서 실제로 몇 마리의 고양이가 옮겨졌는지까지 포함한 기록
    /// </summary>
    public sealed class PerformedMove
    {
        public Move Move { get; }
        public int MovedCount { get; }

        public PerformedMove(Move move, int movedCount)
        {
            if (movedCount < 0)
                throw new ArgumentOutOfRangeException(nameof(movedCount));

            Move       = move;
            MovedCount = movedCount;
        }

        public override string ToString()
            => $"{Move} (moved {MovedCount} cats)";
    }
}