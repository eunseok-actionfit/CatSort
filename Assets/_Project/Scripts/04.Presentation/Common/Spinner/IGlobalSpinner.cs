using System;
using System.Threading;
using Cysharp.Threading.Tasks;


namespace Presentation
{
    /// <summary>
    /// 드물게 "진행 중" 오버레이(스피너/입력차단)를 켰다가 끄는 전역 서비스.
    /// - ref-count로 중첩 호출을 안전하게 처리
    /// - 최소 노출 시간(minShowSec) 보장
    /// - Status 오버레이가 미등록이면 Toast로 대체(fallback)
    /// </summary>
    public interface IGlobalSpinner
    {
        SpinnerToken Begin(string message = "Processing...", bool blockInput = true, float minShowSec = 0.25f);
        UniTask RunAsync(Func<UniTask> flow, string message = "Processing...", bool blockInput = true, float minShowSec = 0.25f, CancellationToken ct = default);
    }
}