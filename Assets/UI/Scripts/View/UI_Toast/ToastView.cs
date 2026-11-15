using System;
using System.Collections.Generic;
using UnityEngine;

namespace SH.Project.UI.Toast
{
    [DisallowMultipleComponent]
    public sealed class ToastView : MonoBehaviour
    {
        private const int MaxVisibleCount = 5;

        [SerializeField] private RectTransform _toastGroup;
        [SerializeField] private ToastItemView _itemPrefab;
        [SerializeField] private ToastAnimation _animation;
        [SerializeField, Range(1, MaxVisibleCount)] private int _maxToastCount = MaxVisibleCount;
        [SerializeField, Min(0f)] private float _displayDuration = 3f;
        [SerializeField] private Color _defaultBackgroundColor = new Color32(36, 36, 36, 255);
        [SerializeField] private Color _defaultTextColor = Color.white;

        private readonly List<Request> _pending = new ();
        private readonly List<ToastItemView> _visible = new (MaxVisibleCount);
        private ToastItemView _opening;
        private int _lastId;
        private bool _configured;

        private sealed class Request
        {
            public int Id { get; set; }
            public string Message { get; set; }
            public Color BackgroundColor { get; set; }
            public Color TextColor { get; set; }
        }

        #region Events

        public event Action<int> OnToastShown; //토스트의 등장 연출이 끝났을 때
        public event Action<int> OnToastRemoved; //표시 항목이 목록에서 제거되거나 대기 요청이 취소됐을 때
        public event Action<int> OnToastExpired; //	표시 시간이 지나 퇴장까지 끝났을 때
        public event Action<int> OnToastDismissed; //DismissToast(id)로 닫힌 항목의 퇴장이 끝났거나, 대기 요청이 취소됐을 때

        #endregion

        #region Properties

        public bool IsVisible { get; private set; } = true; //토스트 영역 전체의 표시 상태, false이면 대기열과 화면에 표시 중인 항목을 모두 제거
        public int VisibleCount => _visible.Count; //화면에 표시 중인 항목 수
        public int PendingCount => _pending.Count; //대기열에 남아 있는 항목 수
        public int MaxToastCount => _maxToastCount; //동시표시 한도, 1~5 사이로 제한됩니다
        public float DisplayDuration => _displayDuration; //새 항목에 적용할 기본 표시 시간, 0이면 즉시 퇴장
        public Color DefaultBackgroundColor => _defaultBackgroundColor; //배경색을 생략한 새 요청에 적용할 색
        public Color DefaultTextColor => _defaultTextColor; //글자색을 생략한 새 요청에 적용할 색

        #endregion

        #region Public API

        //문구와 색상을 지정하여 새 항목을 대기열에 추가합니다, 반환값은 항목 ID이며 0이면 실패입니다
        //추후 SO기반 데이터로 할 경우, 인자값의 구조체를 만들어서 전달하는 방식으로 변경할 수 있습니다
        public int ShowToast(string message, Color? backgroundColor = null, Color? textColor = null)
        {
            if (!isActiveAndEnabled || !_configured || string.IsNullOrWhiteSpace(message)) return 0;

            int id = ++_lastId;
            _pending.Add(new Request
            {
                Id = id,
                Message = message,
                BackgroundColor = backgroundColor ?? _defaultBackgroundColor,
                TextColor = textColor ?? _defaultTextColor
            });
            return id;
        }

        //대기 중인 항목과 화면에 표시 중인 항목을 모두 제거합니다, 제거 사유는 Removed로 전달됩니다
        public bool UpdateToast(int id, string message = null, Color? backgroundColor = null, Color? textColor = null)
        {
            if (message != null && string.IsNullOrWhiteSpace(message)) return false;

            ToastItemView item = _visible.Find(candidate => candidate.Id == id);
            if (item != null) return item.TryUpdate(message, backgroundColor, textColor);

            Request request = _pending.Find(candidate => candidate.Id == id);
            if (request == null) return false;

            request.Message = message ?? request.Message;
            request.BackgroundColor = backgroundColor ?? request.BackgroundColor;
            request.TextColor = textColor ?? request.TextColor;
            return true;
        }

        /// <summary>대기 또는 표시 중인 항목의 문구와 색상을 조회합니다.</summary>
        public bool TryGetToast(int id, out string message, out Color backgroundColor, out Color textColor)
        {
            ToastItemView item = _visible.Find(candidate => candidate.Id == id);
            Request request = item == null ? _pending.Find(candidate => candidate.Id == id) : null;
            message = item != null ? item.Message : request?.Message;
            backgroundColor = item != null ? item.BackgroundColor : request?.BackgroundColor ?? default;
            textColor = item != null ? item.TextColor : request?.TextColor ?? default;
            return item != null || request != null;
        }

        /// 항목을 ID로 제거, 이미 퇴장 중이면 false를 반환하며, 외부 입력 요청으로 닫힌 항목은 Removed 사유로 전달됩니다
        public bool RemoveToast(int id)
        {
            return RequestRemoval(id, ToastItemView.RemovalReason.Removed);
        }

        //항목을 ID로 제거, 이미 퇴장 중이면 false를 반환하며, 외부 입력 요청으로 닫힌 항목은 Dismissed 사유로 전달됩니다
        public bool DismissToast(int id)
        {
            return RequestRemoval(id, ToastItemView.RemovalReason.Dismissed);
        }

        //대기중인 항목과 화면에 표시 중인 항목을 모두 제거합니다, 제거 사유는 Removed로 전달됩니다
        public void ClearAll()
        {
            Request[] pending = _pending.ToArray();
            ToastItemView[] visible = _visible.ToArray();
            _pending.Clear();

            foreach (ToastItemView item in visible) if (item != null) item.TryHide(ToastItemView.RemovalReason.Removed);
            foreach (Request request in pending) NotifyRemoved(request.Id, ToastItemView.RemovalReason.Removed);
        }

        //동시에 표시할 수 있는 항목 수를 설정합니다, 1~5 사이로 제한됩니다
        public void SetMaxToastCount(int count)
        {
            _maxToastCount = Mathf.Clamp(count, 1, MaxVisibleCount);
        }

        //새 항목에 적용할 기본 표시 시간을 설정합니다, 0이면 즉시 퇴장합니다
        public void SetDisplayDuration(float seconds)
        {
            if (!float.IsNaN(seconds) && !float.IsInfinity(seconds)) _displayDuration = Mathf.Max(0f, seconds);
        }

        //등장하는 항목의 배경색과 글자색을 설정, null이면 기본 색상을 사용합니다
        public void SetVisible(bool visible)
        {
            if (!isActiveAndEnabled || !_configured || IsVisible == visible) return;

            IsVisible = visible;
            _animation.SetVisible(visible);
        }

        #endregion

        #region Unity Messages

        private void Awake()
        {
            SetMaxToastCount(_maxToastCount);
            _configured = _toastGroup != null && _itemPrefab != null && _animation != null && _animation.IsConfigured;
            if (!_configured)
            {
                Debug.LogError("ToastView의 Toast Group, Item Prefab, Animation과 Root Group 연결을 확인해 주세요.", this);
                return;
            }

            _animation.SetVisible(IsVisible, true);
        }

        private void LateUpdate()
        {
            if (!_configured || _opening != null || !_toastGroup.gameObject.activeInHierarchy) return;

            bool canSpawn = IsVisible && _pending.Count > 0;
            int targetCount = _maxToastCount - (canSpawn ? 1 : 0);
            if (_visible.Count > targetCount)
            {
                if (!_visible.Exists(item => item.IsHiding)) _visible[0].TryHide(ToastItemView.RemovalReason.Removed);
                return;
            }

            if (canSpawn) SpawnNext();
        }

        private void OnDisable()
        {
            _pending.Clear();
            _opening = null;
            ToastItemView[] items = _visible.ToArray();
            _visible.Clear();

            foreach (ToastItemView item in items) if (item != null) Release(item);
            if (_configured) _animation.SetVisible(IsVisible, true);
        }

        #endregion

        #region Toast List

        private void SpawnNext()
        {
            Request request = _pending[0];
            _pending.RemoveAt(0);

            ToastItemView item = Instantiate(_itemPrefab, _toastGroup);
            item.gameObject.SetActive(true);
            _visible.Add(item);
            _opening = item;
            item.Shown += HandleShown;
            item.Finished += HandleFinished;

            if (item.TryShow(request.Id, request.Message, request.BackgroundColor, request.TextColor, _displayDuration)) return;

            _opening = null;
            _visible.Remove(item);
            Release(item);
            NotifyRemoved(request.Id, ToastItemView.RemovalReason.Removed);
        }

        private bool RequestRemoval(int id, ToastItemView.RemovalReason reason)
        {
            ToastItemView item = _visible.Find(candidate => candidate.Id == id);
            if (item != null) return item.TryHide(reason);

            int index = _pending.FindIndex(candidate => candidate.Id == id);
            if (index < 0) return false;

            _pending.RemoveAt(index);
            NotifyRemoved(id, reason);
            return true;
        }

        private void HandleShown(ToastItemView item)
        {
            if (_opening == item) _opening = null;
            OnToastShown?.Invoke(item.Id);
        }

        private void HandleFinished(ToastItemView item, ToastItemView.RemovalReason reason)
        {
            if (!_visible.Remove(item)) return;
            if (_opening == item) _opening = null;

            int id = item.Id;
            Release(item);
            if (isActiveAndEnabled) NotifyRemoved(id, reason);
        }

        private void NotifyRemoved(int id, ToastItemView.RemovalReason reason)
        {
            OnToastRemoved?.Invoke(id);
            if (reason == ToastItemView.RemovalReason.Expired) OnToastExpired?.Invoke(id);
            else if (reason == ToastItemView.RemovalReason.Dismissed) OnToastDismissed?.Invoke(id);
        }

        private void Release(ToastItemView item)
        {
            item.Shown -= HandleShown;
            item.Finished -= HandleFinished;
            if (item.gameObject.activeInHierarchy) item.gameObject.SetActive(false);

            Destroy(item.gameObject);
        }

        #endregion

#if UNITY_EDITOR
        #region Context Menu

        [ContextMenu("테스트/문구 길이와 색상")]
        private void DebugShowSamples()
        {
            if (!Application.isPlaying) return;
            ShowToast("저장되었습니다.");
            ShowToast("현재 위치에서는 이 행동을 할 수 없습니다.", new Color32(85, 40, 40, 255), Color.white);
        }

        [ContextMenu("테스트/표시 한도 초과")]
        private void DebugExceedLimit()
        {
            if (!Application.isPlaying) return;
            for (int i = 1; i <= _maxToastCount + 1; i++) ShowToast($"토스트 표시 테스트 {i}");
        }

        [ContextMenu("테스트/전체 표시 전환")]
        private void DebugToggleVisibility()
        {
            if (Application.isPlaying) SetVisible(!IsVisible);
        }

        [ContextMenu("테스트/첫 항목 문구와 색상 갱신")]
        private void DebugUpdateFirst()
        {
            if (Application.isPlaying && _visible.Count > 0) UpdateToast(_visible[0].Id, "토스트 표시 내용을 변경했습니다.", new Color32(45, 70, 90, 255), Color.white);
        }

        [ContextMenu("테스트/모두 제거")]
        private void DebugClearAll()
        {
            if (Application.isPlaying) ClearAll();
        }

        #endregion
#endif
    }
}



