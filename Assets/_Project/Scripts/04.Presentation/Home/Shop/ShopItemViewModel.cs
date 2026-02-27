using AES.Tools;
using UnityEngine;

namespace App.Mvvm
{
    public sealed class ShopItemViewModel
    {
        public enum RewardIconKind
        {
            Coin,
            UnlimitedLivesMin,
            Boosters,   
            RemoveAds,
            Unknown
        }

        public sealed class RewardPreviewVM
        {
            public RewardIconKind Kind;
            public int Amount;
            public bool ShowAmount;
        }

        public string ProductKey { get; }

        public ObservableProperty<string> TitleKey { get; } = new("");
        public ObservableProperty<string> PriceText { get; } = new("9999");

        public ObservableList<RewardPreviewVM> RewardPreviews { get; } = new();

        // View 바인딩용 결과물
        public ObservableProperty<Sprite> BackgroundSprite { get; } = new();

        public ObservableProperty<Sprite> CoinSprite { get; } = new();
        public ObservableProperty<int> CoinAmount { get; } = new();
        public ObservableProperty<bool> HasCoin { get; } = new();

        public ObservableProperty<Sprite> UnlimitedLivesSprite { get; } = new();
        public ObservableProperty<int> UnlimitedLivesMin { get; } = new();
        public ObservableProperty<bool> HasUnlimitedLives { get; } = new();
        
        public ObservableProperty<bool> HasBoosters { get; } = new();
        public ObservableProperty<int> BoosterAmount { get; } = new();

        public ObservableProperty<Sprite> RemoveAdsSprite { get; } = new();
        public ObservableProperty<bool> HasRemoveAds { get; } = new();

        public AsyncCommand BuyCommand { get; }

        public ShopItemViewModel(string productKey)
        {
            ProductKey = productKey;

            BuyCommand = new AsyncCommand(
                executeAsync:  _ => {
                    return IAP.PurchaseByProductKeyAsync(ProductKey);
                },
                canExecute: _ => IAP.IsBound && IAP.IsReady
            );
        }

        public void RebuildVisuals(ShopIconTable icons, ShopThemeTable themes, string bgKey = null)
        {
            BackgroundSprite.Value = null;

            CoinAmount.Value = 0; HasCoin.Value = false; CoinSprite.Value = null;
            UnlimitedLivesMin.Value = 0; HasUnlimitedLives.Value = false; UnlimitedLivesSprite.Value = null;

            HasBoosters.Value = false;
            BoosterAmount.Value = 0;

            HasRemoveAds.Value = false; 
            RemoveAdsSprite.Value = null;

            int coin = 0, lives = 0, boosters = 0;
            bool removeAds = false;

            for (int i = 0; i < RewardPreviews.Count; i++)
            {
                var r = RewardPreviews[i];
                if (r == null) continue;

                switch (r.Kind)
                {
                    case RewardIconKind.Coin: coin += r.Amount; break;
                    case RewardIconKind.UnlimitedLivesMin: lives += r.Amount; break;

                    case RewardIconKind.Boosters:
                        // “항상 세트” 전제면 하나만 있어도 충분하지만,
                        // 혹시 중복이 들어오면 max로 정규화
                        boosters = Mathf.Max(boosters, r.Amount);
                        break;

                    case RewardIconKind.RemoveAds:
                        removeAds = true;
                        break;
                }
            }

            if (coin > 0)
            {
                HasCoin.Value = true;
                CoinAmount.Value = coin;
                CoinSprite.Value = icons != null ? icons.GetCoinSprite(coin) : null;
            }

            if (lives > 0)
            {
                HasUnlimitedLives.Value = true;
                UnlimitedLivesMin.Value = lives;
                UnlimitedLivesSprite.Value = icons != null ? icons.heart : null;
            }

            if (boosters > 0)
            {
                HasBoosters.Value = true;
                BoosterAmount.Value = boosters;
                // 아이콘 3종은 프리팹에 고정 스프라이트로 박아두는 걸 권장
            }

            HasRemoveAds.Value = removeAds;
            if (removeAds)
                RemoveAdsSprite.Value = icons != null ? icons.removeAds : null;

            if (themes != null)
            {
                var key = string.IsNullOrWhiteSpace(bgKey) ? ProductKey : bgKey;
                BackgroundSprite.Value = themes.GetBundleBg(key);
            }
        }
    }
}
