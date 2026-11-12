using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.ScreenTransition
{
    [DisallowMultipleComponent]
    public sealed class ScreenTransitionAnimation : MonoBehaviour
    {
        [SerializeField] private Image _topMask;
        [SerializeField] private Image _bottomMask;
        [SerializeField, Min(0.01f)] private float _coverDuration = 0.4f;
        [SerializeField, Min(0.01f)] private float _revealDuration = 0.4f;

        private RectTransform _topRect;
        private RectTransform _bottomRect;
        private Vector2 _topCoveredPosition;
        private Vector2 _bottomCoveredPosition;
        private float _coverage;
        private Tween _tween;

        #region Animation API

        //덮기,열기 연출 (cover: true = 덮기, false = 열기)
        internal bool TryPlay(bool cover, Action onCompleted, Action onInterrupted)
        {
            if (!isActiveAndEnabled || !TryCacheCoveredPositions()) return false;

            StopTween();

            // 씬 시작 직후에도 현재 Canvas 레이아웃의 마스크 높이를 사용
            Canvas.ForceUpdateCanvases();
            SetCoverage(_coverage);
            SetMasksVisible(true);

            float targetCoverage = cover ? 1f : 0f;
            float fullDuration = cover ? _coverDuration : _revealDuration;
            float remainingCoverage = Mathf.Abs(targetCoverage - _coverage);
            float duration = Mathf.Max(0.01f, fullDuration * remainingCoverage);

            Tween tween = DOTween.To(() => _coverage, SetCoverage, targetCoverage, duration);
            _tween = tween;

            tween.SetEase(Ease.InOutQuad);
            tween.SetAutoKill(true);
            tween.SetUpdate(UpdateType.Normal, true);
            tween.OnComplete(() =>
            {
                _tween = null;
                SetCoverage(targetCoverage);
                if (!cover) SetMasksVisible(false);
                onCompleted();
            });
            tween.OnKill(() =>
            {
                if (_tween != tween) return;
                _tween = null;
                onInterrupted();
            });
            tween.Play();

            return true;
        }

        //트윈을 중단하고 현재 마스크 위치에서 화면을 덮거나 엽니다
        internal bool SetCoveredImmediately(bool covered)
        {
            StopTween();

            if (!TryCacheCoveredPositions())
            {
                if (!covered) SetMasksVisible(false);
                return false;
            }

            SetCoverage(covered ? 1f : 0f);
            SetMasksVisible(covered);
            return true;
        }

        #endregion

        #region Unity Messages

        private void OnDisable()
        {
            _tween?.Kill();
        }

        #endregion

        #region Mask Positions

        private bool TryCacheCoveredPositions()
        {
            if (_topMask == null || _bottomMask == null || _topMask == _bottomMask) return false;
            if (_topRect != null && _bottomRect != null) return true;

            _topRect = _topMask.rectTransform;
            _bottomRect = _bottomMask.rectTransform;
            _topCoveredPosition = _topRect.anchoredPosition;
            _bottomCoveredPosition = _bottomRect.anchoredPosition;
            return true;
        }

        private void SetCoverage(float coverage)
        {
            _coverage = coverage;
            float uncovered = 1f - coverage;

            Vector2 topPosition = _topCoveredPosition;
            topPosition.y += _topRect.rect.height * uncovered;
            _topRect.anchoredPosition = topPosition;

            Vector2 bottomPosition = _bottomCoveredPosition;
            bottomPosition.y -= _bottomRect.rect.height * uncovered;
            _bottomRect.anchoredPosition = bottomPosition;
        }

        private void SetMasksVisible(bool visible)
        {
            if (_topMask != null) _topMask.enabled = visible;
            if (_bottomMask != null) _bottomMask.enabled = visible;
        }

        #endregion

        #region Tween

        private void StopTween()
        {
            Tween tween = _tween;
            _tween = null;
            tween?.Kill();
        }

        #endregion
    }
}
