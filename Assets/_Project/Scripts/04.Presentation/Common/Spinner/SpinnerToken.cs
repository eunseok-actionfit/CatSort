using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;


namespace Presentation
{
    public readonly struct SpinnerToken : IDisposable, IAsyncDisposable
    {
        private readonly GlobalSpinnerService _svc;
        private readonly int _id;
        internal SpinnerToken(GlobalSpinnerService svc, int id) { _svc = svc; _id = id; }
        public void Dispose() => _svc?.End(_id);
        public ValueTask DisposeAsync() { _svc?.End(_id); return UniTask.CompletedTask; }
    }
}