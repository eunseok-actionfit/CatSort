using AES.Tools.VContainer.Services.Loading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Presentation.Loading
{
    [DisallowMultipleComponent]
    public sealed class LoadingView : MonoBehaviour
    {
        [Header("UI Refs")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private TMP_Text label;

        [Header("Animation")]
        [SerializeField, Range(0.1f, 5f)] private float smoothSpeed = 2f;

        private float _targetValue;
        private float _currentValue;

        private void OnEnable()
        {
            LoadingBusFacade.OnProgress += HandleProgress;  
            LoadingBusFacade.OnMessage  += HandleMessage;  

            _currentValue = _targetValue = 0f;
            if (progressBar) progressBar.value = 0f;
        }

        private void OnDisable()
        {
            LoadingBusFacade.OnProgress -= HandleProgress;  
            LoadingBusFacade.OnMessage  -= HandleMessage;   
        }

        private void Update()
        {
            if (!progressBar) return;
            _currentValue = Mathf.MoveTowards(_currentValue, _targetValue, Time.deltaTime * smoothSpeed);
            progressBar.value = _currentValue;
        }

        private void HandleProgress(float v) => _targetValue = Mathf.Clamp01(v);
        private void HandleMessage(string msg) { if (label != null) label.text = msg; }
    }
}