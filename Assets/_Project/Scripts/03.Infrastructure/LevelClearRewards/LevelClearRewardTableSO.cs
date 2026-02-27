using System;
using System.Collections.Generic;
using System.Linq;
using App;
using AYellowpaper.SerializedCollections;
using Domain;
using Domain.Booster;
using UnityEngine;

namespace Infrastructure.LevelClearRewards
{
    [CreateAssetMenu(
        fileName = "LevelClearRewardTable",
        menuName = "Config/Rewards/Level Clear Reward Table")]
    public sealed class LevelClearRewardTableSO
        : ScriptableObject, ILevelClearRewardTable
    {
        [Serializable]
        public struct BoosterReward
        {
            public BoosterId id;
            public int amount;
        }

        [Serializable]
        public struct Reward
        {
            public int coin;
            public int lives; // 보상용(상한 무시)
            public List<BoosterReward> boosters;
        }

        [SerializeField]
        private SerializedDictionary<LevelDifficulty, Reward> table = new();

        public LevelClearRewardBundle GetReward(LevelDifficulty difficulty)
        {
            if (!table.TryGetValue(difficulty, out var r))
                return default;

            var boosters =
                (r.boosters == null || r.boosters.Count == 0)
                    ? Array.Empty<(BoosterId id, int amount)>()
                    : r.boosters
                        .Where(x => x.amount > 0)
                        .Select(x => (x.id, x.amount))
                        .ToArray();

            return new LevelClearRewardBundle(
                coin: Mathf.Max(0, r.coin),
                lives: Mathf.Max(0, r.lives),
                boosters: boosters
            );
        }
    }
}