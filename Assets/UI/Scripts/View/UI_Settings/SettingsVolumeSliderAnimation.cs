using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SH.Project.UI.Settings
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Slider))]
    public sealed class SettingsVolumeSliderAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _handleVisual;
        [SerializeField, Min(1f)] private float _hoverScale = 1.1f;
        [SerializeField, Range(0.01f, 1f)] private float _pressedScale = 0.92f;
        [SerializeField, Min(0.01f)] private float _duration = 0.1f;

        private Slider _slider;
        private Vector3 _defaultScale;
        private Tween _scaleTween;
        private bool _isHovered;
        private int? _pressedPointerId;

        #region Properties

        //슬라이더 연출 가능 여부
        private bool CanAnimate => isActiveAndEnabled && _slider != null && _slider.isActiveAndEnabled && _slider.IsInteractable();

        #endregion

        #region Unity Messages

        private void Awake()
        {
            _slider = GetComponent<Slider>();
            if (_handleVisual != null) _defaultScale = _handleVisual.localScale;
        }

        private void OnDisable()
        {
            _isHovered = false;
            _pressedPointerId = null;
            _scaleTween?.Kill();
            _scaleTween = null;

            if (_handleVisual != null) _handleVisual.localScale = _defaultScale;
        }

        #endregion

        #region Pointer Callbacks

        //호버 상태가 시작되면 손잡이를 호버 상태로 표시
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!CanAnimate) return;

            _isHovered = true;

            if (!_pressedPointerId.HasValue) AnimateScale(_hoverScale);
        }

        //호버 상태가 끝나면 손잡이를 기본 상태로 표시
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isActiveAndEnabled) return;

            _isHovered = false;

            if (!_pressedPointerId.HasValue) AnimateScale(1f);
        }

        //클릭 시 손잡이를 눌린 상태로 표시
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!CanAnimate || _pressedPointerId.HasValue || eventData.button != PointerEventData.InputButton.Left) return;

            _pressedPointerId = eventData.pointerId;
            AnimateScale(_pressedScale);
        }

        //클릭을 때면 손잡이를 호버 상태에 맞춰 표시
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isActiveAndEnabled || _pressedPointerId != eventData.pointerId || eventData.button != PointerEventData.InputButton.Left) return;

            _pressedPointerId = null;
            AnimateScale(_isHovered && CanAnimate ? _hoverScale : 1f);
        }

        #endregion

        #region Animation

        private void AnimateScale(float multiplier)
        {
            if (_handleVisual == null) return;

            _scaleTween?.Kill();

            Tween tween = _handleVisual.DOScale(_defaultScale * multiplier, Mathf.Max(0.01f, _duration));
            _scaleTween = tween;
            tween.SetEase(Ease.OutQuad);
            tween.SetAutoKill(true);
            tween.SetUpdate(UpdateType.Normal, true);
            tween.OnKill(() =>
            {
                if (_scaleTween == tween) _scaleTween = null;
            });
            tween.Play();
        }

        #endregion
    }
}
