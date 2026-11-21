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

        [Header("플레이 모드 테스트")]
        [SerializeField] private Transform _debugTarget;

        private RectTransform _rectTransform;
        private RectTransform _parentRect;
        private Canvas _canvas;
        private Transform _target;
        private Display _display;

        #region Properties

        /// <summary>부모 RectTransform과 Canvas, 두 하위 View의 참조가 유효한지 나타냅니다.</summary>
        public bool IsConfigured { get; private set; }

        #endregion

        #region Public API

        /// <summary>대상 위에 상호작용 가능 여부에 맞는 키캡 프롬프트를 표시합니다.</summary>
        public void ShowPrompt(Transform target, bool canInteract)
        {
            if (!IsConfigured) return;

            _promptView.SetInteractable(canInteract);
            SetDisplay(Display.Prompt, target);
        }

        /// <summary>대상 위에 0~1 범위의 초기 진행값으로 게이지를 표시합니다.</summary>
        public void ShowProgress(Transform target, float normalizedProgress = 0f)
        {
            if (!IsConfigured) return;

            _progressView.SetProgress(normalizedProgress);
            SetDisplay(Display.Progress, target);
        }

        /// <summary>진행도 표시 상태에서 게이지의 0~1 진행값을 갱신합니다.</summary>
        public void SetProgress(float normalizedProgress)
        {
            if (_display == Display.Progress) _progressView.SetProgress(normalizedProgress);
        }

        /// <summary>프롬프트와 게이지를 숨기고 추적 대상을 해제합니다.</summary>
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
            if (parentCanvas != null)
            {
                _canvas = parentCanvas.rootCanvas;
            }

            IsConfigured = _parentRect != null && _canvas != null && _promptView != null && _promptView.IsConfigured && _progressView != null && _progressView.IsConfigured;

            if (!IsConfigured)
            {
#if UNITY_EDITOR
                Debug.LogError("InteractionView를 Canvas 아래에 배치하고 Prompt View, Progress View, Filled Image를 확인해 주세요.", this);
#endif
                return;
            }

            Hide();
        }

        private void LateUpdate()
        {
            if (_display != Display.Hidden) RefreshPosition();
        }

        private void OnDisable()
        {
            if (IsConfigured) SetVisiblePanels(Display.Hidden);
        }

        #endregion

        #region Presentation

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

        private void RefreshPosition()
        {
            if (_target == null)
            {
                Hide();
                return;
            }

            if (_worldCamera == null) _worldCamera = Camera.main;

            Camera canvasCamera = null;
            if (_canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                canvasCamera = _canvas.worldCamera;
            }

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

        private void SetVisiblePanels(Display display)
        {
            _promptView.SetVisible(display == Display.Prompt);
            _progressView.SetVisible(display == Display.Progress);
        }

        #endregion

#if UNITY_EDITOR
        #region Context Menu

        [ContextMenu("테스트/상호작용 가능")]
        private void DebugShowPrompt()
        {
            if (Application.isPlaying && _debugTarget != null) ShowPrompt(_debugTarget, true);
        }

        [ContextMenu("테스트/상호작용 불가")]
        private void DebugShowGrayPrompt()
        {
            if (Application.isPlaying && _debugTarget != null) ShowPrompt(_debugTarget, false);
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
