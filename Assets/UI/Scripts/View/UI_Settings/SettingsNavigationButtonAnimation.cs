using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SH.Project.UI.Settings
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class SettingsNavigationButtonAnimation : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _label;
        [SerializeField, Min(1f)] private float _hoverScale = 1.06f;
        [SerializeField, Range(0.01f, 1f)] private float _pressedScale = 0.95f;
        [SerializeField, Min(0.01f)] private float _duration = 0.1f;

        private Button _button;
        private Vector3 _defaultScale;
        private Tween _scaleTween;
        private bool _isHovered;
        private int? _pressedPointerId;

        #region Properties

        //연출 가능 확인 프로퍼티
        private bool CanAnimate => isActiveAndEnabled && _button != null && _button.isActiveAndEnabled && _button.IsInteractable();

        #endregion

        #region Unity Messages

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_label != null) _defaultScale = _label.localScale;
        }

        private void OnDisable()
        {
            _isHovered = false;
            _pressedPointerId = null;
            StopTween();

            if (_label != null) _label.localScale = _defaultScale;
        }

        #endregion

        #region Pointer Callbacks

        //호버 시 라벨을 호버 상태로 표시
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!CanAnimate) return;

            _isHovered = true;
            AnimateScale(_pressedPointerId.HasValue ? _pressedScale : _hoverScale);
        }

        //호버 해제 시 라벨을 기본 상태로 표시
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isActiveAndEnabled) return;

            _isHovered = false;
            AnimateScale(1f);
        }

        //누를 때 라벨을 눌린 상태로 표시
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!CanAnimate || _pressedPointerId.HasValue || eventData.button != PointerEventData.InputButton.Left) return;

            _pressedPointerId = eventData.pointerId;
            AnimateScale(_pressedScale);
        }

        //땔 때 라벨을 원래 상태로 표시
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
            if (_label == null) return;

            StopTween();

            Tween tween = _label.DOScale(_defaultScale * multiplier, Mathf.Max(0.01f, _duration));
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

        private void StopTween()
        {
            Tween tween = _scaleTween;
            _scaleTween = null;
            tween?.Kill();
        }

        #endregion
    }
}
