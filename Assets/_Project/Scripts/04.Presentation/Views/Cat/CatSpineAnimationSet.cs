using System;
using Spine.Unity;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Cat/Spine Animation Set")]
public sealed class CatSpineAnimationSet : ScriptableObject
{
    [Header("Idle")]
    public AnimationReferenceAsset idleLoop;          // A_stand_idle0
    public AnimationReferenceAsset[] idleVariants;    // A_stand_idle1~4

    [Serializable]
    public struct JumpSet
    {
        public AnimationReferenceAsset jump;      // B_Jump_01~03
        public AnimationReferenceAsset jumpIdle;  // C_Jump_idle1~3
        public AnimationReferenceAsset land;      // D_Land_01~03
    }

    [Header("Jump Sets")]
    public JumpSet[] jumpSets;                    // index 0 -> 01셋, 1 -> 02셋 ...

    [Header("Complete")]
    public AnimationReferenceAsset complete;      // F_complete
    
    [Header("Eat")]
    public AnimationReferenceAsset eatLoop;
}