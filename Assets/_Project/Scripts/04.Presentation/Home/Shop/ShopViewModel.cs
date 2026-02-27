using System;
using System.Linq;
using AES.Tools;
using UnityEngine;

namespace App.Mvvm
{
    public sealed class ShopViewModel
    {
        public ObservableList<ShopItemViewModel> StarterItems { get; } = new();
        public ObservableList<ShopItemViewModel> BundleItems  { get; } = new();
        public ObservableList<ShopItemViewModel> CoinItems    { get; } = new();

        private readonly IapDatabase _db;
        private readonly ShopIconTable _icons;
        private readonly ShopThemeTable _themes;

        public ShopViewModel(IapDatabase db, ShopIconTable icons, ShopThemeTable themes)
        {
            Debug.Log($"[ShopViewModel] ctor hash={GetHashCode()}");
            _db = db;
            _icons = icons;
            _themes = themes;
            
            IAP.PriceUpdatedByProductKey += SetPriceByProductKey;
            IAP.Ready += RefreshAllCachedPrices;
            BuildLists();
            RefreshAllCachedPrices();
        }

        private void BuildLists()
        {
            StarterItems.Clear();
            BundleItems.Clear();
            CoinItems.Clear();

            var items = _db.EnumerateActiveProductIdsForCurrentPlatform()
                .Select(x =>
                {
                    if (!_db.TryGetProduct(x.productKey, out var meta)) return default;
                    if (!meta.IsVisible) return default;
                    return (ok: true, x.productKey, meta);
                })
                .Where(x => x.ok)
                .OrderBy(x => x.meta.SortOrder)
                .ToList();

            foreach (var it in items)
            {
                var vm = new ShopItemViewModel(it.productKey);

                // 캐시에 이미 있으면 즉시 반영
                if (IAP.TryGetLocalizedPriceByProductKey(it.productKey, out var price) && !string.IsNullOrWhiteSpace(price))
                {
                    vm.PriceText.Value = price;
                    Debug.Log($"[ShopViewModel] BuildLists cached: {it.productKey} => {price}");
                }

                vm.TitleKey.Value =
                    string.IsNullOrEmpty(it.meta.DisplayKey)
                        ? it.meta.AnalyticsId
                        : it.meta.DisplayKey;

                FillRewardPreviews(vm, it.productKey);
                vm.RebuildVisuals(_icons, _themes);

                switch (it.meta.DisplayGroup)
                {
                    case "Starter": StarterItems.Add(vm); break;
                    case "Bundle":  BundleItems.Add(vm);  break;
                    case "Coin":    CoinItems.Add(vm);    break;
                }
            }
        }

        private void RefreshAllCachedPrices()
        {
            RefreshList(StarterItems);
            RefreshList(BundleItems);
            RefreshList(CoinItems);
        }

        private void RefreshList(ObservableList<ShopItemViewModel> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var vm = list[i];
                if (vm == null) continue;

                if (IAP.TryGetLocalizedPriceByProductKey(vm.ProductKey, out var price) && !string.IsNullOrWhiteSpace(price))
                {
                    vm.PriceText.Value = price;
                    Debug.Log($"[IAP ShopViewModel] Refresh cached: {vm.ProductKey} => {price}");
                }
            }
        }

        private void FillRewardPreviews(ShopItemViewModel vm, string productKey)
        {
            vm.RewardPreviews.Clear();

            var rewards = _db.GetRewards(productKey);
            if (rewards == null || rewards.Count == 0)
                return;

            foreach (var r in rewards.OrderBy(x => x.DisplayOrder))
            {
                var kind = MapKind(r.ItemType);

                vm.RewardPreviews.Add(new ShopItemViewModel.RewardPreviewVM
                {
                    Kind = kind,
                    Amount = r.Amount,
                    ShowAmount = kind != ShopItemViewModel.RewardIconKind.RemoveAds
                });
            }

            NormalizeLegacyBoosterRows(vm);
        }

        private static ShopItemViewModel.RewardIconKind MapKind(string itemType)
        {
            if (string.IsNullOrWhiteSpace(itemType))
                return ShopItemViewModel.RewardIconKind.Unknown;

            switch (itemType.Trim())
            {
                case "Coin": return ShopItemViewModel.RewardIconKind.Coin;
                case "UnlimitedLivesMin": return ShopItemViewModel.RewardIconKind.UnlimitedLivesMin;
                case "Boosters": return ShopItemViewModel.RewardIconKind.Boosters;
                case "RemoveAds": return ShopItemViewModel.RewardIconKind.RemoveAds;
                default: return ShopItemViewModel.RewardIconKind.Unknown;
            }
        }

        private static void NormalizeLegacyBoosterRows(ShopItemViewModel vm)
        {
            int maxBooster = 0;

            for (int i = vm.RewardPreviews.Count - 1; i >= 0; i--)
            {
                var p = vm.RewardPreviews[i];
                if (p.Kind != ShopItemViewModel.RewardIconKind.Boosters)
                    continue;

                maxBooster = Math.Max(maxBooster, p.Amount);
                vm.RewardPreviews.RemoveAt(i);
            }

            if (maxBooster > 0)
            {
                vm.RewardPreviews.Add(new ShopItemViewModel.RewardPreviewVM
                {
                    Kind = ShopItemViewModel.RewardIconKind.Boosters,
                    Amount = maxBooster,
                    ShowAmount = true
                });
            }
        }

        public void SetPriceByProductKey(string productKey, string priceText)
        {
            Debug.Log($"[ShopViewModel] SetPriceByProductKey hash={GetHashCode()} {productKey} => {priceText}");

            if (string.IsNullOrWhiteSpace(productKey)) return;

            SetPriceByProductKey(StarterItems, productKey, priceText);
            SetPriceByProductKey(BundleItems, productKey, priceText);
            SetPriceByProductKey(CoinItems, productKey, priceText);
        }

        private static void SetPriceByProductKey(ObservableList<ShopItemViewModel> list, string productKey, string priceText)
        {
            
            for (int i = 0; i < list.Count; i++)
            {
                var vm = list[i];
                if (vm != null && string.Equals(vm.ProductKey, productKey, StringComparison.Ordinal))
                {
                    vm.PriceText.Value = priceText;
                    Debug.Log($"[IAP ShopViewModel] Price applied: {productKey} => {priceText}");
                    return;
                }
            }

            // 여기서 Warning 찍지 말기 (VM 아직 없음은 정상)
            Debug.Log($"[IAP ShopViewModel] Price arrived before VM build: {productKey} => {priceText}");
        }
    }
}
