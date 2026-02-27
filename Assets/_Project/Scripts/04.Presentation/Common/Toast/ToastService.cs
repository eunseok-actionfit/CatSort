using System;
using System.Collections.Generic;
using System.Threading;
using AES.Tools.UI_Regacy_.Core.Controller;
using Cysharp.Threading.Tasks;


// IUIController

namespace Presentation.Toast
{
    public sealed class ToastService : IToastService
    {
        private readonly IUIController _ui;
        private readonly Queue<(string,float)> _queue = new();
        private bool _running;

        public ToastService(IUIController ui) { _ui = ui; }

        public void Clear() => _queue.Clear();

        public async UniTask ShowAsync(string message, float seconds = 1.2f, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            _queue.Enqueue((message, seconds));
            if (_running) return;
            _running = true;

            try
            {
                while (_queue.Count > 0)
                {
                    var (msg, sec) = _queue.Dequeue();
                    var vm = new { text = msg, duration = sec }; // 네 Toast ViewModel 형태에 맞춰 전달
                    await _ui.ShowInstanceAsync(UIWIndowId.Toast, vm, ct);
                    await UniTask.Delay(TimeSpan.FromSeconds(sec), cancellationToken: ct);

                    // 인스턴스형이면 HideInstanceAsync로 뺄 수도 있음. 등록 구조에 따라 선택.
                }
            }
            catch (OperationCanceledException) { /* ignore */ }
            finally { _running = false; }
        }
    }
}