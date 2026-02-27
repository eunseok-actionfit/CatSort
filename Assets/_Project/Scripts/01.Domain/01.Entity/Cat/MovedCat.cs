using System;


namespace Domain
{

    /// <summary>
    /// 이동한 고양이 한 마리 정보.
    /// </summary>
    public class MovedCat
    {
        public readonly CatId CatId;
        public readonly ColumnId ToColumnId;
        public readonly int SlotIndex;

        public MovedCat(CatId catId, ColumnId toColumnId, int slotIndex)
        {
            CatId = catId;
            ToColumnId = toColumnId;
            SlotIndex = slotIndex;
        }

    }
}