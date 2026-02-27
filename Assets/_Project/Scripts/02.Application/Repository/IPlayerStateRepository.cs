using Cysharp.Threading.Tasks;
using Domain;


namespace Infrastructure
{
    public interface IPlayerStateRepository
    {
        UniTask<PlayerState> LoadAsync();
        UniTask SaveAsync(PlayerState data);
    }
}


