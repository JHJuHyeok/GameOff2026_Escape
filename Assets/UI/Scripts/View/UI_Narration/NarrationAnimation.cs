using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace SH.Project.UI.Narration
{
    [DisallowMultipleComponent]
    public sealed class NarrationAnimation : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _textGroup;
        [SerializeField] private CanvasGroup _backgroundGroup;

        [SerializeField, Min(0.01f)] private float _fadeInDuration = 0.35f;
        [SerializeField, Min(0.01f)] private float _fadeOutDuration = 0.3f;
        [SerializeField, Min(0.01f)] private float _skipFadeOutDuration = 0.12f;
        [SerializeField, Min(1f)] private float _charactersPerSecond = 30f;

        private Sequence _sequence;
        private TMP_Text _typingText;

        #region Properties

        //나레이션 자체의 검은 배경이 존재하고 활성화되어 있는지 여부
        private bool HasBackground => _backgroundGroup != null && _backgroundGroup != _textGroup &&
            _backgroundGroup.transform.IsChildOf(transform) && _backgroundGroup.gameObject != gameObject;

        #endregion

        #region Animation API

        /// 현재 나레이션을 숨기고 지정한 위치에 새 나레이션을 표시
        internal bool TryShow(TMP_Text text, bool showBackground, Action onCompleted, Action onInterrupted)
        {
            if (!isActiveAndEnabled || _textGroup == null || text == null || (showBackground && !HasBackground)) return false;

            ResetToHidden();

            if (showBackground) _backgroundGroup.gameObject.SetActive(true);

            PlayTyping(text, onCompleted, onInterrupted);
            return true;
        }

        //텍스트 연출을 중단하고 새 텍스트를 타이핑합니다
        internal bool TryRestartTyping(TMP_Text text, Action onCompleted, Action onInterrupted)
        {
            if (!isActiveAndEnabled || _textGroup == null || text == null) return false;

            StopTween();
            PlayTyping(text, onCompleted, onInterrupted);
            return true;
        }

        //현재 나레이션을 숨기고 지정한 위치에 새 나레이션을 표시
        internal bool TryHide(bool skip, Action onCompleted, Action onInterrupted)
        {
            if (!isActiveAndEnabled || _textGroup == null) return false;

            StopTween();
            float duration = skip ? _skipFadeOutDuration : _fadeOutDuration;
            Sequence sequence = CreateSequence();
            AppendFade(sequence, 0f, duration);
            PlaySequence(sequence, () =>
            {
                ResetToHidden();
                onCompleted();
            }, onInterrupted);
            return true;
        }

       //트윈을 종료하고 텍스트를 숨깁니다
        internal void ResetToHidden()
        {
            StopTween();

            if (_textGroup != null) _textGroup.alpha = 0f;
            if (_typingText != null) _typingText.maxVisibleCharacters = int.MaxValue;

            _typingText = null;

            if (HasBackground)
            {
                _backgroundGroup.alpha = 0f;
                _backgroundGroup.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Unity Messages

        private void OnDisable()
        {
            _sequence?.Kill();
        }

        #endregion

        #region Tween

        private void PlayTyping(TMP_Text text, Action onCompleted, Action onInterrupted)
        {
            _typingText = text;
            text.maxVisibleCharacters = int.MaxValue;
            text.ForceMeshUpdate();
            int characterCount = text.textInfo.characterCount;
            text.maxVisibleCharacters = 0;

            Sequence sequence = CreateSequence();
            float remainingAlpha = 1f - _textGroup.alpha;

            if (remainingAlpha > 0f) AppendFade(sequence, 1f, _fadeInDuration * remainingAlpha);
            if (characterCount > 0)
            {
                float duration = characterCount / Mathf.Max(1f, _charactersPerSecond);
                Tween typing = DOTween.To(() => 0f,value => text.maxVisibleCharacters = Mathf.FloorToInt(value), characterCount, duration);
                sequence.Insert(0f, typing.SetEase(Ease.Linear));
            }
            else if (remainingAlpha <= 0f)
            {
                // 빈 문구도 View가 표시 상태를 갱신한 뒤 완료 콜백을 받도록 합니다
                sequence.AppendInterval(0.01f);
            }

            PlaySequence(sequence, () =>
            {
                text.maxVisibleCharacters = int.MaxValue;
                onCompleted();
            }, onInterrupted);
        }

        private Sequence CreateSequence()
        {
            Sequence sequence = DOTween.Sequence();
            _sequence = sequence;
            sequence.Pause();
            sequence.SetAutoKill(true);
            sequence.SetUpdate(UpdateType.Normal, true);
            return sequence;
        }

        private void AppendFade(Sequence sequence, float targetAlpha, float duration)
        {
            duration = Mathf.Max(0.01f, duration);
            sequence.Append(_textGroup.DOFade(targetAlpha, duration).SetEase(Ease.Linear));

            if (HasBackground && _backgroundGroup.gameObject.activeSelf) sequence.Join(_backgroundGroup.DOFade(targetAlpha, duration).SetEase(Ease.Linear));
        }

        private void PlaySequence(Sequence sequence, Action onCompleted, Action onInterrupted)
        {
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
