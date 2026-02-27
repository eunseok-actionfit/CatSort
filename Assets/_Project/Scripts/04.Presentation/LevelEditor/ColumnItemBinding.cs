using AES.Tools;
using AES.Tools.TBC.Collections;
using UnityEngine;
using UnityEngine.UI;


public sealed class ColumnItemBinding : ContextBindingBase
{
    
    [SerializeField] private Image item;
    
    private CompositeDisposable _disposables = new();
    private ColumnConfigViewModel _vm;
    
    protected override void OnContextAvailable(IBindingContext ctx, string path)
    {
        
        _vm = ctx.GetValue() as ColumnConfigViewModel;
        if (_vm == null) return;
        
        _disposables.Dispose();
        _disposables = new CompositeDisposable();
        
        ctx.SubscribeProperty<ColumnConfigViewModel, int>(
            vm => vm.Parent.SelectedColumnIndex,
            index => {
                    var selected = index == _vm.ColumnIndex.Value;
                    item.color = selected ? Color.yellow : Color.white;
                })
            .AddTo(_disposables);
    }

    protected override void OnContextUnavailable()
    {
        _disposables.Dispose();
        _disposables = new CompositeDisposable();
    }
    
}