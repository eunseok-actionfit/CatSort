using VContainer;
using Infrastructure;
using Infrastructure.Booster;
using Infrastructure.PlayerEconomy;


namespace AES.Tools.VContainer.Bootstrap.Framework
{
    public sealed class GameRepositoriesFeature : AppFeatureSO
    {
        public override void Install(IContainerBuilder builder, in FeatureContext ctx)
        {
            builder.Register<IPlayerProgressRepository, PlayerProgressRepository>(Lifetime.Singleton);
            builder.Register<IPlayerEconomyRepository, PlayerEconomyRepository>(Lifetime.Singleton);
            builder.Register<IPlayerStateRepository, PlayerStateRepository>(Lifetime.Singleton);

            builder.Register<ISettingsRepository, SettingsRepository>(Lifetime.Singleton);

            builder.Register<IBoosterInventoryRepository, BoosterInventoryRepository>(Lifetime.Singleton);

        }
    }
}