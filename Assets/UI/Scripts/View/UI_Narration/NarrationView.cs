using System;
using TMPro;
using UnityEngine;

namespace SH.Project.UI.Narration
{
    public enum NarrationPlacement
    {
        Bottom,
        Center
    }

    public enum NarrationState
    {
        Hidden,
        Showing,
        Visible,
        Hiding
    }

    [DisallowMultipleComponent]
    public sealed class NarrationView : MonoBehaviour
    {
        [SerializeField] private RectTransform _textGroup;
        [SerializeField] private TMP_Text _narrationText;
        [SerializeField] private NarrationAnimation _animation;

        [SerializeField] private NarrationPlacement _defaultPlacement = NarrationPlacement.Bottom;
        [SerializeField] private Vector2 _bottomPosition = new (0f, 172f);
        [SerializeField] private Vector2 _centerPosition = Vector2.zero;
        [SerializeField] private TextAlignmentOptions _textAlignment = TextAlignmentOptions.Center;

        private string _pendingText;

        #region Events

        public event Action OnNarrationStarted; //나레이션 등장 연출을 시작했을 때 발생하는 이벤트
        public event Action OnTextDisplayed; //문구가 완전히 표시됐을 때 발생하는 이벤트
        public event Action OnSkipRequested; //나레이션을 건너뛰기 위해 짧은 퇴장 연출을 시작했을 때 발생하는 이벤트
        public event Action OnNarrationCompleted; //나레이션이 완전히 숨겨졌을 때 발생하는 이벤트
        public event Action OnNarrationCancelled; //나레이션을 중단하고 숨겼을 때 발생하는 이벤트

        #endregion

        #region Properties

        public NarrationState State { get; private set; } = NarrationState.Hidden; //현재 나레이션의 상태

        #endregion

        #region Public API

        /// 현재 나레이션을 숨기고 기본 위치에 새 나레이션을 표시
        public bool Show()
        {
            return Show(_defaultPlacement);
        }

        //현재 나레이션을 숨기고 지정한 위치에 새 나레이션을 표시
        public bool Show(NarrationPlacement placement, bool showBackground = false)
        {
            if (!isActiveAndEnabled || State != NarrationState.Hidden || _animation == null || _textGroup == null || _narrationText == null) return false;

            ApplyPlacement(placement);

            if (_pendingText != null) _narrationText.text = _pendingText;
            if (!_animation.TryShow(_narrationText, showBackground, HandleShown, HandleInterrupted)) return false;

            _pendingText = null;
            State = NarrationState.Showing;
            OnNarrationStarted?.Invoke();
            return true;
        }

        //나레이션 문구를 교체
        public void SetText(string text)
        {
            if (_narrationText == null) return;

            string nextText = text ?? string.Empty;

            if (State == NarrationState.Hiding)
            {
                _pendingText = nextText;
                return;
            }

            _pendingText = null;
            _narrationText.text = nextText;

            if ((State == NarrationState.Showing || State == NarrationState.Visible) &&
                _animation != null && _animation.TryRestartTyping(_narrationText, HandleShown, HandleInterrupted)) State = NarrationState.Showing;
        }

        //현재 나레이션을 건너뛰고 짧은 퇴장
        public bool Skip()
        {
            if (!TryHide(true)) return false;

            OnSkipRequested?.Invoke();
            return true;
        }

        //현재 나레이션을 중단 합니다
        public bool Hide()
        {
            return TryHide(false);
        }

        //현재 나레이션을 중단하고 숨깁니다
        public bool Cancel()
        {
            if (State == NarrationState.Hidden) return false;

            State = NarrationState.Hidden;

            if (_animation != null) _animation.ResetToHidden();

            OnNarrationCancelled?.Invoke();
            return true;
        }

        #endregion

        #region Unity Messages

        private void Awake()
        {
            if (State == NarrationState.Hidden && _animation != null) _animation.ResetToHidden();
        }

        private void OnDisable()
        {
            Cancel();
        }

        #endregion

        #region Layout and Transitions

        private void ApplyPlacement(NarrationPlacement placement)
        {
            bool center = placement == NarrationPlacement.Center;
            float anchorY = center ? 0.5f : 0f;

            Vector2 min = _textGroup.anchorMin;
            Vector2 max = _textGroup.anchorMax;
            min.y = anchorY;
            max.y = anchorY;
            _textGroup.anchorMin = min;
            _textGroup.anchorMax = max;
            _textGroup.anchoredPosition = center ? _centerPosition : _bottomPosition;
            _narrationText.alignment = _textAlignment;
        }

        private bool TryHide(bool skip)
        {
            if (!isActiveAndEnabled || _animation == null || (State != NarrationState.Showing && State != NarrationState.Visible)) return false;
            if (!_animation.TryHide(skip, HandleHidden, HandleInterrupted)) return false;

            State = NarrationState.Hiding;
            return true;
        }

        private void HandleShown()
        {
            State = NarrationState.Visible;
            OnTextDisplayed?.Invoke();
        }

        private void HandleHidden()
        {
            State = NarrationState.Hidden;
            OnNarrationCompleted?.Invoke();
        }

        private void HandleInterrupted()
        {
            Cancel();
        }

        #endregion

#if UNITY_EDITOR
        #region Context Menu

        [ContextMenu("Narration/Show Bottom")]
        private void ShowBottomForTest()
        {
            if (Application.isPlaying) Show(NarrationPlacement.Bottom);
        }

        [ContextMenu("Narration/Show Center")]
        private void ShowCenterForTest()
        {
            if (Application.isPlaying) Show(NarrationPlacement.Center);
        }

        [ContextMenu("Narration/Show Center With Background")]
        private void ShowCenterWithBackgroundForTest()
        {
            if (Application.isPlaying) Show(NarrationPlacement.Center, showBackground: true);
        }

        [ContextMenu("Narration/Skip")]
        private void SkipForTest()
        {
            if (Application.isPlaying) Skip();
        }

        [ContextMenu("Narration/Hide")]
        private void HideForTest()
        {
            if (Application.isPlaying) Hide();
        }

        [ContextMenu("Narration/Cancel")]
        private void CancelForTest()
        {
            if (Application.isPlaying) Cancel();
        }

        #endregion
#endif
    }
}
