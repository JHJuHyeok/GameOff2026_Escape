using System;
using DG.Tweening;
using UnityEngine;

namespace SH.Project.UI.CommonPopup
{
    [DisallowMultipleComponent]
    public sealed class CommonPopupAnimation : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float _openDuration = 0.2f;
        [SerializeField, Min(0.01f)] private float _closeDuration = 0.15f;
        [SerializeField, Range(0f, 0.1f)] private float _openPunchScale = 0.025f;

        private Transform _content;
        private Vector3 _defaultScale;
        private Sequence _sequence;

        #region Animation API

        //팝업 설정 초기화
        internal void Initialize(Transform content)
        {
            _content = content;
            _defaultScale = content.localScale;
        }

        //팝업의 등장/퇴장 연출을 재생
        internal void Play(CanvasGroup group, bool show, Action onCompleted, Action onInterrupted)
        {
            Stop();

            float duration = Mathf.Max(0.01f, show ? _openDuration : _closeDuration);
            Sequence sequence = DOTween.Sequence();
            _sequence = sequence;
            sequence.Pause().SetUpdate(UpdateType.Normal, true).SetAutoKill(true);

            sequence.Append(group.DOFade(show ? 1f : 0f, duration).SetEase(Ease.Linear));

            if (show)
            {
                _content.localScale = _defaultScale;
                if (_openPunchScale > 0f)
                {
                    Vector3 strength = new Vector3(_defaultScale.x, _defaultScale.y, 0f) * _openPunchScale;
                    sequence.Join(_content.DOPunchScale(strength, duration, 3, 0.3f));
                }
            }
            else
            {
                sequence.Join(_content.DOScale(_defaultScale, duration).SetEase(Ease.OutQuad));
            }

            sequence
                .OnComplete(() =>
                {
                    _sequence = null;
                    _content.localScale = _defaultScale;
                    onCompleted();
                })
                .OnKill(() =>
                {
                    if (_sequence != sequence) return;
                    _sequence = null;
                    onInterrupted();
                });

            sequence.Play();
        }

        //팝업을 강제로 닫고, 콘텐츠를 기준 크기로 되돌립니다
        internal void ResetToHidden(CanvasGroup group)
        {
            Stop();
            group.alpha = 0f;
            _content.localScale = _defaultScale;
        }

        #endregion

        #region Unity Messages

        private void OnDisable()
        {
            _sequence?.Kill();
        }

        #endregion

        #region Tween

        private void Stop()
        {
            Sequence sequence = _sequence;
            _sequence = null;
            sequence?.Kill();
        }

        #endregion
    }
}
