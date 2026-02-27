using AES.Tools.SwitchToggle;
using UnityEngine;

public class SettingGearToggle : SimpleToggle
{
    [SerializeField] private DimView dimView;

    protected override void Awake()
    {
        base.Awake();

        if (dimView != null)
            dimView.Init();       // 초기화는 여기서

        onTurnOn.AddListener(() => dimView?.FadeIn());
        onTurnOff.AddListener(() => dimView?.FadeOut());
    }
}