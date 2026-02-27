namespace Domain.StageCatalog
{
    public interface IStageCatalog
    {
        int MinLevel { get; }
        int MaxLevel { get; }

        bool IsValid(int level);
    }

    public sealed class StaticStageCatalog : IStageCatalog
    {
        public int MinLevel => 1;
        public int MaxLevel => 116; // 지금은 10스테이지 고정

        public bool IsValid(int level) => level >= MinLevel && level <= MaxLevel;
    }
}