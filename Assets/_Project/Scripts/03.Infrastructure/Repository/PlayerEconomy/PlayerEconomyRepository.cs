using System;
using AES.Tools;
using Cysharp.Threading.Tasks;
using Domain.Economy;

namespace Infrastructure.PlayerEconomy
{
    public sealed class PlayerEconomyRepository : IPlayerEconomyRepository
    {
        private readonly IStorageService _storage;
        private readonly ISlotService _slot;
        private readonly LivesConfig _config;   

        public PlayerEconomyRepository(
            IStorageService storage,
            ISlotService slot,
            LivesConfig config)             
        {
            _storage = storage;
            _slot    = slot;
            _config  = config;
        }

        public Domain.Economy.PlayerEconomy Current  { get; private set; } 

        public async UniTask<Domain.Economy.PlayerEconomy> LoadAsync()
        {
            var slotId = _slot.CurrentSlotId;
            var result = await _storage.LoadAsync<PlayerEconomyData>(slotId);

            var dto = result.Value;

            if (dto == null)
            {
                dto = new PlayerEconomyData
                {
                    coin  = 0,
                    gem   = 0,
                    lives = _config.InitialLives,
                    livesLastUpdatedUtc = DateTime.UtcNow,
                    unlimitedLivesEndUtc = DateTime.MinValue,
                    removeAds = false
                };

                await _storage.SaveAsync(slotId, dto);
            }

            // DateTime 기본값/Kind 방어
            var lastUpdated = dto.livesLastUpdatedUtc;

            if (lastUpdated == default)
            {
                // 저장이 안 돼 있던 경우 현재 시간으로 초기화
                lastUpdated = DateTime.UtcNow;
            }
            else if (lastUpdated.Kind == DateTimeKind.Unspecified)
            {
                // 직렬화 과정에서 Kind 정보가 사라졌다면 UTC로 간주
                lastUpdated = DateTime.SpecifyKind(lastUpdated, DateTimeKind.Utc);
            }
            
            var endUtc = dto.unlimitedLivesEndUtc;
            if (endUtc.Kind == DateTimeKind.Unspecified)
                endUtc = DateTime.SpecifyKind(endUtc, DateTimeKind.Utc);
            
            Current = new Domain.Economy.PlayerEconomy(
                dto.coin,
                dto.gem,
                dto.lives,
                lastUpdated,
                endUtc,
                dto.removeAds
            );

            return Current;
        }

        public async UniTask SaveAsync(Domain.Economy.PlayerEconomy economy)
        {
            // 항상 UTC로 저장
            var utc = economy.LivesLastUpdatedUtc;
            if (utc.Kind != DateTimeKind.Utc)
                utc = utc.ToUniversalTime();
            
            var endUtc = economy.UnlimitedLivesEndUtc;
            if (endUtc.Kind != DateTimeKind.Utc)
                endUtc = endUtc.ToUniversalTime();


            var dto = new PlayerEconomyData
            {
                coin  = economy.Coin,
                gem   = economy.Gem,
                lives = economy.Lives,
                livesLastUpdatedUtc = utc,
                unlimitedLivesEndUtc = endUtc,
                removeAds = economy.RemoveAds
            };

            var slotId = _slot.CurrentSlotId;
            await _storage.SaveAsync(slotId, dto);
            
            Current = economy;
        }
    }
}
