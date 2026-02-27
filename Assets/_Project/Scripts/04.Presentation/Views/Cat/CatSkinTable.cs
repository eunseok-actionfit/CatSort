using System;
using AYellowpaper.SerializedCollections;
using Domain;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Spine/Cat Skin Table")]
public class CatSkinTable : ScriptableObject
{
    [Serializable]
    public struct SkinData
    {
        public string skinKey;   // Spine 애니메이션 스킨 이름
    }

    [SerializeField]
    private SerializedDictionary<CatColor, SkinData> table;

    public bool TryGet(CatColor color, out SkinData data)
    {
        return table.TryGetValue(color, out data);
    }
}