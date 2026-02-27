namespace Domain
{
    public class ColumnLockRule
    {
        public ColumnId TargetColumnId;
        public readonly CatColor RequiredColor; // 어떤 색으로 꽉 찬 컬럼을 만들면 열리는지

        public ColumnLockRule(ColumnId targetColumnId, CatColor requiredColor)
        {
            TargetColumnId = targetColumnId;
            RequiredColor = requiredColor;
        }
    }
}