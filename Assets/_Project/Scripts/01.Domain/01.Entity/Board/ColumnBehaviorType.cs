namespace Domain
{
    public enum ColumnBehaviorType
    {
        Normal = 0,          // 기본: from/to 모두 가능
        Hidden = 1,          // 가림막: 조건 충족 시 Unlock
        DestinationOnly = 2, // To 전용: 들어오는 건 되지만 from 선택 불가
        // 앞으로 추가 가능
    }
}