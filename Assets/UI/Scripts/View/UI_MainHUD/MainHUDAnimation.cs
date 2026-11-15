using System;
using DG.Tweening;
using UnityEngine;

namespace SH.Project.UI.MainHUD
{
    [DisallowMultipleComponent]
    public sealed class MainHUDAnimation : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float _openDuration = 0.2f;
        [SerializeField, Min(0.01f)] private float _closeDuration = 0.15f;

        private Tween _fadeTween;

        #region Animation API

        /// <summary>HUD 전체를 현재 알파에서 페이드 인 또는 아웃합니다.</summary>
        /// <param name="group">HUD 전체를 감싸는 Canvas Group입니다.</param>
        /// <param name="visible">페이드 인이면 true입니다.</param>
        /// <param name="onCompleted">페이드가 완료됐을 때 실행합니다.</param>
        /// <param name="onInterrupted">트윈이 외부에서 중단됐을 때 실행합니다.</param>
        /// <returns>페이드를 시작했으면 true입니다.</returns>
        internal bool TryPlay(CanvasGroup group, bool visible, Action onCompleted, Action onInterrupted)
        {
            if (!isActiveAndEnabled || group == null)
                return false;

            StopTween();

            float targetAlpha = visible ? 1f : 0f;
            float duration = Mathf.Max(0.01f, visible ? _openDuration : _closeDuration);

            Tween tween = group.DOFade(targetAlpha, duration);
            _fadeTween = tween;
            tween.SetEase(Ease.Linear);
            tween.SetAutoKill(true);
            tween.SetUpdate(UpdateType.Normal, true);
            tween.OnComplete(() =>
            {
                _fadeTween = null;
                onCompleted();
            });
            tween.OnKill(() =>
            {
                if (_fadeTween != tween)
                    return;

                _fadeTween = null;
                onInterrupted();
            });
            tween.Play();

            return true;
        }

        /// <summary>트윈을 중단하고 HUD 알파를 즉시 지정한 상태로 설정합니다.</summary>
        /// <param name="group">HUD 전체를 감싸는 Canvas Group입니다.</param>
        /// <param name="visible">알파를 1로 설정하려면 true입니다.</param>
        internal void SetVisibleImmediately(CanvasGroup group, bool visible)
        {
            StopTween();

            if (group != null)
                group.alpha = visible ? 1f : 0f;
        }

        #endregion

        #region Unity Messages

        private void OnDisable()
        {
            _fadeTween?.Kill();
        }

        #endregion

        #region Tween

        private void StopTween()
        {
            Tween tween = _fadeTween;
            _fadeTween = null;
            tween?.Kill();
        }

        #endregion
    }
}
