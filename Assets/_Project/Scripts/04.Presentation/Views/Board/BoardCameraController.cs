using DG.Tweening;
using UnityEngine;

namespace Presentation
{
    [RequireComponent(typeof(Camera))]
    public sealed class BoardCameraController : MonoBehaviour
    {
        [Header("Ref")]
        [SerializeField] private Transform target;
        [SerializeField] private Camera targetCamera;

        [Header("Padding (World Units)")]
        [SerializeField] private float paddingX = 1f;
        [SerializeField] private float paddingY = 1f;

        [Header("Offset (World Units)")]
        [SerializeField] private Vector2 positionOffset = Vector2.zero;

        [Header("Zoom Limit")]
        [SerializeField] private float minOrthographicSize = 3f;
        [SerializeField] private float maxOrthographicSize = 20f;

        [Header("Smoothing")]
        [SerializeField] private bool smooth = true;
        [SerializeField] private float moveDuration = 0.35f;
        [SerializeField] private Ease moveEase = Ease.OutQuad;
        [SerializeField] private Ease zoomEase = Ease.OutQuad;

        [Header("Auto Refit")]
        [SerializeField] private bool refitOnResolutionChange = true;

        [Header("Watch Mode (Frames)")]
        [Tooltip("Fit 신호 들어온 뒤 몇 프레임 동안 감시할지")]
        [SerializeField] private int watchFrames = 5;

        [Tooltip("몇 프레임 간격으로 DoFitToBoard를 호출할지 (1이면 매 프레임)")]
        [SerializeField] private int watchFrameInterval = 1;

        [Header("Bounds Filter")]
        [SerializeField] private LayerMask boundsLayerMask = ~0; // 기본: 전부 허용
        
        private int _lastScreenWidth;
        private int _lastScreenHeight;

        private Tween _posTween;
        private Tween _sizeTween;

        // 감시용 상태
        private int _watchRemainingFrames;
        private bool _pendingInstant;

        private bool _isAlive;

        private void Reset()
        {
            targetCamera = GetComponent<Camera>();
        }

// #if UNITY_EDITOR
//         private void OnValidate()
//         {
//             // 플레이 중에는 트윈/위치에 손대지 않음
//             if (Application.isPlaying)
//                 return;
//
//             if (targetCamera == null)
//                 targetCamera = GetComponent<Camera>();
//
//             // 에디터 프리뷰용 Fit (instant)
//             DoFitToBoard(instant: true);
//         }
// #endif

        private void Awake()
        {
            _isAlive = true;

            if (!targetCamera)
                targetCamera = GetComponent<Camera>();

            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
        }

        private void OnDisable()
        {
            _isAlive = false;
            KillTweens();
        }

        private void OnDestroy()
        {
            _isAlive = false;
            KillTweens();
        }

        private void Update()
        {
            if (!_isAlive)
                return;

            if (refitOnResolutionChange)
            {
                if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
                {
                    _lastScreenWidth = Screen.width;
                    _lastScreenHeight = Screen.height;

                    FitToBoard(instant: false);
                }
            }
        }

        private void LateUpdate()
        {
            if (!_isAlive)
                return;

            if (_watchRemainingFrames <= 0)
                return;

            if ((_watchRemainingFrames % Mathf.Max(1, watchFrameInterval)) == 0)
            {
                bool instant = _pendingInstant;
                DoFitToBoard(instant);
            }

            _watchRemainingFrames--;
            if (_watchRemainingFrames <= 0)
            {
                _pendingInstant = false;
            }
        }

        public void FitToBoard(bool instant = false)
        {
            RequestFit(instant);
        }

        private void RequestFit(bool instant)
        {
            // 비활성 상태면 요청 무시
            if (!_isAlive || !isActiveAndEnabled)
                return;

            // 한 프레임 안에 여러 번 신호가 들어오면 instant가 한 번이라도 true면 즉시 모드 허용
            if (instant)
                _pendingInstant = true;

            // 새로 신호 들어올 때마다 감시 프레임 수 갱신
            _watchRemainingFrames = Mathf.Max(_watchRemainingFrames, watchFrames);
        }

        private void DoFitToBoard(bool instant)
        {
            if (!_isAlive)
                return;

            if (targetCamera == null || target == null)
                return;

            if (!TryGetBoardBounds(out var bounds))
                return;

            var center = bounds.center + (Vector3)positionOffset;
            var currentPos = targetCamera.transform.position;
            var targetPos = new Vector3(center.x, center.y, currentPos.z);

            float halfWidth = bounds.size.x * 0.5f + paddingX;
            float halfHeight = bounds.size.y * 0.5f + paddingY;

            float aspect = targetCamera.aspect;
            float sizeByHeight = halfHeight;
            float sizeByWidth = halfWidth / Mathf.Max(aspect, 0.0001f);

            float targetSize = Mathf.Max(sizeByHeight, sizeByWidth);
            targetSize = Mathf.Clamp(targetSize, minOrthographicSize, maxOrthographicSize);

            if (!smooth || instant || moveDuration <= 0f)
            {
                KillTweens();
                targetCamera.transform.position = targetPos;
                targetCamera.orthographicSize = targetSize;
                return;
            }

            KillTweens();

            _posTween = targetCamera.transform
                .DOMove(targetPos, moveDuration)
                .SetEase(moveEase)
                .SetLink(gameObject); // 카메라 GO 파괴 시 자동 Kill

            _sizeTween = DOTween.To(
                    () => targetCamera.orthographicSize,
                    v => targetCamera.orthographicSize = v,
                    targetSize,
                    moveDuration)
                .SetEase(zoomEase)
                .SetLink(gameObject); // 동일하게 링크
        }

        private bool TryGetBoardBounds(out Bounds bounds)
        {
            bounds = default;

            if (target == null)
                return false;

            var colliders = target.GetComponentsInChildren<Collider2D>(false);
            if (colliders == null || colliders.Length == 0)
                return false;

            bool first = true;

            foreach (var col in colliders)
            {
                if (col == null)
                    continue;

                // 레이어 마스크 필터
                int layer = col.gameObject.layer;
                if ((boundsLayerMask.value & (1 << layer)) == 0)
                    continue;

                var b = col.bounds;

                if (first)
                {
                    bounds = b;
                    first = false;
                }
                else
                {
                    bounds.Encapsulate(b);
                }
            }

            // 하나도 안 들어갔으면 false
            return !first;
        }


        private void KillTweens()
        {
            if (_posTween != null)
            {
                _posTween.Kill();
                _posTween = null;
            }

            if (_sizeTween != null)
            {
                _sizeTween.Kill();
                _sizeTween = null;
            }
        }
    }
}
