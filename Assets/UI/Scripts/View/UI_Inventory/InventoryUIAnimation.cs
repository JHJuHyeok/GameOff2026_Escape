using System;
using DG.Tweening;
using UnityEngine;

namespace SH.Project.UI.Inventory
{
    [DisallowMultipleComponent]
    public sealed class InventoryUIAnimation : MonoBehaviour
    {
        [SerializeField] private RectTransform _frame;
        [SerializeField, Min(0.01f)] private float _openDuration = 0.18f;
        [SerializeField, Min(0.01f)] private float _closeDuration = 0.12f;
        [SerializeField, Min(0f)] private float _moveOffset = 12f;

        private Vector2 _visiblePosition;
        private bool _initialized;
        private Sequence _sequence;

        //현재 위치와 알파에서 등장 또는 퇴장 연출을 시작
        internal bool TryPlay(CanvasGroup group,bool visible,Action onCompleted,Action onInterrupted)
        {
            if (!isActiveAndEnabled || group == null || _frame == null) return false;

            InitializePosition();
            StopTween();

            float alpha = 0f;
            float duration = _closeDuration;
            Vector2 position = _visiblePosition - Vector2.up * _moveOffset;
            Ease moveEase = Ease.InQuad;

            if (visible)
            {
                alpha = 1f;
                duration = _openDuration;
                position = _visiblePosition;
                moveEase = Ease.OutQuad;
            }

            duration = Mathf.Max(0.01f, duration);

            Sequence sequence = DOTween.Sequence();
            _sequence = sequence;

            sequence.Pause();
            sequence.SetUpdate(UpdateType.Normal, true);
            sequence.SetAutoKill(true);
            sequence.Append(group.DOFade(alpha, duration).SetEase(Ease.Linear));
            sequence.Join(_frame.DOAnchorPos(position, duration).SetEase(moveEase));

            sequence.OnComplete(() =>
            {
                _sequence = null;
                onCompleted?.Invoke();
            });

            sequence.OnKill(() =>
            {
                if (_sequence != sequence)
                    return;

                _sequence = null;
                onInterrupted?.Invoke();
            });

            sequence.Play();
            return true;
        }

        //진행 중인 연출을 중단하고 표시 상태를 즉시 적용
        internal void SetVisibleImmediately(CanvasGroup group, bool visible)
        {
            StopTween();

            if (group != null)
            {
                if (visible) group.alpha = 1f; else group.alpha = 0f;
            }
            if (_frame == null) return;

            InitializePosition();

            if (visible) _frame.anchoredPosition = _visiblePosition;
            else _frame.anchoredPosition = _visiblePosition - Vector2.up * _moveOffset;
        }

        private void OnDisable()
        {
            _sequence?.Kill();
        }

        private void InitializePosition()
        {
            if (_initialized) return;

            _visiblePosition = _frame.anchoredPosition;
            _initialized = true;
        }

        private void StopTween()
        {
            Sequence sequence = _sequence;
            _sequence = null;
            sequence?.Kill();
        }
    }
}