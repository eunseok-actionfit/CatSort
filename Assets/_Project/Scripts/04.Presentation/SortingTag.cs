using UnityEngine;


namespace Presentation
{
    public static class SortingTag
    {
        public static int L_Background { get; private set; }
        public static int L_Board      { get; private set; }
        public static int L_Block      { get; private set; }
        public static int L_Effect     { get; private set; }
        public static int L_Overlay    { get; private set; }
        public static int L_UI         { get; private set; }

        public enum Band { Board = 0, Block = 100, Effect = 200, Overlay = 300, UI = 1000 }
        public enum Offset { Back = -10, Default = 0, Front = 10 }

        public static int Order(Band band, Offset offset = Offset.Default) => (int)band + (int)offset;

        static bool _inited;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            if (_inited) return;
            _inited = true;
        
            L_Background = SortingLayer.NameToID("Background");
            L_Board      = SortingLayer.NameToID("Board");
            L_Block      = SortingLayer.NameToID("Block");
            L_Effect     = SortingLayer.NameToID("Effect");
            L_Overlay    = SortingLayer.NameToID("Overlay");
            L_UI         = SortingLayer.NameToID("UI");
        }
    }
}