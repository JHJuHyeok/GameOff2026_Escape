using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SH.Project.UI.Result
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class ResultActionButtonAnimation : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private Graphic _visual;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Color _hoverVisualColor = Color.red;
        [SerializeField] private Color _hoverLabelColor = Color.black;
        [SerializeField, Min(0.01f)] private float _hoverDuration = 0.09f;
        [SerializeField, Range(0.8f, 1f)] private float _pressedScale = 0.96f;
        [SerializeField, Min(0.01f)] private float _pressDuration = 0.035f;
        [SerializeField, Range(0f, 0.15f)] private float _clickPunchScale = 0.055f;
        [SerializeField, Min(0.01f)] private float _punchDuration = 0.12f;

        private Vector3 _defaultScale;
        private Color _defaultVisualColor;
        private Color _defaultLabelColor;
        private Tween _colorTween;
        private Tween _scaleTween;
        private bool _configured;
        private bool _isHovered;
        private bool _hoverPending;
        private int? _pressedPointerId;

        #region Properties

        private bool CanAnimate => _configured && isActiveAndEnabled && _button.isActiveAndEnabled && _button.IsInteractable();

        #endregion

        #region Unity Messages

        private void Awake()
        {
            _configured = _button != null && _button.gameObject == gameObject && _visual != null && _label != null && _visual != _label;
            if (!_configured)
            {
                Debug.LogError("ResultActionButtonAnimation의 Button, Visual, Label 연결을 확인해 주세요.", this);
                enabled = false;
                return;
            }

            _defaultScale = _visual.rectTransform.localScale;
            _defaultVisualColor = _visual.color;
            _defaultLabelColor = _label.color;
        }

        private void OnEnable()
        {
            if (_configured) _button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            if (!_configured) return;

            _button.onClick.RemoveListener(HandleClick);
            ResetFeedback();
        }

        private void LateUpdate()
        {
            if (_hoverPending) RefreshHover();
        }

        #endregion

        #region Pointer Callbacks

        // 포인터가 들어오면 호버 색상을 적용합니다. 입력 잠금 중에는 해제될 때까지 기다립니다.
        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            _hoverPending = true;
            RefreshHover();
        }

        // 포인터가 벗어나면 기본 색상으로 복귀합니다.
        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            _hoverPending = false;
            RefreshHover();
        }

        // 왼쪽 포인터를 누르면 다음 눌림 연출을 위해 기준 크기로 복귀합니다.
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!CanAnimate || _pressedPointerId.HasValue || eventData.button != PointerEventData.InputButton.Left) return;

            _pressedPointerId = eventData.pointerId;
            _scaleTween?.Kill();
            _scaleTween = _visual.rectTransform.DOScale(_defaultScale, Mathf.Max(0.01f, _pressDuration))
                .SetEase(Ease.OutQuad).SetUpdate(UpdateType.Normal, true)
                .SetAutoKill(true).SetRecyclable(false).Play();
        }

        // 같은 포인터를 버튼 위에서 떼면 짧은 눌림 연출을 재생합니다.
        public void OnPointerUp(PointerEventData eventData)
        {
            if (_pressedPointerId != eventData.pointerId || eventData.button != PointerEventData.InputButton.Left) return;

            _pressedPointerId = null;
            if (CanAnimate && _isHovered) PlayPress(false);
        }

        #endregion

        #region Button Feedback

        private void RefreshHover()
        {
            if (!CanAnimate) return;

            _hoverPending = false;
            _colorTween?.Kill();
            float duration = Mathf.Max(0.01f, _hoverDuration);
            Color visualColor = _isHovered ? _hoverVisualColor : _defaultVisualColor;
            Color labelColor = _isHovered ? _hoverLabelColor : _defaultLabelColor;

            Sequence sequence = CreateSequence();
            _colorTween = sequence;
            sequence.Append(_visual.DOColor(visualColor, duration));
            sequence.Join(_label.DOColor(labelColor, duration));
            sequence.Play();
        }

        // Button이 클릭 또는 Submit을 확정하면 눌림과 펀치를 이어서 재생합니다.
        private void HandleClick()
        {
            if (isActiveAndEnabled) PlayPress(true);
        }

        private void ResetFeedback()
        {
            _isHovered = false;
            _hoverPending = false;
            _pressedPointerId = null;
            _colorTween?.Kill();
            _scaleTween?.Kill();
            _colorTween = null;
            _scaleTween = null;
            _visual.color = _defaultVisualColor;
            _label.color = _defaultLabelColor;
            _visual.rectTransform.localScale = _defaultScale;
        }

        #endregion

        #region Scale Animation

        private void PlayPress(bool punch)
        {
            _scaleTween?.Kill();
            float duration = Mathf.Max(0.01f, _pressDuration);
            Vector3 pressedScale = new Vector3(_defaultScale.x * _pressedScale, _defaultScale.y * _pressedScale, _defaultScale.z);
            RectTransform visual = _visual.rectTransform;

            Sequence sequence = CreateSequence();
            _scaleTween = sequence;
            sequence.Append(visual.DOScale(pressedScale, duration).SetEase(Ease.OutQuad));
            sequence.Append(visual.DOScale(_defaultScale, duration).SetEase(Ease.OutQuad));

            if (punch && _clickPunchScale > 0f)
            {
                Vector3 strength = new Vector3(_defaultScale.x, _defaultScale.y, 0f) * _clickPunchScale;
                sequence.Append(visual.DOPunchScale(strength, Mathf.Max(0.01f, _punchDuration), 4, 0.3f));
            }

            sequence.Play();
        }

        private static Sequence CreateSequence()
        {
            return DOTween.Sequence().Pause().SetUpdate(UpdateType.Normal, true).SetAutoKill(true).SetRecyclable(false);
        }

        #endregion
    }
}
