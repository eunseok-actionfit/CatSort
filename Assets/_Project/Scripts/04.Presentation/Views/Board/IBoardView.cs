using App;
using Domain;


namespace Presentation
{
    public interface IBoardView
    {
        void ApplyLayout(BoardLayoutDto layout);
        void EnsureColumns(BoardStateDto state);
        void LayoutColumns();

        void RenderInitial(BoardStateDto state);

        void SpawnCat(ViewSlotAddr addr, CatStateDto catStateDto);
        void RemoveCat(ViewSlotAddr addr);
        void MoveCat(ViewSlotAddr from, ViewSlotAddr to, float delay=0f);
        void SetCatHidden(ViewSlotAddr addr, bool hidden);

        void SetSelection(int? columnIndex);

        /// <summary>마지막 이동 시도 인덱스를 저장해 InvalidMove FX에 사용.</summary>
        void SetLastMoveAttempt(int fromColumnIndex, int toColumnIndex);

        void ShowMoveResult(MoveResultType resultType);
    }
}