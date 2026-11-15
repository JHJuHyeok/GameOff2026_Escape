using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SH.Project.UI.CommonPopup
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class CommonPopupButtonAnimation : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Graphic _visual;
        [SerializeField] private TMP_Text _label;
        [SerializeField, FormerlySerializedAs("_hoverBorderColor")] private Color _hoverVisualColor = Color.red;
        [SerializeField] private Color _hoverLabelColor = Color.black;
        [SerializeField, Min(0.01f)] private float _hoverDuration = 0.09f;
        [SerializeField, Range(0.8f, 1f)] private float _pressedScale = 0.96f;
        [SerializeField, Min(0.01f)] private float _pressDuration = 0.035f;
        [SerializeField, Range(0f, 0.15f)] private float _clickPunchScale = 0.055f;
        [SerializeField, Min(0.01f)] private float _punchDuration = 0.12f;

        private Button _button;
        private Vector3 _defaultScale;
        private Color _defaultVisualColor;
        private Color _defaultLabelColor;
        private Tween _colorTween;
        private Tween _scaleTween;
        private bool _initialized;
        private bool _isHovered;
        private bool _hoverPending;
        private int? _pressedPointerId;

        #region Properties

        private bool CanAnimate => _initialized && isActiveAndEnabled && _button.isActiveAndEnabled && _button.IsInteractable();

        #endregion

        #region Unity Messages

        private void Awake()
        {
            _button = GetComponent<Button>();
            _initialized = _visual != null && _label != null;

            if (!_initialized)
            {
                Debug.LogError("CommonPopupButtonAnimation의 Visual과 Label을 연결해 주세요.", this);
                enabled = false;
                return;
            }

            _defaultScale = _visual.rectTransform.localScale;
            _defaultVisualColor = _visual.color;
            _defaultLabelColor = _label.color;
        }

        private void OnEnable()
        {
            if (_initialized) _button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            if (!_initialized) return;
            _button.onClick.RemoveListener(HandleClick);
            ResetFeedback();
        }

        private void LateUpdate()
        {
            if (_hoverPending) RefreshHover();
        }

        #endregion

        #region Pointer Callbacks

        //포인터가 버튼 위에 올라가면 버튼 이미지와 문구 색상을 호버 색상으로 변경
        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            _hoverPending = true;
            RefreshHover();
        }

        //포인터가 나가면 버튼 이미지와 문구에 기본 색상을 적용, 눌린 상태에서 벗어나면 클릭 시 펀치 애니메이션을 재생
        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            _hoverPending = false;
            RefreshHover();
        }

        //포인터를 누르면 버튼이 눌린 상태로 바뀌고, 눌린 상태에서 벗어나면 클릭 시 펀치 애니메이션을 재생
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!CanAnimate || _pressedPointerId.HasValue || eventData.button != PointerEventData.InputButton.Left) return;

            _pressedPointerId = eventData.pointerId;
            _scaleTween?.Kill();
            _scaleTween = _visual.rectTransform.DOScale(_defaultScale, Mathf.Max(0.01f, _pressDuration))
                .SetEase(Ease.OutQuad)
                .SetUpdate(UpdateType.Normal, true)
                .SetAutoKill(true)
                .SetRecyclable(false)
                .Play();
        }

        //포인터를 떼면 버튼이 눌린 상태에서 벗어나고, 클릭 시 펀치 애니메이션을 재생
        public void OnPointerUp(PointerEventData eventData)
        {
            if (_pressedPointerId != eventData.pointerId || eventData.button != PointerEventData.InputButton.Left) return;
            _pressedPointerId = null;
            if (CanAnimate && _isHovered) PlayPress(false);
        }

        #endregion

        #region Button Feedback

        //호버 상태가 바뀌면 버튼 이미지와 문구 색상을 트윈으로 변경
        private void RefreshHover()
        {
            if (!CanAnimate) return;

            _hoverPending = false;
            _colorTween?.Kill();
            float duration = Mathf.Max(0.01f, _hoverDuration);
            Color visualColor = _isHovered ? _hoverVisualColor : _defaultVisualColor;
            Color labelColor = _isHovered ? _hoverLabelColor : _defaultLabelColor;

            Sequence sequence = DOTween.Sequence();
            _colorTween = sequence;
            sequence.Pause().SetUpdate(UpdateType.Normal, true).SetAutoKill(true).SetRecyclable(false);
            sequence.Append(DOTween.To(() => _visual.color, value => _visual.color = value, visualColor, duration));
            sequence.Join(DOTween.To(() => _label.color, value => _label.color = value, labelColor, duration));
            sequence.Play();
        }

        //클릭 이벤트를 처리하고 버튼이 눌렸을 때의 연출을 재생
        private void HandleClick()
        {
            // Button이 클릭을 보낸 뒤 다른 리스너가 닫기를 요청해도 퇴장 중 펀치를 재생
            if (isActiveAndEnabled) PlayPress(true);
        }

        //트윈을 멈추고 버튼 상태를 초기화
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
            RectTransform visualTransform = _visual.rectTransform;

            Sequence sequence = DOTween.Sequence();
            _scaleTween = sequence;
            sequence.Pause().SetUpdate(UpdateType.Normal, true).SetAutoKill(true).SetRecyclable(false);
            sequence.Append(visualTransform.DOScale(pressedScale, duration).SetEase(Ease.OutQuad));
            sequence.Append(visualTransform.DOScale(_defaultScale, duration).SetEase(Ease.OutQuad));

            if (punch && _clickPunchScale > 0f)
            {
                Vector3 strength = new Vector3(_defaultScale.x, _defaultScale.y, 0f) * _clickPunchScale;
                sequence.Append(visualTransform.DOPunchScale(strength, Mathf.Max(0.01f, _punchDuration), 4, 0.3f));
            }

            sequence.Play();
        }

        #endregion
    }
}
