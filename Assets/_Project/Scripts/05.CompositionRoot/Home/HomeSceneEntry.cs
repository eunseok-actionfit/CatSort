using AES.Tools;
using AES.Tools.VContainer;
using MoreMountains.Tools;
using Presentation;
using VContainer.Unity;


namespace Composition
{
    public sealed class HomeSceneEntry : IStartable
    {
        private readonly HomeSceneViewModel _homeVM;
        private readonly MonoContext _context;


        public HomeSceneEntry(HomeSceneViewModel homeVm, MonoContext context)
        {
            _homeVM = homeVm;
            _context = context;
        }
        
        public void Start()
        {
            _context.SetViewModel(_homeVM);
            ADS.HideBanner();
        }
    }
}