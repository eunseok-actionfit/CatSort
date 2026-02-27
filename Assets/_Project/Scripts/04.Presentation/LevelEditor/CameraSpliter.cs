using UnityEngine;


namespace Composition
{
    public class CameraSplitter : MonoBehaviour
    {
        public Rect gameRect = new(0f, 0f, 0.7f, 1f); // 왼쪽 70%
        public Rect editorRect= new(0.7f, 0f, .7f, 1f); // 오른쪽 30%
        
        public Camera editorCam;
        public Camera gameCam;

        // #if UNITY_EDITOR
        // void OnValidate() => Split();
        // #endif

        public void Start() => Split();
        
        void Split()
        {
            gameCam.rect = gameRect;
            editorCam.rect = editorRect;
        }
    }
}