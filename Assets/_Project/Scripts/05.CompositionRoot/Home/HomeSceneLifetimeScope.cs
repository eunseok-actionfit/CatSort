using AES.Tools;
using App.Home;
using App.Mvvm;
using Domain.StageCatalog;
using Infrastructure.PlayerEconomy;
using Presentation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Composition
{
    public class HomeSceneLifetimeScope : LifetimeScope
    {
        public MonoContext HomeContext;
        
        [SerializeField] private ShopThemeTable shopThemeTable;
        [SerializeField] private ShopIconTable shopIconTable;


        protected override void Configure(IContainerBuilder builder)
        {
            // Stage Catalog
            builder.Register<IStageCatalog, StaticStageCatalog>(Lifetime.Scoped);

            // Context
            builder.RegisterInstance(HomeContext).AsSelf();

            builder.RegisterInstance(shopThemeTable);
            builder.RegisterInstance(shopIconTable);
            
            // ---------- Infrastructure ----------
            builder.Register<IPlayerEconomyRepository, PlayerEconomyRepository>(Lifetime.Scoped);

            // (이미 어딘가에 PlayerProgressRepository, IPlayerProgressRepository 등록이 있다면 거기 맞춰 사용)

            // ---------- UseCases ----------
            // Home 쪽 기존
            builder.Register<GetHomeDefaultStageUseCase>(Lifetime.Scoped);
            builder.Register<CanStartStageUseCase>(Lifetime.Scoped);
            builder.Register<AddLifeFromRewardUseCase>(Lifetime.Scoped);
            builder.Register<RefillLivesWithCoinUseCase>(Lifetime.Scoped);
            

            // ---------- ViewModels ----------
            builder.Register<ShopViewModel>(Lifetime.Scoped);
            builder.Register<HomeSceneViewModel>(Lifetime.Scoped);

            // ---------- Views / Entry ----------
         //   builder.RegisterComponentInHierarchy<LobbyHud>();
            builder.RegisterEntryPoint<HomeSceneEntry>(Lifetime.Scoped);
        }
    }
}
