using DG.Tweening;
using UnityEngine;

namespace SH.Project.UI.PlayerInfoHUD
{
    [DisallowMultipleComponent]
    public sealed class PlayerInfoAttributeAnimation : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _detailGroup;
        [SerializeField, Min(0f)] private float _slideDistance = 12f;
        [SerializeField, Min(0.01f)] private float _duration = 0.16f;

        private RectTransform _detailRect;
        private float _visibleX;
        private bool _targetVisible;
        private Sequence _sequence;

        #region Animation API

        //상세 영역의 표시 상태를 설정함
        internal void SetVisible(bool visible, bool animate)
        {
            if (!TryCacheLayout()) return;

            float targetAlpha = visible ? 1f : 0f;
            float targetX = visible ? _visibleX : _visibleX - _slideDistance;

            if (!animate || !isActiveAndEnabled)
            {
                StopTween();
                ApplyPose(targetAlpha, targetX);
                return;
            }
            if (_sequence != null && _targetVisible == visible) return;

            StopTween();

            if (Mathf.Approximately(_detailGroup.alpha, targetAlpha) && Mathf.Approximately(_detailRect.anchoredPosition.x, targetX)) return;

            _targetVisible = visible;
            float duration = Mathf.Max(0.01f, _duration);

            Sequence sequence = DOTween.Sequence();
            _sequence = sequence;
            sequence.Pause();
            sequence.SetAutoKill(true);
            sequence.SetUpdate(UpdateType.Normal, true);
            sequence.Append(_detailGroup.DOFade(targetAlpha, duration).SetEase(Ease.OutQuad));
            sequence.Join(_detailRect.DOAnchorPosX(targetX, duration).SetEase(Ease.OutQuad));
            sequence.OnKill(() =>
            {
                if (_sequence != sequence) return;
                _sequence = null;
                ApplyPose(targetAlpha, targetX);
            });
            sequence.Play();
        }

        #endregion

        #region Unity Messages

        private void OnDisable()
        {
            SetVisible(visible: false, animate: false);
        }

        #endregion

        #region Layout and Tween

        private bool TryCacheLayout()
        {
            if (_detailGroup == null) return false;
            if (_detailRect != null) return true;

            _detailRect = _detailGroup.transform as RectTransform;

            if (_detailRect == null) return false;

            _visibleX = _detailRect.anchoredPosition.x;
            return true;
        }

        private void ApplyPose(float alpha, float x)
        {
            _detailGroup.alpha = alpha;
            Vector2 position = _detailRect.anchoredPosition;
            position.x = x;
            _detailRect.anchoredPosition = position;
        }

        private void StopTween()
        {
            Sequence sequence = _sequence;
            _sequence = null;
            sequence?.Kill();
        }

        #endregion
    }
}
