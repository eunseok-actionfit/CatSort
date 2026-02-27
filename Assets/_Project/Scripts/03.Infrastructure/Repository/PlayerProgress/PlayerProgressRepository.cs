using AES.Tools;
using Cysharp.Threading.Tasks;
using Domain;


namespace Infrastructure
{
    public sealed class PlayerProgressRepository : IPlayerProgressRepository
    {
        private readonly IStorageService _storage;
        private readonly ISlotService _slot;

        public PlayerProgress Current { get; private set; } 

        public PlayerProgressRepository(IStorageService storage, ISlotService slot)
        {
            _storage = storage;
            _slot = slot;
        }

        public async UniTask<PlayerProgress> LoadAsync()
        {
            var slotId = _slot.CurrentSlotId;
            var result = await _storage.LoadAsync<PlayerProgressData>(slotId);

            var dto = result.Value ?? new PlayerProgressData();

            Current = new PlayerProgress(dto.lastClearedLevel); 
            return Current;
        }

        public async UniTask SaveAsync(PlayerProgress progress)
        {
            var dto = new PlayerProgressData
            {
                lastClearedLevel = progress.LastClearedLevel
            };

            var slotId = _slot.CurrentSlotId;
            await _storage.SaveAsync(slotId, dto);

            Current = progress;
        }
    }
}