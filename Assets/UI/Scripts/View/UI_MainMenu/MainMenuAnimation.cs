using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace SH.Project.UI.MainMenu
{
    [DisallowMultipleComponent]
    public sealed class MainMenuAnimation : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _titleGroup;
        [SerializeField] private CanvasGroup _startButtonGroup;
        [SerializeField] private CanvasGroup _settingsButtonGroup;
        [SerializeField] private CanvasGroup _quitButtonGroup;

        [FormerlySerializedAs("_showDuration")]
        [SerializeField, Min(0.01f)] private float _backgroundFadeDuration = 0.65f;
        [SerializeField, Min(0f)] private float _titleDelay = 0.15f;
        [SerializeField, Min(0.01f)] private float _titleFadeDuration = 0.35f;
        [SerializeField, Min(0f)] private float _buttonDelay = 0.15f;
        [SerializeField, Min(0.01f)] private float _buttonFadeDuration = 0.2f;
        [SerializeField, Min(0.01f)] private float _hideDuration = 0.35f;

        private Sequence _sequence;

        #region Animation API

        //배경, 타이틀, 버튼 순차 등장/퇴장 연출을 트라이 함
        internal bool TryPlay(CanvasGroup backgroundGroup, CanvasGroup contentGroup, bool show,Action onCompleted, Action onInterrupted)
        {
            if (!isActiveAndEnabled) return false;
            if (show && (_titleGroup == null || _startButtonGroup == null || _settingsButtonGroup == null || _quitButtonGroup == null)) return false;

            Stop();

            Sequence sequence = DOTween.Sequence();
            _sequence = sequence;

            sequence.Pause();
            sequence.SetAutoKill(true);
            sequence.SetUpdate(UpdateType.Normal, true);
            if (show) AppendShow(sequence, backgroundGroup, contentGroup);
            else AppendHide(sequence, backgroundGroup, contentGroup);

            sequence.OnComplete(() =>
            {
                _sequence = null;
                onCompleted();
            });
            sequence.OnKill(() =>
            {
                // 정상 완료 또는 전환 요청에 따른 중단은 이미 호출한 View에서 처리
                if (_sequence != sequence) return;

                _sequence = null;
                onInterrupted();
            });
            sequence.Play();

            return true;
        }

        //연출 스퀸스를 중단하고, 상태를 초기화 함
        internal void Stop()
        {
            Sequence sequence = _sequence;
            _sequence = null;
            sequence?.Kill();
        }

        #endregion

        #region Sequences

        private void AppendShow(Sequence sequence, CanvasGroup backgroundGroup, CanvasGroup contentGroup)
        {
            // 퇴장 중 다시 등장할 때 부모 알파를 자식에게 반영해 현재 화면 밝기를 유지
            float contentAlpha = contentGroup.alpha;
            _titleGroup.alpha *= contentAlpha;
            _startButtonGroup.alpha *= contentAlpha;
            _settingsButtonGroup.alpha *= contentAlpha;
            _quitButtonGroup.alpha *= contentAlpha;
            contentGroup.alpha = 1f;

            float backgroundDuration = Mathf.Max(0.01f, _backgroundFadeDuration);
            float titleDuration = Mathf.Max(0.01f, _titleFadeDuration);
            float buttonDuration = Mathf.Max(0.01f, _buttonFadeDuration);

            sequence.Append(backgroundGroup.DOFade(1f, backgroundDuration).SetEase(Ease.Linear));
            sequence.AppendInterval(Mathf.Max(0f, _titleDelay));
            sequence.Append(_titleGroup.DOFade(1f, titleDuration).SetEase(Ease.Linear));
            sequence.AppendInterval(Mathf.Max(0f, _buttonDelay));
            sequence.Append(_startButtonGroup.DOFade(1f, buttonDuration).SetEase(Ease.Linear));
            sequence.Append(_settingsButtonGroup.DOFade(1f, buttonDuration).SetEase(Ease.Linear));
            sequence.Append(_quitButtonGroup.DOFade(1f, buttonDuration).SetEase(Ease.Linear));
        }

        private void AppendHide(Sequence sequence, CanvasGroup backgroundGroup, CanvasGroup contentGroup)
        {
            float duration = Mathf.Max(0.01f, _hideDuration);
            sequence.Append(backgroundGroup.DOFade(0f, duration).SetEase(Ease.Linear));
            sequence.Join(contentGroup.DOFade(0f, duration).SetEase(Ease.Linear));
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
