using System.Threading;
using Cysharp.Threading.Tasks;


namespace Presentation.Toast
{
    public interface IToastService
    {
        UniTask ShowAsync(string message, float seconds = 1.2f, CancellationToken ct = default);
        void    Clear(); // 큐 초기화
    }
}