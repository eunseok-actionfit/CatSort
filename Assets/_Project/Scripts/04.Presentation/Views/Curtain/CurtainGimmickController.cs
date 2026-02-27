using UnityEngine;
using UnityEngine.Playables;


public class CurtainGimmickController : MonoBehaviour
{
    [SerializeField] private PlayableDirector openDirector;

    public void PlayOpen()
    {
        openDirector.time = 0;
        openDirector.Play();
    }
    

    public void ResetTimeline()
    {
        if (openDirector == null)
            return;

        // 1. 0초로 이동
        openDirector.time = 0;

        // 2. 0초 상태를 강제로 평가(적용)
        openDirector.Evaluate();

        // 3. 그 프레임에서 멈춰 있도록 Pause
        openDirector.Pause();
    }

    // Timeline SignalTrack에서 호출
    public void OnOpenFinished()
    {
        // todo 활성화 이후 로직
    }
}