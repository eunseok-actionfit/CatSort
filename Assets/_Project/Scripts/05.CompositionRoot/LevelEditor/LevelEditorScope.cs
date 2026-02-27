using App.Mvvm;
using Domain;
using Presentation;
using VContainer;
using VContainer.Unity;


namespace Composition
{
    public sealed class LevelEditorScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            
            builder.Register<NoopLevelConfigRepository>(Lifetime.Scoped).As<ILevelConfigRepository>();
            
            // 레벨 에디터 VM
            builder.Register<LevelEditorViewModel>(Lifetime.Scoped);
            builder.RegisterComponentInHierarchy<LevelEditorView>();
            
            builder.RegisterEntryPoint<LevelEditorEntryPoint>(Lifetime.Scoped);
        }
    }
}