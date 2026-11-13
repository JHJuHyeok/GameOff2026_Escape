using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace SH.Project.UI.Result
{
    [DisallowMultipleComponent]
    public sealed class ResultMessageAnimation : MonoBehaviour
    {
        [SerializeField] private RectTransform _title;
        [SerializeField] private CanvasGroup _titleGroup;
        [SerializeField] private CanvasGroup _followupGroup;
        [SerializeField] private Vector2 _titleStartPosition = Vector2.zero;
        [SerializeField] private Vector2 _titleEndPosition = new (0f, 88f);
        [SerializeField, Min(0.01f)] private float _titleFadeDuration = 0.2f;
        [SerializeField, Min(0f)] private float _titleHoldDuration = 0.2f;
        [SerializeField, Min(0.01f)] private float _titleMoveDuration = 0.25f;
        [SerializeField, Min(0.01f)] private float _followupFadeDuration = 0.2f;

        private Sequence _sequence;

        #region Properties

        internal bool IsConfigured => _title != null && _titleGroup != null && _followupGroup != null;

        #endregion

        #region Animation API

        // 제목 등장과 위쪽 이동을 마친 뒤 코멘트와 버튼을 함께 표시합니다.
        internal IEnumerator Play()
        {
            ResetPresentation();

            _sequence = DOTween.Sequence().Pause().SetUpdate(UpdateType.Normal, true).SetAutoKill(false).SetRecyclable(false);
            _sequence.Append(_titleGroup.DOFade(1f, Mathf.Max(0.01f, _titleFadeDuration)));
            _sequence.AppendInterval(Mathf.Max(0f, _titleHoldDuration));
            _sequence.Append(_title.DOAnchorPos(_titleEndPosition, Mathf.Max(0.01f, _titleMoveDuration)).SetEase(Ease.OutCubic));
            _sequence.Append(_followupGroup.DOFade(1f, Mathf.Max(0.01f, _followupFadeDuration)));
            _sequence.Play();

            yield return _sequence.WaitForCompletion();
            StopSequence();
        }

        // 코멘트와 버튼 그룹의 포인터 입력 허용 여부를 설정합니다.
        internal void SetFollowupInput(bool enabled)
        {
            _followupGroup.interactable = enabled;
            _followupGroup.blocksRaycasts = enabled;
        }

        // 제목을 중앙으로 돌리고 제목과 후속 요소를 숨깁니다.
        internal void ResetPresentation()
        {
            StopSequence();
            _title.anchoredPosition = _titleStartPosition;
            _titleGroup.alpha = 0f;
            _followupGroup.alpha = 0f;
            SetFollowupInput(false);
        }

        #endregion

        #region Unity Messages

        private void OnDestroy()
        {
            StopSequence();
        }

        #endregion

        #region Tween

        private void StopSequence()
        {
            _sequence?.Kill();
            _sequence = null;
        }

        #endregion
    }
}



