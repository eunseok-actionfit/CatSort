using System;
using System.Collections.Generic;
using AES.Tools;
using Cysharp.Threading.Tasks;
using App.Ads;
using App.Economy.Lives;
using App.Booster;
using App.Economy.Coin;


namespace App.Iap
{
    public sealed class GameIapRewardApplier : IIapRewardApplier
    {
        private readonly AddCoinUseCase _addCoin;
        private readonly AddBoosterUseCase _addBooster;
        private readonly AddUnlimitedLivesMinutesUseCase _addUnlimitedLives;
        private readonly RemoveAdsUseCase _removeAds;

        public GameIapRewardApplier(
            AddCoinUseCase addCoin,
            AddBoosterUseCase addBooster,
            AddUnlimitedLivesMinutesUseCase addUnlimitedLives,
            RemoveAdsUseCase removeAds)
        {
            _addCoin = addCoin;
            _addBooster = addBooster;
            _addUnlimitedLives = addUnlimitedLives;
            _removeAds = removeAds;
        }

        public async UniTask ApplyAsync(IReadOnlyList<IapBundleContentRow> rewards)
        {
            if (rewards == null || rewards.Count == 0)
                return;

            for (int i = 0; i < rewards.Count; i++)
            {
                var r = rewards[i];
                if (r == null) continue;

                var type = (r.ItemType ?? "").Trim();
                var amount = Math.Max(0, r.Amount);

                switch (type)
                {
                    case "Coin":
                        if (amount > 0) await _addCoin.ExecuteAsync(amount);
                        break;

                    case "UnlimitedLivesMin":
                        if (amount > 0) await _addUnlimitedLives.ExecuteAsync(amount);
                        break;

                    case "Boosters":
                        if (amount > 0)
                        {
                            await _addBooster.ExecuteAsync(BoosterId.Undo, amount);
                            await _addBooster.ExecuteAsync(BoosterId.Shuffle, amount);
                            await _addBooster.ExecuteAsync(BoosterId.AddColumn, amount);
                        }
                        break;

                    case "RemoveAds":
                        await _removeAds.ExecuteAsync();
                        break;
                }
            }
        }
        
    }
}
