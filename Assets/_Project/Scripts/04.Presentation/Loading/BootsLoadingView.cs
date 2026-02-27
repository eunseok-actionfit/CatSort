using AES.Tools.VContainer.Services.Loading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Presentation.Loading
{
    public class BootsLoadingView : MonoBehaviour
    {

        [Header("UI Refs")]
        [SerializeField] private Image fillImage;
        [SerializeField] private TMP_Text label;
    
        [Header("Animation")]
        [SerializeField, Range(0.1f, 5f)] private float smoothSpeed = 2f;

        private float _targetValue;
        private float _currentValue;

        private void OnEnable()
        {
            LoadingBusFacade.OnProgress += HandleProgress;
            LoadingBusFacade.OnMessage += HandleMessage;

            // 초기화
            _currentValue = _targetValue = 0f;
            if (fillImage) fillImage.fillAmount = 0f;
        }

        private void OnDisable()
        {
            LoadingBusFacade.OnProgress -= HandleProgress;
            LoadingBusFacade.OnMessage -= HandleMessage;
        }

        private void Update()
        {
            if (!fillImage) return;
            _currentValue = Mathf.MoveTowards(_currentValue, _targetValue, Time.deltaTime * smoothSpeed);
            fillImage.fillAmount = _currentValue;
        }

        private void HandleProgress(float v)
        {
            _targetValue = Mathf.Clamp01(v);
        }

        private void HandleMessage(string msg)
        {
            if (label)
                label.text = msg;
        }
    }
}