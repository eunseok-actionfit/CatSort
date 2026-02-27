using System;
using System.Threading;
using AES.Tools.UI_Regacy_.Core.Controller;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace Presentation
{
    public sealed class GlobalSpinnerService : IGlobalSpinner
    {
        private readonly IUIController _ui;
        private readonly UIWIndowId _statusId;
        private readonly UIWIndowId _toastId;

        private int _depth = 0;
        private int _nextToken = 1;
        private float _lastShowTime;    // 최소 노출 보장용
        private string _currentMsg;
        private bool _busy;

        public GlobalSpinnerService(IUIController ui, UIWIndowId statusId = UIWIndowId.Status, UIWIndowId toastId = UIWIndowId.Toast)
        {
            _ui = ui;
            _statusId = statusId;
            _toastId = toastId;
        }

        public SpinnerToken Begin(string message = "Processing...", bool blockInput = true, float minShowSec = 0.25f)
        {
            var tokenId = _nextToken++;
            _ = BeginInternal(message, blockInput, minShowSec); // fire & forget
            return new SpinnerToken(this, tokenId);
        }

        public async UniTask RunAsync(Func<UniTask> flow, string message = "Processing...", bool blockInput = true, float minShowSec = 0.25f, CancellationToken ct = default)
        {
            using var _ = Begin(message, blockInput, minShowSec);
            await flow();
        }

        internal async UniTask BeginInternal(string message, bool blockInput, float minShowSec)
        {
            _depth++;
            _currentMsg = message;

            if (_depth == 1 && !_busy)
            {
                _busy = true;
                _lastShowTime = Time.realtimeSinceStartup;

                // Status 창이 등록돼 있으면 그걸 띄우고, 아니면 Toast로 대체
                if (_ui.IsOpen(_statusId) || TryShowStatus(message, blockInput))
                {
                    // 이미 떠 있거나 정상 표시됨
                }
                else
                {
                    // fallback: Status 미등록이면 Toast 한 번만
                    await _ui.ShowInstanceAsync(_toastId, new { text = message, duration = 1.0f });
                }
            }
        }

        internal async void End(int tokenId)
        {
            if (_depth <= 0) return;
            _depth--;
            if (_depth > 0) return;

            // 최소 노출 시간 보장
            var elapsed = Time.realtimeSinceStartup - _lastShowTime;
            if (elapsed < 0.25f) // 기본값과 동일. 필요 시 매개변수로 저장해 사용해도 OK
                await UniTask.Delay(TimeSpan.FromSeconds(0.25f - elapsed));

            if (_ui.IsOpen(_statusId))
                await _ui.HideAsync(_statusId);

            _busy = false;
            _currentMsg = null;
        }

        private bool TryShowStatus(string message, bool blockInput)
        {
            try
            {
                // Status 오버레이가 있다면 메시지/블로커 정보를 전달 (뷰에서 메시지/블로커 바인딩 처리)
                var vm = new { text = message, block = blockInput };
                // 싱글톤 표현: ShowAsync(id) 사용
                _ = _ui.ShowAsync(_statusId, vm);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GlobalSpinnerService] Status show failed: {e.Message}");
                return false;
            }
        }
    }
}
