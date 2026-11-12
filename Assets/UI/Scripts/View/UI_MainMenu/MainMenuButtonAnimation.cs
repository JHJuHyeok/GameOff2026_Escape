using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SH.Project.UI.MainMenu
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class MainMenuButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _label;
        [SerializeField, Min(1f)] private float _hoverScale = 1.06f;
        [SerializeField, Range(0.01f, 1f)] private float _pressedScale = 0.95f;
        [SerializeField, Min(0.01f)] private float _duration = 0.1f;

        private Button _button;
        private Vector3 _restingScale;
        private Tween _scaleTween;
        private bool _isHovered;
        private bool _isPressed;

        #region Unity Messages

        private void Awake()
        {
            _button = GetComponent<Button>();

            if (_label != null) _restingScale = _label.localScale;
        }

        private void OnDisable()
        {
            _isHovered = false;
            _isPressed = false;
            _scaleTween?.Kill();
            _scaleTween = null;

            if (_label != null) _label.localScale = _restingScale;
        }

        #endregion

        #region Pointer Callbacks

        //포인트가 버튼 위로 오면 호버 연출 처리
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isActiveAndEnabled || !_button.IsInteractable()) return;

            _isHovered = true;
            AnimateScale(_hoverScale);
        }

        ///포인트가 버튼 밖으로 나가면 호버 연출 해제
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isActiveAndEnabled) return;

            _isHovered = false;
            _isPressed = false;
            AnimateScale(1f);
        }

        //왼쪽 버튼을 누르면 눌린 연출 처리
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isActiveAndEnabled || !_button.IsInteractable() || eventData.button != PointerEventData.InputButton.Left) return;

            _isPressed = true;
            AnimateScale(_pressedScale);
        }

        //왼쪽 버튼을 떼면 눌린 연출 해제
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isActiveAndEnabled || !_isPressed || eventData.button != PointerEventData.InputButton.Left) return;

            _isPressed = false;
            AnimateScale(_isHovered && _button.IsInteractable() ? _hoverScale : 1f);
        }

        #endregion

        #region Animation

        private void AnimateScale(float multiplier)
        {
            if (_label == null) return;

            _scaleTween?.Kill();

            Tween tween = _label.DOScale(_restingScale * multiplier, Mathf.Max(0.01f, _duration));
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
