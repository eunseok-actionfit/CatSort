using System.Threading;
using AES.Tools;
using AES.Tools.VContainer;
using App.Gameplay;
using Cysharp.Threading.Tasks;
using Domain;
using VContainer;
using VContainer.Unity;


namespace Composition
{
    public sealed class GameEntryPoint : IAsyncStartable
    {
        private readonly SceneStartArgs _args;
        private readonly LoadLevelUseCase _loadLevelUseCase;
        private readonly GameSceneViewModel _gameSceneViewModel;

        private readonly MonoContext _context;

        public GameEntryPoint(
            [Key("GameScene")] MonoContext context,
            ISceneArgs args,
            GameSceneViewModel gameSceneViewModel)
        {
            _context = context;
            _args = args as SceneStartArgs;
            _gameSceneViewModel = gameSceneViewModel;
        }


        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            _context.SetViewModel(_gameSceneViewModel);
            await _gameSceneViewModel.LoadLevelAsync(_args.StageNumber);
            
            ADS.ShowBanner();
        }
    }
}