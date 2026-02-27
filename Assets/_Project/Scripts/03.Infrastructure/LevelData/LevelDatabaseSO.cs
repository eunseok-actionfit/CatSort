using System.Collections.Generic;
using UnityEngine;


namespace Infrastructure
{
    [CreateAssetMenu(fileName = "LevelDefinitionDatabase", menuName = "KittySort/LevelDatabase")]
    public class LevelDatabaseSO : ScriptableObject
    {
        public List<LevelDefinitionSO> levels = new();

        public LevelDefinitionSO GetLevelAsset(int levelId)
        {
            return levels.Find(x => x != null && x.levelId == levelId);
        }
    }
}