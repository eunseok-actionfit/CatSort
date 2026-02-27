using System.Threading;
using Cysharp.Threading.Tasks;


namespace Presentation
{
    public interface ITopBar
    {
        void ShowMask(TopBarFeature mask);
        UniTask ShowAsync(CancellationToken ct = default);
    }
}