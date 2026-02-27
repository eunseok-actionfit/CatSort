using System;
using System.Collections.Generic;
using App.Ads;
using UnityEngine;
using VContainer;
using Domain;
using Domain.Economy;
using App.Economy.Lives;
using App.Economy.Coin;
using App.Iap;
using Cysharp.Threading.Tasks;
using Infrastructure.PlayerEconomy;
using Presentation;
using VContainer.Unity;


namespace AES.Tools.VContainer.Bootstrap.Framework
{
    [CreateAssetMenu(menuName="Bootstrap/Features/Game Economy", fileName="GameEconomyFeature")]
    public sealed class GameEconomyFeature : AppFeatureSO, IProvideCapabilities
    {
        [SerializeField] private int maxLives = 5;
        [SerializeField] private float regenMinutes = 30f;

        public void Provide(IDictionary<Type, IFeatureCapability> caps)
        {
            caps[typeof(IIapRewardApplierCapability)] =
                new IapRewardApplierCapability();
        }
        
        public override void Install(IContainerBuilder builder, in FeatureContext ctx)
        {
            builder.Register<ITimeProvider, SystemTimeProvider>(Lifetime.Singleton);

            builder.RegisterInstance(new LivesConfig(
                maxLives: maxLives,
                regenInterval: TimeSpan.FromMinutes(regenMinutes),
                initialLives: maxLives));

            builder.Register<RefreshLivesUseCase>(Lifetime.Scoped);
            builder.Register<GetLivesStatusUseCase>(Lifetime.Scoped);
            builder.Register<ConsumeLifeForStageUseCase>(Lifetime.Scoped);

            builder.Register<GetCoinStatusUseCase>(Lifetime.Scoped);
            builder.Register<AddCoinUseCase>(Lifetime.Scoped);
            builder.Register<SpendCoinUseCase>(Lifetime.Scoped);

            builder.Register<LivesViewModel>(Lifetime.Scoped);
            builder.Register<CoinViewModel>(Lifetime.Scoped);
            
            builder.Register<AddUnlimitedLivesMinutesUseCase>(Lifetime.Scoped);
            builder.Register<RemoveAdsUseCase>(Lifetime.Scoped);
            builder.Register<GameIapRewardApplier>(Lifetime.Singleton)
                .As<IIapRewardApplier>();
        }

     
        public override async UniTask Initialize(LifetimeScope rootScope, FeatureContext ctx)
        {
            // 기존 코드
            var repo = await rootScope.Container
                .Resolve<IPlayerEconomyRepository>()
                .LoadAsync();

            if (repo.RemoveAds)
                ADS.SetRuntimeDisabled(true);
            
            if (ctx.Capabilities.TryGetCapability<IIapRewardApplierCapability>(out var cap))
            {
                cap.RewardApplier =
                    rootScope.Container.Resolve<IIapRewardApplier>();
            }
        }
    }
}