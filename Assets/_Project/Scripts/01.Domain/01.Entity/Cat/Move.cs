using System;


namespace Domain
{
    public readonly struct Move
    {
        public readonly ColumnId From;
        public readonly ColumnId To;
        private readonly DateTime PerformedAtUtc;

        public Move(ColumnId from, ColumnId to, DateTime performedAtUtc)
        {
            From = from;
            To = to;
            PerformedAtUtc = performedAtUtc;
        }

        public override string ToString()
            => $"{From.Value} -> {To.Value} @ {PerformedAtUtc:O}";
    }
}