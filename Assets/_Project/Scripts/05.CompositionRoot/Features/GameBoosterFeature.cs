using UnityEngine;
using VContainer;
using App.Booster;
using Presentation;

namespace AES.Tools.VContainer.Bootstrap.Framework
{
    [CreateAssetMenu(menuName="Bootstrap/Features/Game Booster", fileName="GameBoosterFeature")]
    public sealed class GameBoosterFeature : AppFeatureSO
    {
        [SerializeField] private BoosterSettings boosterSettings;

        public override void Install(IContainerBuilder builder, in FeatureContext ctx)
        {
            builder.RegisterInstance(boosterSettings).AsSelf();

            builder.Register<AddBoosterUseCase>(Lifetime.Scoped);
            builder.Register<ConsumeBoosterUseCase>(Lifetime.Scoped);
            builder.Register<GetBoosterCountUseCase>(Lifetime.Scoped);

            builder.Register<BoosterViewModel>(Lifetime.Scoped);
        }
        
        

    }
}