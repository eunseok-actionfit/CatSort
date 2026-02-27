using Cysharp.Threading.Tasks;
using Domain;


namespace Infrastructure
{
    public interface IPlayerProgressRepository
    {
        PlayerProgress Current { get; } 
        
        UniTask<PlayerProgress> LoadAsync();
        UniTask SaveAsync(PlayerProgress data);
    }
}


