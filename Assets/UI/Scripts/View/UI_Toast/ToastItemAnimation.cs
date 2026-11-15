using System;
using DG.Tweening;
using UnityEngine;

namespace SH.Project.UI.Toast
{
    [DisallowMultipleComponent]
    public sealed class ToastItemAnimation : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _visualGroup;
        [SerializeField, Min(0.01f)] private float _showDuration = 0.18f;
        [SerializeField, Min(0.01f)] private float _hideDuration = 0.16f;
        [SerializeField, Min(0f)] private float _slideDistance = 12f;

        private RectTransform _visual;
        private Vector2 _restPosition;
        private Sequence _sequence;

        #region Properties

        internal bool CanPlay => isActiveAndEnabled && _visual != null; //현재 연출이 진행중인지 여부

        #endregion

        #region Animation API

        //항목이 살짝 내려오면서 등장하게 합니다
        internal void Show(Action onComplete, Action onInterrupted)
        {
            _visual.anchoredPosition = _restPosition + Vector2.up * _slideDistance;
            _visualGroup.alpha = 0f;
            Play(true, onComplete, onInterrupted);
        }

        //항목이 살짝 올라가면서 사라지게 합니다
        internal void Hide(Action onComplete)
        {
            Play(false, onComplete, onComplete);
        }

        //현재 진행중인 연출을 중단 (Show, Hide 호출 시 자동으로 호출됨)
        internal void Stop()
        {
            Sequence sequence = _sequence;
            _sequence = null;
            sequence?.Kill();
        }

        #endregion

        #region Unity Messages

        private void Awake()
        {
            if (_visualGroup == null || !(_visualGroup.transform is RectTransform visual))
            {
                Debug.LogError("ToastItemAnimation의 Visual Group을 연결해 주세요.", this);
                return;
            }

            _visual = visual;
            _restPosition = visual.anchoredPosition;
            _visualGroup.alpha = 0f;
        }

        private void OnDisable()
        {
            _sequence?.Kill();
        }

        #endregion

        #region Tween

        private void Play(bool show, Action onComplete, Action onInterrupted)
        {
            Stop();

            float duration = Mathf.Max(0.01f, show ? _showDuration : _hideDuration);
            Vector2 target = show ? _restPosition : _restPosition + Vector2.up * _slideDistance;
            Sequence sequence = DOTween.Sequence();
            _sequence = sequence;
            sequence.Pause().SetUpdate(UpdateType.Normal, true).SetAutoKill(true);
            sequence.Append(_visual.DOAnchorPos(target, duration).SetEase(show ? Ease.OutCubic : Ease.InQuad));
            sequence.Join(_visualGroup.DOFade(show ? 1f : 0f, duration).SetEase(Ease.Linear));
            sequence.OnComplete(() =>
            {
                _sequence = null;
                onComplete();
            });
            sequence.OnKill(() =>
            {
                if (_sequence != sequence) return;
                _sequence = null;
                onInterrupted();
            });
            sequence.Play();
        }

        #endregion
    }
}