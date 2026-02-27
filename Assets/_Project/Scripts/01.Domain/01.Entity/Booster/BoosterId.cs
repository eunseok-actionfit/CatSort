using AES.Tools;

using AES.Tools;


// DictKeyAttribute

public enum BoosterId
{
    [DictKey(3)] Undo,       // 기본 3개
    [DictKey(3)] Shuffle,    // 기본 2개
    [DictKey(3)] AddColumn,  // 기본 1개
}
