using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.Toast
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class ToastItemView : MonoBehaviour
    {
        internal enum RemovalReason
        {
            Removed,
            Expired,
            Dismissed
        }

        private enum State
        {
            Idle,
            Showing,
            Visible,
            Hiding,
            Finished
        }

        [SerializeField] private ToastItemAnimation _animation;
        [SerializeField] private RectTransform _visual;
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField, Min(1f)] private float _minWidth = 96f;
        [SerializeField, Min(1f)] private float _maxWidth = 1000f;

        private State _state;
        private Coroutine _expiration;
        private RemovalReason _removalReason;
        private bool _configured;

        #region Events

        internal event Action<ToastItemView> Shown; ///항목 표시 완료 시 발생
        internal event Action<ToastItemView, RemovalReason> Finished; //항목 표시 완료 시 발생, 제거 사유와 함께 전달

        #endregion

        #region Properties

        public int Id { get; private set; } ///현재 항목의 ID입니다
        public string Message => _messageText.text; ///현재 항목의 문구입니다
        public Color BackgroundColor => _background.color; ///현재 항목의 문구 색상입니다
        public Color TextColor => _messageText.color; ///항목이 표시 중인지 나타냅니다
        internal bool IsHiding => _state == State.Hiding; //항목이 퇴장 연출 중인지 나타냅니다

        #endregion

        #region Item API

        //항목을 표시할 수 있는 상태인지 나타냅니다
        internal bool TryShow(int id, string message, Color backgroundColor, Color textColor, float displayDuration)
        {
            if (!_configured || !isActiveAndEnabled || !_animation.CanPlay || _state != State.Idle) return false;

            Id = id;
            ApplyContent(message, backgroundColor, textColor);
            _state = State.Showing;
            _animation.Show(() => FinishShowing(displayDuration), () => Finish(RemovalReason.Removed));
            return true;
        }

        //항목의 내용과 색상을 갱신, 항목이 표시 중이 아니면 false를 반환합니다
        internal bool TryUpdate(string message, Color? backgroundColor, Color? textColor)
        {
            if (_state != State.Showing && _state != State.Visible) return false;

            ApplyContent(message, backgroundColor, textColor);
            return true;
        }

        //표시 중인 항목을 퇴장 연출과 함께 제거, 제거 사유는 인자로 전달됩니다
        internal bool TryHide(RemovalReason reason)
        {
            if (_state != State.Showing && _state != State.Visible) return false;

            _state = State.Hiding;
            _removalReason = reason;
            StopExpiration();

            if (_animation.CanPlay) _animation.Hide(() => Finish(reason));
            else Finish(reason);

            return true;
        }

        #endregion

        #region Unity Messages

        private void Awake()
        {
            _configured = _animation != null && _visual != null && _background != null && _messageText != null;
            if (!_configured) Debug.LogError("ToastItemView의 Animation, Visual, Background, Message Text를 연결해 주세요.", this);
        }

        private void OnDisable()
        {
            if (_state != State.Showing && _state != State.Visible && _state != State.Hiding) return;

            StopExpiration();
            _animation.Stop();
            Finish(_state == State.Hiding ? _removalReason : RemovalReason.Removed);
        }

        private void OnRectTransformDimensionsChange()
        {
            if (_configured && (_state == State.Showing || _state == State.Visible)) ResizeToMessage();
        }

        #endregion

        #region Content

        private void ApplyContent(string message, Color? backgroundColor, Color? textColor)
        {
            if (backgroundColor.HasValue) _background.color = backgroundColor.Value;
            if (textColor.HasValue) _messageText.color = textColor.Value;
            if (message == null) return;

            _messageText.text = message;
            ResizeToMessage();
        }

        private void ResizeToMessage()
        {
            RectTransform textRect = _messageText.rectTransform;
            float padding = textRect.offsetMin.x - textRect.offsetMax.x;
            float preferredWidth = _messageText.GetPreferredValues(Message, Mathf.Infinity, Mathf.Infinity).x;
            float rowWidth = ((RectTransform)transform).rect.width;
            float maxWidth = rowWidth > 0f ? Mathf.Min(_maxWidth, rowWidth) : _maxWidth;
            float width = Mathf.Clamp(Mathf.Ceil(preferredWidth + padding), Mathf.Min(_minWidth, maxWidth), maxWidth);
            if (!Mathf.Approximately(_visual.rect.width, width)) _visual.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        #endregion

        #region Lifetime

        private void FinishShowing(float displayDuration)
        {
            _state = State.Visible;
            _expiration = StartCoroutine(ExpireAfter(displayDuration));
            Shown?.Invoke(this);
        }

        private IEnumerator ExpireAfter(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            _expiration = null;
            TryHide(RemovalReason.Expired);
        }

        private void StopExpiration()
        {
            if (_expiration == null) return;
            StopCoroutine(_expiration);
            _expiration = null;
        }

        private void Finish(RemovalReason reason)
        {
            _state = State.Finished;
            Finished?.Invoke(this, reason);
        }

        #endregion
    }
}



