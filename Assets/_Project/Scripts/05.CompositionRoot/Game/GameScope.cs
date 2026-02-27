using AES.Tools;
using App;
using App.Booster;
using App.Gameplay;
using App.Mvvm;
using Domain;
using Infrastructure;
using Infrastructure.LevelClearRewards;
using Presentation;
using UnityEngine;
using VContainer;
using VContainer.Unity;


namespace Composition
{
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [Header("References")]
        [SerializeField] private BoardCameraController boardCameraController;
        [SerializeField] private LevelDatabaseSO levelDatabase;

        [SerializeField] private MonoContext gameContext;
        
        [Header("Config")]
        [SerializeField] private int startLevelId = 1;
        [SerializeField] private LevelClearRewardTableSO rewardTable;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(gameContext).Keyed("GameScene").AsSelf();
            builder.Register<GameSceneViewModel>(Lifetime.Scoped);
            
            builder.RegisterInstance<ILevelClearRewardTable>(rewardTable);
            
            // BoardViewModel 등록
            builder.Register<BoardViewModel>(Lifetime.Scoped);
            
            // BoosterVM
            builder.RegisterFactory<BoosterId, BoosterViewModel>(r => {
                var getBoosterCountUseCase = r.Resolve<GetBoosterCountUseCase>();
                return (id) => new BoosterViewModel(id, getBoosterCountUseCase);
            }, Lifetime.Scoped);

            builder.Register<BoosterBarViewModel>(Lifetime.Scoped);
            
            // Camera
            builder.RegisterComponent(boardCameraController);
            
            // Repository/Config
            builder.Register<InMemoryBoardRepository>(Lifetime.Scoped).As<IBoardRepository>();
            builder.Register<InMemoryMoveHistoryRepository>(Lifetime.Scoped).As<IMoveHistoryRepository>();
            builder.Register<LevelConfigAddressablesRepository>(Lifetime.Scoped).As<ILevelConfigRepository>();
            builder.Register<LevelDifficultyRewardTable>(Lifetime.Scoped).As<ILevelDifficultyRewardTable>();
            
            // UseCase
            builder.Register<LoadLevelUseCase>(Lifetime.Scoped);
            builder.Register<MoveCatUseCase>(Lifetime.Scoped);
            builder.Register<UndoMoveUseCase>(Lifetime.Scoped);
            builder.Register<AddColumnUseCase>(Lifetime.Scoped);
            builder.Register<SaveLevelClearProgressUseCase>(Lifetime.Scoped);
            builder.Register<GrantLevelClearRewardsUseCase>(Lifetime.Scoped);
            builder.Register<HandleLevelClearedUseCase>(Lifetime.Scoped);
            
            // EntryPoint
            builder.RegisterEntryPoint(r =>
            {
                if (!r.TryResolve<ISceneArgs>(out var args))
                    args = new SceneStartArgs(startLevelId);

                var vm        = r.Resolve<GameSceneViewModel>();
              //  var view      = boardView;
                var context   = gameContext;

                // ViewModel -> View 연결
              //  view.SetViewModel(vm);

                return new GameEntryPoint(context, args,vm);
            }, Lifetime.Scoped);
        }
    }
}