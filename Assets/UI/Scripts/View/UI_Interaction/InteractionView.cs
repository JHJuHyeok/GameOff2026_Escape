using UnityEngine;

namespace SH.Project.UI.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class InteractionView : MonoBehaviour
    {
        private enum Display
        {
            Hidden,
            Prompt,
            Progress
        }

        [SerializeField] private InteractionPromptView _promptView;
        [SerializeField] private InteractionProgressView _progressView;
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private Vector3 _worldOffset = new (0f, 0.5f, 0f);

#if UNITY_EDITOR
        [Header("플레이 모드 테스트")]
        [SerializeField] private Transform _debugTarget;
#endif

        private RectTransform _rectTransform;
        private RectTransform _parentRect;
        private Canvas _canvas;
        private Transform _target;
        private Display _display;

        #region Properties

        //부모 RectTransform과 Canvas, 두 하위 View의 참조가 유효한지 나타냅니다
        public bool IsConfigured { get; private set; }

        #endregion

        #region Public API

        //지정한 월드 대상 위에 전달받은 색으로 상호작용 키를 표시합니다
        public void ShowPrompt(Transform target, Color color)
        {
            if (!IsConfigured) return;

            _promptView.SetColor(color);
            SetDisplay(Display.Prompt, target);
        }

        //지정한 월드 대상 위에 0~1 범위의 초기 진행값으로 게이지를 표시합니다
        public void ShowProgress(Transform target, float normalizedProgress = 0f)
        {
            if (!IsConfigured) return;

            _progressView.SetProgress(normalizedProgress);
            SetDisplay(Display.Progress, target);
        }

        //진행도 표시 상태에서 게이지의 0~1 진행값을 갱신합니다
        public void SetProgress(float normalizedProgress)
        {
            if (_display == Display.Progress) _progressView.SetProgress(normalizedProgress);
        }

        //프롬프트와 게이지를 숨기고 추적 대상을 해제합니다
        public void Hide()
        {
            _display = Display.Hidden;
            _target = null;

            if (IsConfigured) SetVisiblePanels(Display.Hidden);
        }

        #endregion

        #region Unity Messages

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _parentRect = transform.parent as RectTransform;
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            _canvas = parentCanvas != null ? parentCanvas.rootCanvas : null;

            IsConfigured = _parentRect != null && _canvas != null && _promptView != null && _promptView.IsConfigured && _progressView != null && _progressView.IsConfigured;

            if (!IsConfigured)
            {
                Debug.LogError("InteractionView를 Canvas 아래에 배치하고 Prompt View, Progress View, Filled Image를 확인해 주세요.", this);
                return;
            }

            Hide();
        }

        private void LateUpdate()
        {
            // 카메라나 대상이 움직이는 동안 표시 위치를 갱신합니다.
            if (_display != Display.Hidden) RefreshPosition();
        }

        private void OnDisable()
        {
            if (IsConfigured) SetVisiblePanels(Display.Hidden);
        }

        #endregion

        #region Presentation

        // 표시할 패널과 추적 대상을 설정합니다. 대상이 비어 있으면 전체를 숨깁니다.
        private void SetDisplay(Display display, Transform target)
        {
            if (target == null)
            {
                Hide();
                return;
            }

            _display = display;
            _target = target;

            if (isActiveAndEnabled) RefreshPosition();
        }

        // 대상의 월드 위치에 오프셋을 더하고 화면 좌표를 거쳐 부모 RectTransform의 로컬 좌표로 변환합니다.
        private void RefreshPosition()
        {
            if (_target == null)
            {
                Hide();
                return;
            }

            if (_worldCamera == null) _worldCamera = Camera.main;

            // Overlay Canvas의 로컬 좌표 변환에는 카메라를 전달하지 않습니다.
            Camera canvasCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

            if (_worldCamera == null || (_canvas.renderMode != RenderMode.ScreenSpaceOverlay && canvasCamera == null))
            {
                SetVisiblePanels(Display.Hidden);
                return;
            }

            Vector3 screenPoint = _worldCamera.WorldToScreenPoint(_target.position + _worldOffset);
            if (screenPoint.z <= 0f || !RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, screenPoint, canvasCamera, out Vector2 localPoint))
            {
                SetVisiblePanels(Display.Hidden);
                return;
            }

            _rectTransform.localPosition = new Vector3(localPoint.x, localPoint.y, 0f);
            SetVisiblePanels(_display);
        }

        // 현재 표시 모드에 맞춰 프롬프트와 진행도 패널의 표시 여부를 적용합니다.
        private void SetVisiblePanels(Display display)
        {
            _promptView.SetVisible(display == Display.Prompt);
            _progressView.SetVisible(display == Display.Progress);
        }

        #endregion

#if UNITY_EDITOR
        #region Context Menu

        [ContextMenu("테스트/프롬프트 기본색")]
        private void DebugShowPrompt()
        {
            if (Application.isPlaying && _debugTarget != null) ShowPrompt(_debugTarget, Color.white);
        }

        [ContextMenu("테스트/프롬프트 회색")]
        private void DebugShowGrayPrompt()
        {
            if (Application.isPlaying && _debugTarget != null) ShowPrompt(_debugTarget, new Color32(128, 128, 128, 255));
        }

        [ContextMenu("테스트/진행도 표시")]
        private void DebugShowProgress()
        {
            if (Application.isPlaying && _debugTarget != null) ShowProgress(_debugTarget);
        }

        [ContextMenu("테스트/진행도 50%")]
        private void DebugSetProgress()
        {
            if (Application.isPlaying) SetProgress(0.5f);
        }

        [ContextMenu("테스트/숨김")]
        private void DebugHide()
        {
            if (Application.isPlaying) Hide();
        }

        #endregion
#endif
    }
}
