using DG.Tweening;
using UnityEngine;

namespace SH.Project.UI.QuickSlotHUD
{
    [DisallowMultipleComponent]
    public sealed class QuickSlotHUDAnimation : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _expandedOffset = 24f;
        [SerializeField, Min(0.01f)] private float _duration = 0.18f;

        private RectTransform _rect;
        private float _closedY;
        private Tween _moveTween;

        #region Animation API

        /// <summary>열림 상태에서는 HUD를 닫힘 기준점 위로 옮기고, 닫힘 상태에서는 기준점으로 내립니다.</summary>
        /// <param name="expanded">열림 위치로 이동하려면 true입니다.</param>
        /// <param name="animate">이동에 트윈을 사용하려면 true입니다.</param>
        internal void SetExpanded(bool expanded, bool animate)
        {
            if (_rect == null)
            {
                _rect = transform as RectTransform;

                if (_rect == null)
                    return;

                _closedY = _rect.anchoredPosition.y;
            }

            StopTween();

            float y = _closedY + (expanded ? Mathf.Max(0f, _expandedOffset) : 0f);

            if (!animate || !isActiveAndEnabled)
            {
                Vector2 position = _rect.anchoredPosition;
                position.y = y;
                _rect.anchoredPosition = position;
                return;
            }

            if (Mathf.Approximately(_rect.anchoredPosition.y, y))
                return;

            Tween tween = _rect.DOAnchorPosY(y, Mathf.Max(0.01f, _duration));
            _moveTween = tween;
            tween.SetEase(Ease.OutQuad);
            tween.SetAutoKill(true);
            tween.SetUpdate(UpdateType.Normal, true);
            tween.OnKill(() =>
            {
                if (_moveTween == tween)
                    _moveTween = null;
            });
            tween.Play();
        }

        #endregion

        #region Unity Messages

        private void OnDisable()
        {
            StopTween();
        }

        #endregion

        #region Tween

        private void StopTween()
        {
            Tween tween = _moveTween;
            _moveTween = null;
            tween?.Kill();
        }

        #endregion
    }
}
