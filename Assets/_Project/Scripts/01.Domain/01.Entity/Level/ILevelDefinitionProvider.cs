namespace Domain
{
    public interface ILevelDefinitionProvider
    {
        Board CreateInitialBoard(int levelId);
    }
}

