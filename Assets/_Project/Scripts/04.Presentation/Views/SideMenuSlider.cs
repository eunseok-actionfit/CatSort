using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class SideMenuSlider : MonoBehaviour
{
    [Header("Buttons (자식에서 자동 수집, 필요 시 인스펙터에서 수정 가능)")]
    [SerializeField] private RectTransform[] buttonRects;

    [Header("Motion")]
    [SerializeField] private float slideDistance = 400f;   // 오른쪽 밖으로 뺄 거리
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private float stagger = 0.04f;        // 버튼 간 딜레이
    [SerializeField] private float extraOvershoot = 20f;   // 도착 전에 더 들어갈 거리
    [SerializeField] private Ease easeIn = Ease.OutCubic;
    [SerializeField] private Ease easeOut = Ease.InCubic;

    private Vector2[] originPos;
    private Sequence currentSeq;

    private void Awake()
    {
        if (buttonRects == null || buttonRects.Length == 0)
        {
            Debug.LogWarning($"{name} : buttonRects 비어있음. OnValidate에서 자식 수집이 안 된 듯.");
            return;
        }

        // 원래 위치 저장
        originPos = new Vector2[buttonRects.Length];
        for (int i = 0; i < buttonRects.Length; i++)
        {
            if (buttonRects[i] == null) continue;
            originPos[i] = buttonRects[i].anchoredPosition;
        }

        // 시작은 화면 밖(오른쪽)으로 빼두기
        MoveOffScreenImmediate();
    }

    private void OnDestroy()
    {
        currentSeq?.Kill();
    }

    private void MoveOffScreenImmediate()
    {
        if (buttonRects == null) return;

        for (int i = 0; i < buttonRects.Length; i++)
        {
            if (buttonRects[i] == null) continue;
            buttonRects[i].anchoredPosition = originPos[i] + new Vector2(slideDistance, 0f);
        }
    }

    public void SlideIn()
    {
        if (buttonRects == null || buttonRects.Length == 0) return;

        currentSeq?.Kill();
        currentSeq = DOTween.Sequence();

        float firstPhase = duration * 0.8f;
        float secondPhase = duration * 0.2f;

        for (int i = 0; i < buttonRects.Length; i++)
        {
            var rect = buttonRects[i];
            if (rect == null) continue;

            // 항상 바깥쪽에서 시작
            rect.anchoredPosition = originPos[i] + new Vector2(slideDistance, 0f);

            // 오버슈트 지점 (조금 더 안쪽으로)
            Vector2 overshootPos = originPos[i] + new Vector2(-extraOvershoot, 0f);

            // 1단계: 밖 → 오버슈트 위치
            currentSeq.Insert(i * stagger,
                rect.DOAnchorPos(overshootPos, firstPhase)
                    .SetEase(easeIn));

            // 2단계: 오버슈트 → 원래 위치
            currentSeq.Insert(i * stagger + firstPhase,
                rect.DOAnchorPos(originPos[i], secondPhase)
                    .SetEase(easeOut));
        }
    }

    public void SlideOut()
    {
        if (buttonRects == null || buttonRects.Length == 0) return;

        currentSeq?.Kill();
        currentSeq = DOTween.Sequence();

        for (int i = 0; i < buttonRects.Length; i++)
        {
            var rect = buttonRects[i];
            if (rect == null) continue;

            currentSeq.Insert(i * stagger,
                rect.DOAnchorPos(originPos[i] + new Vector2(slideDistance, 0f), duration)
                    .SetEase(easeOut));
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 프리팹 편집 / 인스펙터 변경 시마다 자식 RectTransform 자동 수집
        var list = new List<RectTransform>();

        foreach (Transform child in transform)
        {
            if (child is RectTransform rt)
                list.Add(rt);
        }

        buttonRects = list.ToArray();
    }
#endif
}
