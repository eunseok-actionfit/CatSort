// CatSpinePlayer.cs
using System;
using Spine;
using Spine.Unity;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class CatSpinePlayer : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation skeleton;
    [SerializeField] private CatSpineAnimationSet animSet;

    [Header("Speeds")]
    [SerializeField] private float idleSpeed = 1f;
    [SerializeField] private float idleVariantSpeed = 1f;
    [SerializeField] private float jumpSpeed = 1f;
    [SerializeField] private float airIdleSpeed = 1f;
    [SerializeField] private float landSpeed = 1f;
    [SerializeField] private float eatSpeed = 1f;
    [SerializeField] private float completeSpeed = 1f;

    // ✅ static 제거: 고양이(뷰) 단위로 인덱스 보유
    private int _jumpSetIndex = -1;

    private void Awake()
    {
        if (!skeleton)
            skeleton = GetComponentInChildren<SkeletonAnimation>(true);
    }

    // ✅ seed로 점프셋 인덱스 결정 (animSet 길이는 여기서만 앎)
    public void SetJumpSetFromSeed(int seed)
    {
        if (animSet?.jumpSets == null || animSet.jumpSets.Length == 0) return;

        int len = animSet.jumpSets.Length;
        _jumpSetIndex = (int)((uint)seed % (uint)len);
    }

    // (원래 있던 랜덤 함수는 필요하면 유지)
    public void SetJumpSetIndex()
    {
        _jumpSetIndex = Random.Range(0, animSet.jumpSets.Length);
    }

    public void PlayIdleLoop()
    {
        if (!skeleton || animSet == null || animSet.idleLoop == null) return;
        skeleton.timeScale = idleSpeed;
        skeleton.AnimationState.SetAnimation(0, animSet.idleLoop.Animation, true);
    }

    public void PlayIdleVariantOnceThenIdle()
    {
        if (!skeleton || animSet == null) return;
        if (animSet.idleVariants == null || animSet.idleVariants.Length == 0)
        {
            PlayIdleLoop();
            return;
        }

        var clip = animSet.idleVariants[Random.Range(0, animSet.idleVariants.Length)];
        skeleton.timeScale = idleVariantSpeed;

        TrackEntry entry = skeleton.AnimationState.SetAnimation(0, clip.Animation, false);
        entry.Complete += _ => PlayIdleLoop();
    }

    public void PlayJumpAndAirLoop()
    {
        if (!skeleton || animSet == null || animSet.jumpSets == null || animSet.jumpSets.Length == 0) return;

        // 방어코드: seed가 안 들어왔으면 랜덤
        if (_jumpSetIndex < 0 || _jumpSetIndex >= animSet.jumpSets.Length)
            _jumpSetIndex = Random.Range(0, animSet.jumpSets.Length);

        var set = animSet.jumpSets[_jumpSetIndex];

        skeleton.timeScale = jumpSpeed;
        var st = skeleton.AnimationState;

        st.SetAnimation(0, set.jump.Animation, false);

        TrackEntry air = st.AddAnimation(0, set.jumpIdle.Animation, true, 0f);
        air.Complete += _ => skeleton.timeScale = airIdleSpeed;
    }

    public TrackEntry PlayLand(Action onComplete)
    {
        if (!skeleton || animSet == null || animSet.jumpSets == null || animSet.jumpSets.Length == 0) return null;

        if (_jumpSetIndex < 0 || _jumpSetIndex >= animSet.jumpSets.Length)
            _jumpSetIndex = Random.Range(0, animSet.jumpSets.Length);

        var set = animSet.jumpSets[_jumpSetIndex];
        skeleton.timeScale = landSpeed;

        TrackEntry entry = skeleton.AnimationState.SetAnimation(0, set.land.Animation, false);
        if (onComplete != null) entry.Complete += _ => onComplete();
        return entry;
    }

    public TrackEntry PlayComplete(Action onComplete)
    {
        if (!skeleton || animSet == null || animSet.complete == null) return null;

        skeleton.timeScale = completeSpeed;
        TrackEntry entry = skeleton.AnimationState.SetAnimation(0, animSet.complete.Animation, false);
        if (onComplete != null) entry.Complete += _ => onComplete();
        return entry;
    }

    public void PlayEatLoop()
    {
        if (!skeleton || animSet == null || animSet.eatLoop == null) return;
        skeleton.timeScale = eatSpeed;
        skeleton.AnimationState.SetAnimation(0, animSet.eatLoop.Animation, true);
    }

    public void PlayComplete()
    {
        if (!skeleton || animSet == null || animSet.complete == null) return;
        skeleton.timeScale = completeSpeed;
        skeleton.AnimationState.SetAnimation(0, animSet.complete.Animation, false);
    }

    public float GetCompleteDuration()
    {
        if (animSet?.complete?.Animation == null) return 0.5f;
        return animSet.complete.Animation.Duration / Mathf.Max(0.0001f, completeSpeed);
    }
}
