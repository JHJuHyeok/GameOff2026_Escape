using System;
using DG.Tweening;
using UnityEngine;

namespace SH.Project.UI.Checklist
{
    [DisallowMultipleComponent]
    public sealed class ChecklistAnimation : MonoBehaviour
    {
        [SerializeField] private RectTransform _panel;
        [SerializeField] private RectTransform _header;
        [SerializeField, Min(0.01f)] private float _openDuration = 0.25f;
        [SerializeField, Min(0.01f)] private float _closeDuration = 0.2f;

        private Vector2 _expandedPosition;
        private bool _initialized;
        private Sequence _sequence;

        internal bool IsConfigured => _panel != null && _header != null;

        internal bool TryPlay(CanvasGroup bodyGroup,bool expanded,Action onCompleted,Action onInterrupted)
        {
            if (!isActiveAndEnabled || !IsConfigured || bodyGroup == null)
            {
                return false;
            }

            InitializePosition();
            StopTween();

            float duration = Mathf.Max(0.01f,expanded ? _openDuration : _closeDuration);
            float targetAlpha = expanded ? 1f : 0f;
            Vector2 targetPosition = GetTargetPosition(expanded);
            Ease moveEase = expanded ? Ease.OutCubic : Ease.InCubic;

            Sequence sequence = DOTween.Sequence();
            _sequence = sequence;

            sequence.Pause();
            sequence.SetUpdate(UpdateType.Normal, true);
            sequence.SetAutoKill(true);
            sequence.Append(_panel.DOAnchorPos(targetPosition, duration).SetEase(moveEase));
            sequence.Join(bodyGroup.DOFade(targetAlpha, duration).SetEase(Ease.Linear));

            sequence.OnComplete(() =>
            {
                _sequence = null;
                onCompleted?.Invoke();
            });

            sequence.OnKill(() =>
            {
                if (_sequence != sequence) return;
                _sequence = null;
                onInterrupted?.Invoke();
            });
            sequence.Play();
            return true;
        }

        internal void SetExpandedImmediately(CanvasGroup bodyGroup,bool expanded)
        {
            StopTween();
            if (bodyGroup != null) bodyGroup.alpha = expanded ? 1f : 0f;
            if (!IsConfigured) return;
            InitializePosition();
            _panel.anchoredPosition = GetTargetPosition(expanded);
        }

        private void OnDisable()
        {
            _sequence?.Kill();
        }

        private void InitializePosition()
        {
            if (_initialized) return;
            _expandedPosition = _panel.anchoredPosition;
            _initialized = true;
        }

        private Vector2 GetTargetPosition(bool expanded)
        {
            if (expanded) return _expandedPosition;
            _panel.ForceUpdateRectTransforms();
            float moveDistance = Mathf.Max(0f,_panel.rect.height - _header.rect.height);
            return _expandedPosition - Vector2.up * moveDistance;
        }

        private void StopTween()
        {
            Sequence sequence = _sequence;
            _sequence = null;
            sequence?.Kill();
        }
    }
}