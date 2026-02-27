using System;
using System.Collections.Generic;
using AES.Tools;
using Domain;
using UnityEngine;


namespace Infrastructure
{
    [Serializable]
    public sealed class CatDefinition
    {
        public CatColor color;
        public bool isHidden;
    
    }

    [Serializable]
    public sealed class ColumnDefinition
    {
        [Min(1)]
        public int capacity = 4;
        public List<CatDefinition> catsFromBottomToTop = new();

        public ColumnBehaviorType behaviorType;

        [ShowIf(nameof(behaviorType), ColumnBehaviorType.Hidden)]
         public CatColor requiredColor;
    }

    [CreateAssetMenu(
        fileName = "LevelDefinition",
        menuName = "KittySort/Level Definition",
        order = 0)]
    public sealed class LevelDefinitionSO : ScriptableObject
    {
        [Header("Meta")]
        public int levelId = 1;

        [Header("Layout")]
        [SerializeField] public BoardLayoutType layoutType = BoardLayoutType.Grid;

        [Header("Layout Config")]
        [ShowIf(nameof(layoutType), BoardLayoutType.Grid)]
        [SerializeField] public float spacingX = 2f;
        [ShowIf(nameof(layoutType), BoardLayoutType.Grid)]
        [SerializeField] public float spacingY = 2.5f;
        
        [ShowIf(nameof(layoutType), BoardLayoutType.Grid)]
        [SerializeField] public int gridRowCount = 3;

        [ShowIf(nameof(layoutType), BoardLayoutType.Arc)]
        [SerializeField] public float arcRadius = 5f;
        [ShowIf(nameof(layoutType), BoardLayoutType.Arc)]
        [SerializeField] public float arcStartAngle = -45f;
        [ShowIf(nameof(layoutType), BoardLayoutType.Arc)]
        [SerializeField] public float arcEndAngle = 45f;

        [Header("Columns")]
        public List<ColumnDefinition> columns = new();
    }
}