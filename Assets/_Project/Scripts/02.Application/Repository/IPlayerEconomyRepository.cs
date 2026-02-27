using Cysharp.Threading.Tasks;


namespace Infrastructure.PlayerEconomy
{
    public interface IPlayerEconomyRepository
    {
        Domain.Economy.PlayerEconomy Current { get; } 
        UniTask<Domain.Economy.PlayerEconomy> LoadAsync();
        UniTask SaveAsync(Domain.Economy.PlayerEconomy data);
    }
}


