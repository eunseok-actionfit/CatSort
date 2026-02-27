using AES.Tools;
using App.Mvvm;
using Presentation;
using UnityEngine;

public sealed class LevelEditorColumnInputHandler : ContextBindingBase
{
    [Header("입력 소스 (ScriptableObject)")]
    [SerializeField] private InputReader input;

    private LevelEditorViewModel _vm;
    
    protected override void OnContextAvailable(IBindingContext ctx, string path)
    {
        _vm = ctx.GetValue() as LevelEditorViewModel;
        
        if (input != null)
        {
            input.EnablePlayerActions();
            input.OnTap += OnTap;  
        }
    }
        
    protected override void OnContextUnavailable()
    {
        if(input) input.OnTap -= OnTap;
    }

    /// <summary>
    /// 2D 보드 상의 콜라이더를 OverlapPoint로 직접 검사.
    /// </summary>
    private void OnTap(Vector2 worldPos)
    {
        if (_vm?.CurrentLevel.Value == null) return;
        
        var curLevel = _vm.CurrentLevel.Value;
        
        // 2D 물리 기준으로, 해당 지점에 존재하는 콜라이더를 검사
        var hit = Physics2D.OverlapPoint(worldPos);

        if (hit == null)
        {
            curLevel.SelectColumn(null);
            return;
        }

        var col = hit.GetComponentInParent<IColumnIndexProvider>();
        if (col == null)
        {
            curLevel.SelectColumn(0);
            return;
        }

        var index = col.ColumnIndex;
        curLevel.SelectColumn(index);
    }
}