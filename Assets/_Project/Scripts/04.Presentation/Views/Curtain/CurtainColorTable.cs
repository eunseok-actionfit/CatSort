using System;
using AYellowpaper.SerializedCollections;
using Domain;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Curtain Color Table")]
public class CurtainColorTable : ScriptableObject
{
    [Serializable]
    public struct CurtainSprites
    {
        public Sprite header;
        public Sprite fabric;
    }

    // 예: SerializedDictionary<CatColor, CurtainSprites>
    // 프로젝트에서 쓰는 딕셔너리 타입명으로 교체
    [SerializeField]
    private SerializedDictionary<CatColor, CurtainSprites> table;

    public bool TryGet(CatColor color, out CurtainSprites sprites)
    {
        return table.TryGetValue(color, out sprites);
    }
}