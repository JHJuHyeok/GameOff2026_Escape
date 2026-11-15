using System;
using DG.Tweening;
using UnityEngine;

namespace SH.Project.UI.Intro
{
    [DisallowMultipleComponent]
    public sealed class IntroAnimation : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _logoCanvasGroup;

        [SerializeField, Min(0.01f)] private float _fadeInDuration = 0.6f;
        [SerializeField, Min(0f)] private float _holdDuration = 1f;
        [SerializeField, Min(0.01f)] private float _fadeOutDuration = 0.6f;
        [SerializeField, Min(0f)] private float _postFadeHoldDuration = 0.4f;

        private Sequence _sequence;

        #region Animation API

        /// <summary>로고의 페이드 인, 표시 유지, 페이드 아웃, 검은 화면 유지를 순서대로 재생합니다.</summary>
        /// <param name="onCompleted">검은 화면 유지까지 끝났을 때 호출할 콜백입니다.</param>
        /// <param name="onInterrupted">재생 중 시퀀스가 외부에서 중단되거나 컴포넌트가 비활성화됐을 때 호출할 콜백입니다.</param>
        /// <returns>시퀀스를 시작했으면 true, 시작할 수 없으면 false입니다.</returns>
        internal bool TryPlay(Action onCompleted, Action onInterrupted)
        {
            if (!isActiveAndEnabled || _logoCanvasGroup == null || _sequence != null) return false;

            ResetLogo();

            float fadeInDuration = Mathf.Max(0.01f, _fadeInDuration);
            float holdDuration = Mathf.Max(0f, _holdDuration);
            float fadeOutDuration = Mathf.Max(0.01f, _fadeOutDuration);

            Sequence sequence = DOTween.Sequence();
            _sequence = sequence;

            sequence.Pause();
            sequence.SetAutoKill(true);
            sequence.SetUpdate(UpdateType.Normal, true);
            sequence.Append(_logoCanvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.Linear));
            sequence.AppendInterval(holdDuration);
            sequence.Append(_logoCanvasGroup.DOFade(0f, fadeOutDuration).SetEase(Ease.Linear));
            sequence.AppendInterval(Mathf.Max(0f, _postFadeHoldDuration));
            sequence.OnComplete(() => CompleteSequence(onCompleted));
            sequence.OnKill(() => HandleSequenceKilled(sequence, onInterrupted));
            sequence.Play();
            return true;
        }

        /// <summary>시퀀스를 중단하고 로고의 알파를 0으로 초기화합니다. 취소 이벤트는 호출한 View에서 발행합니다.</summary>
        internal void Stop()
        {
            Sequence sequence = _sequence;

            // 취소 처리는 호출한 View가 담당하므로 이 시퀀스의 중단 콜백을 건너뜁니다.
            _sequence = null;
            sequence?.Kill();
            ResetLogo();
        }

        #endregion

        #region Unity Messages

        private void OnDisable()
        {
            _sequence?.Kill();
        }

        #endregion

        #region Sequence Callbacks

        private void CompleteSequence(Action onCompleted)
        {
            _sequence = null;
            onCompleted();
        }

        private void HandleSequenceKilled(Sequence sequence, Action onInterrupted)
        {
            // 정상 완료 후에도 OnKill이 실행되므로 현재 시퀀스가 중단된 경우에만 처리합니다.
            if (_sequence != sequence) return;

            _sequence = null;
            ResetLogo();
            onInterrupted();
        }

        private void ResetLogo()
        {
            if (_logoCanvasGroup != null) _logoCanvasGroup.alpha = 0f;
        }

        #endregion
    }
}
