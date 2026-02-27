using Cysharp.Threading.Tasks;
using Domain;
using Infrastructure.PlayerEconomy;


namespace Infrastructure
{
    public class PlayerStateRepository : IPlayerStateRepository
    {
        private readonly IPlayerProgressRepository _playerProgressRepo;
        private readonly IPlayerEconomyRepository _playerEconomyRepo;

        public PlayerStateRepository(
            IPlayerProgressRepository playerProgressRepo,
            IPlayerEconomyRepository playerEconomyRepo)
        {
            _playerProgressRepo = playerProgressRepo;
            _playerEconomyRepo = playerEconomyRepo;
        }


        public async UniTask<PlayerState> LoadAsync()
        {
            var player = await _playerProgressRepo.LoadAsync();
            var economy = await _playerEconomyRepo.LoadAsync();

            return new PlayerState(player, economy);
        }

        public async UniTask SaveAsync(PlayerState state)
        {
            await _playerProgressRepo.SaveAsync(state.Progress);
            await _playerEconomyRepo.SaveAsync(state.Economy);
        }
    }
}