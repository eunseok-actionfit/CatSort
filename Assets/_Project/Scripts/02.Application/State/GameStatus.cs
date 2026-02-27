namespace App.State
{
    public enum GameStatus
    {
        None = 0, Playing = 1, Cleared = 2,
        Failed = 3, Editing = 100, // 에디터 모드
    }
}