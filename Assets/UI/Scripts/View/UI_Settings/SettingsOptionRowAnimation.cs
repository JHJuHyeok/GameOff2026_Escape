using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.Settings
{
    [DisallowMultipleComponent]
    public sealed class SettingsOptionRowAnimation : MonoBehaviour
    {
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private RectTransform _valueVisual;
        [SerializeField, Min(0f)] private float _distance = 12f;
        [SerializeField, Min(0.01f)] private float _duration = 0.14f;

        private float _restingX;
        private Tween _moveTween;

        #region Unity Messages

        private void Awake()
        {
            if (_valueVisual != null) _restingX = _valueVisual.anchoredPosition.x;
        }

        private void OnEnable()
        {
            if (_previousButton != null) _previousButton.onClick.AddListener(HandlePreviousClicked);
            if (_nextButton != null) _nextButton.onClick.AddListener(HandleNextClicked);
        }

        private void OnDisable()
        {
            if (_previousButton != null) _previousButton.onClick.RemoveListener(HandlePreviousClicked);
            if (_nextButton != null) _nextButton.onClick.RemoveListener(HandleNextClicked);

            StopMotion();

            if (_valueVisual != null) SetPositionX(_restingX);
        }

        #endregion

        #region Input Callbacks

        private void HandlePreviousClicked()
        {
            PlaySlide(-1f);
        }

        private void HandleNextClicked()
        {
            PlaySlide(1f);
        }

        #endregion

        #region Animation

        private void PlaySlide(float direction)
        {
            if (_valueVisual == null) return;

            StopMotion();
            SetPositionX(_restingX + Mathf.Max(0f, _distance) * direction);

            Tween tween = _valueVisual.DOAnchorPosX(_restingX, Mathf.Max(0.01f, _duration));
            _moveTween = tween;
            tween.SetEase(Ease.OutQuad);
            tween.SetAutoKill(true);
            tween.SetUpdate(UpdateType.Normal, true);
            tween.OnKill(() =>
            {
                if (_moveTween == tween) _moveTween = null;
            });
            tween.Play();
        }

        private void SetPositionX(float x)
        {
            Vector2 position = _valueVisual.anchoredPosition;
            position.x = x;
            _valueVisual.anchoredPosition = position;
        }

        private void StopMotion()
        {
            Tween tween = _moveTween;
            _moveTween = null;
            tween?.Kill();
        }

        #endregion
    }
}
