using DG.Tweening;
using UnityEngine;

namespace SH.Project.UI.QuickSlotHUD
{
    [DisallowMultipleComponent]
    public sealed class QuickSlotAnimation : MonoBehaviour
    {
        [SerializeField] private GameObject _selectedFrame;
        [SerializeField, Range(0f, 1f)] private float _dimmedAlpha = 0.55f;
        [SerializeField, Min(0.01f)] private float _duration = 0.16f;

        private Tween _alphaTween;
        private float _targetAlpha;

        #region Animation API

        /// <summary>슬롯 전체의 알파와 선택 테두리를 갱신합니다.</summary>
        /// <param name="group">슬롯 전체에 적용할 Canvas Group입니다.</param>
        /// <param name="emphasized">알파를 1로 높이려면 true입니다.</param>
        /// <param name="showSelectedFrame">선택 테두리를 표시하려면 true입니다.</param>
        /// <param name="animate">알파를 트윈으로 바꾸려면 true입니다.</param>
        internal void SetPresentation(CanvasGroup group, bool emphasized, bool showSelectedFrame, bool animate)
        {
            if (_selectedFrame != null)
                _selectedFrame.SetActive(showSelectedFrame);

            if (group == null)
                return;

            float alpha = emphasized ? 1f : Mathf.Clamp01(_dimmedAlpha);

            if (!animate || !isActiveAndEnabled)
            {
                StopTween();
                group.alpha = alpha;
                return;
            }

            if (_alphaTween != null && Mathf.Approximately(_targetAlpha, alpha))
                return;

            StopTween();

            if (Mathf.Approximately(group.alpha, alpha))
                return;

            _targetAlpha = alpha;
            Tween tween = group.DOFade(alpha, Mathf.Max(0.01f, _duration));
            _alphaTween = tween;
            tween.SetEase(Ease.OutQuad);
            tween.SetAutoKill(true);
            tween.SetUpdate(UpdateType.Normal, true);
            tween.OnKill(() =>
            {
                if (_alphaTween == tween)
                    _alphaTween = null;
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
            Tween tween = _alphaTween;
            _alphaTween = null;
            tween?.Kill();
        }

        #endregion
    }
}
