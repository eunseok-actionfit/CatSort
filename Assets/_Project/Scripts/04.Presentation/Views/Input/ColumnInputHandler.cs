using AES.Tools;
using App.Mvvm;
using Presentation;
using UnityEngine;

public sealed class ColumnInputHandler : ContextListenerBase
{
    [Header("입력 소스 (ScriptableObject)")]
    [SerializeField] private InputReader input;

    private BoardViewModel _vm;
    
    protected override void OnContextAvailable(IBindingContext ctx)
    {
        _vm = ctx.GetValue() as BoardViewModel;
        
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
        if (_vm == null) return;
        
        // 2D 물리 기준으로, 해당 지점에 존재하는 콜라이더를 검사
        var hit = Physics2D.OverlapPoint(worldPos);

        if (hit == null)
        {
            _vm?.EmptyTapCommand?.Execute();
            return;
        }

        var col = hit.GetComponentInParent<IColumnIndexProvider>();
        if (col == null)
        {
            _vm?.EmptyTapCommand?.Execute();
            return;
        }

        _vm?.ColumnTapCommand?.Execute(col.ColumnIndex);
    }
}