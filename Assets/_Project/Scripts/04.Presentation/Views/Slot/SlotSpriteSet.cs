using UnityEngine;


namespace Presentation.Slot
{
    [CreateAssetMenu(menuName = "Game/Slot Sprite Set")]
    public sealed class SlotSpriteSet : ScriptableObject
    {
        [Header("Normal")]
        public Sprite top;
        public Sprite middle;
        public Sprite bottom;

        [Header("Locked")]
        public Sprite topLocked;
        public Sprite middleLocked;
        public Sprite bottomLocked;

        public Sprite GetSprite(bool locked, SlotVisualKind kind)
        {
            if (locked)
            {
                return kind switch
                {
                    SlotVisualKind.Top    => topLocked    ? topLocked    : top,
                    SlotVisualKind.Middle => middleLocked ? middleLocked : middle,
                    SlotVisualKind.Bottom => bottomLocked ? bottomLocked : bottom,
                    _                     => middle
                };
            }

            return kind switch
            {
                SlotVisualKind.Top    => top,
                SlotVisualKind.Middle => middle,
                SlotVisualKind.Bottom => bottom,
                _                     => middle
            };
        }
    }
}