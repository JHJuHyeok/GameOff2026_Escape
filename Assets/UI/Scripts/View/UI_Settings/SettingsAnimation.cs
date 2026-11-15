using System;
using DG.Tweening;
using UnityEngine;

namespace SH.Project.UI.Settings
{
    [DisallowMultipleComponent]
    public sealed class SettingsAnimation : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float _openDuration = 0.25f;
        [SerializeField, Min(0.01f)] private float _closeDuration = 0.2f;

        private Sequence _sequence;

        #region Animation API

        //연출 재생 시도
        internal bool TryPlay(CanvasGroup backgroundGroup, CanvasGroup contentGroup, bool open,Action onCompleted, Action onInterrupted)
        {
            if (!isActiveAndEnabled) return false;

            Stop();

            float target = open ? 1f : 0f;
            float duration = Mathf.Max(0.01f, open ? _openDuration : _closeDuration);
            Sequence sequence = DOTween.Sequence();
            _sequence = sequence;

            sequence.Pause();
            sequence.SetAutoKill(true);
            sequence.SetUpdate(UpdateType.Normal, true);
            sequence.Append(backgroundGroup.DOFade(target, duration).SetEase(Ease.Linear));
            sequence.Join(contentGroup.DOFade(target, duration).SetEase(Ease.Linear));

            sequence.OnComplete(() =>
            {
                _sequence = null;
                onCompleted();
            });

            sequence.OnKill(() =>
            {
                if (_sequence != sequence) return;
                _sequence = null;
                onInterrupted();
            });

            sequence.Play();
            return true;
        }

        //숨김 상태로 초기화
        internal void ResetToHidden(CanvasGroup backgroundGroup, CanvasGroup contentGroup)
        {
            Stop();
            if (backgroundGroup != null) backgroundGroup.alpha = 0f;
            if (contentGroup != null) contentGroup.alpha = 0f;
        }

        #endregion

        #region Sequence

        private void Stop()
        {
            Sequence sequence = _sequence;
            _sequence = null;
            sequence?.Kill();
        }

        #endregion

        #region Unity Messages

        private void OnDisable()
        {
            _sequence?.Kill();
        }

        #endregion
    }
}
