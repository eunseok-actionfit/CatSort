using System.Linq;
using Domain;

namespace App
{
    public static class BoardMapper
    {
        public static BoardStateDto ToDto(Board board)
        {
            var result = new BoardStateDto();

            foreach (var column in board.Columns)
            {
                
                var colDto = new ColumnStateDto
                {
                    ColumnId      = column.Id,
                    Capacity      = column.Capacity,
                    IsComplete    = column.IsUniformAndFull(),
                    MovableRunCountFromTop = column.GetMovableRunCountFromTop(),
                    LockColor = column.LockColor != null ? (int)column.LockColor : 0,
                    BehaviorType = column.BehaviorType
                };

                foreach (var cat in column.Cats.Reverse()) // bottom → top
                {
                    var catDto = new CatStateDto
                    {
                        CatId   = cat.Id,
                        Color   = (int)cat.Color,
                        IsHidden  = cat.IsHidden,
                        IsCompleted = cat.IsCompleted
                    };

                    colDto.CatsFromBottomToTop.Add(catDto);
                }

                result.Columns.Add(colDto);
            }

            return result;
        }
    }
}