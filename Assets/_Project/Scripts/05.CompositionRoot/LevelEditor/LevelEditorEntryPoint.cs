using App.Mvvm;
using App.State;
using Presentation;
using VContainer.Unity;


namespace Composition
{
    public sealed class LevelEditorEntryPoint : IStartable
    {
        private readonly LevelEditorViewModel _vm;
        private readonly LevelEditorView _view;

        public LevelEditorEntryPoint(LevelEditorViewModel vm, LevelEditorView view, BoardViewModel boardViewModel)
        {
            _vm = vm;
            _view = view;
        }

        public void Start()
        {
            _view.SetViewModel(_vm);
            _vm.ChangeState(GameStatus.Editing);
        }

    }
}