using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.CommonPopup
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CommonPopupAnimation))]
    public sealed class CommonPopupView : MonoBehaviour
    {
        private enum PopupState
        {
            Hidden,
            Opening,
            Open,
            Closing
        }

        [SerializeField] private CanvasGroup _rootGroup;
        [SerializeField] private CanvasGroup _contentGroup;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _detailText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private TMP_Text _confirmLabel;
        [SerializeField] private TMP_Text _cancelLabel;
        [SerializeField] private string _defaultConfirmText = "CONFIRM";
        [SerializeField] private string _defaultCancelText = "CANCEL";

        [SerializeField] private CommonPopupAnimation _popupAnimation;

        private PopupState _state;
        private bool _initialized;

        #region Events

        public event Action OnPopupOpened; //연출이 끝나고 버튼 입력이 가능한 상태가 되면 발생
        public event Action OnPopupClosed; //팝업이 닫히고 뒤쪽 UI의 포인터 차단이 해제되면 발생
        public event Action OnConfirmClicked; //열린 팝업에서 확인 버튼을 눌렀을 때 발생
        public event Action OnCancelClicked; //취소 버튼이 활성화되어 있을 때만 발생

        #endregion

        #region Properties

        public bool IsOpen => _state == PopupState.Open; //연출이 끝나고 버튼 입력이 가능한 상태

        private bool CanShow => isActiveAndEnabled && _initialized && _popupAnimation.isActiveAndEnabled && _state == PopupState.Hidden;

        #endregion

        #region Public API

        ///설정된 문구와 버튼 구성으로 팝업을 열고, 연출을 시작했으면 true를 반환합니다
        public bool Show()
        {
            if (!CanShow) return false;

            _state = PopupState.Opening;
            _rootGroup.blocksRaycasts = true;

            _popupAnimation.Play(_rootGroup, true, FinishOpening, ResetToHidden);
            _contentGroup.gameObject.SetActive(true);
            return true;
        }

        //설정된 문구와 버튼 구성으로 팝업을 열고, 연출을 시작했으면 true를 반환합니다
        //추후 SO기반 데이터를 처리해야할 필요가 있습니다, 또는 특정 컨트롤러나, 이벤트에서 명시된 내용을 전달해줘야합니다
        public bool Show(string detail, bool showCancel = false, string title = null,string confirmText = null, string cancelText = null)
        {
            if (!CanShow) return false;

            SetTitle(title);
            SetDetail(detail);
            SetButtonCount(showCancel ? 2 : 1);
            SetConfirmText(confirmText);
            SetCancelText(cancelText);
            return Show();
        }

        /// <summary>팝업을 닫고, 닫기 요청을 처리했으면 true를 반환합니다.</summary>
        public bool Hide()
        {
            if (_state != PopupState.Opening && _state != PopupState.Open) return false;

            _state = PopupState.Closing;
            _contentGroup.interactable = false;

            if (_popupAnimation.isActiveAndEnabled) _popupAnimation.Play(_rootGroup, false, FinishClosing, ResetToHidden);
            else FinishClosing();

            return true;
        }

        ///상단 제목을 설정, 비어 있으면 제목 영역을 숨깁니다
        public void SetTitle(string title)
        {
            _titleText.text = title ?? string.Empty;
            _titleText.gameObject.SetActive(!string.IsNullOrWhiteSpace(title));
        }

        //상세 문구를 설정, 비어 있으면 상세 영역을 숨깁니다
        public void SetDetail(string detail)
        {
            _detailText.text = detail ?? string.Empty;
        }

        //버튼 개수를 설정, 1 또는 2만 허용하며, 2이면 취소 버튼을 표시합니다
        public void SetButtonCount(int count)
        {
            if (count != 1 && count != 2) throw new ArgumentOutOfRangeException(nameof(count), "버튼 개수는 1 또는 2여야 합니다.");
            _cancelButton.gameObject.SetActive(count == 2);
        }

        /// 확인 버튼의 문구를 설정, 비어 있으면 Inspector 기본 문구를 사용합니다
        public void SetConfirmText(string text)
        {
            _confirmLabel.text = string.IsNullOrWhiteSpace(text) ? _defaultConfirmText : text;
        }

        //취소 버튼의 문구를 설정, 비어 있으면 Inspector 기본 문구를 사용합니다
        public void SetCancelText(string text)
        {
            _cancelLabel.text = string.IsNullOrWhiteSpace(text) ? _defaultCancelText : text;
        }

        #endregion

        #region Unity Messages

        private void Awake()
        {
            _initialized = _popupAnimation != null &&
                _rootGroup != null && _contentGroup != null && _rootGroup != _contentGroup &&
                _titleText != null && _detailText != null && _confirmButton != null && _cancelButton != null &&
                _confirmButton != _cancelButton && _confirmLabel != null && _cancelLabel != null;

            if (!_initialized)
            {
                Debug.LogError("CommonPopupView의 Inspector 연결을 확인해 주세요.", this);
                enabled = false;
                return;
            }

            _popupAnimation.Initialize(_contentGroup.transform);
            ResetToHidden();
        }

        private void OnEnable()
        {
            if (!_initialized) return;
            _confirmButton.onClick.AddListener(HandleConfirm);
            _cancelButton.onClick.AddListener(HandleCancel);
        }

        private void OnDisable()
        {
            if (!_initialized) return;
            _confirmButton.onClick.RemoveListener(HandleConfirm);
            _cancelButton.onClick.RemoveListener(HandleCancel);
            ResetToHidden();
        }

        #endregion

        #region Popup State

        private void FinishOpening()
        {
            _state = PopupState.Open;
            _contentGroup.interactable = true;
            OnPopupOpened?.Invoke();
        }

        private void FinishClosing()
        {
            ResetToHidden();
            OnPopupClosed?.Invoke();
        }

        private void ResetToHidden()
        {
            _state = PopupState.Hidden;
            _contentGroup.interactable = false;
            _rootGroup.blocksRaycasts = false;
            _popupAnimation.ResetToHidden(_rootGroup);
            _contentGroup.gameObject.SetActive(false);

            SetTitle(null);
            SetDetail(null);
            SetButtonCount(1);
            SetConfirmText(null);
            SetCancelText(null);
        }

        private void HandleConfirm()
        {
            if (IsOpen) OnConfirmClicked?.Invoke();
        }

        private void HandleCancel()
        {
            if (IsOpen && _cancelButton.gameObject.activeInHierarchy) OnCancelClicked?.Invoke();
        }

        #endregion

#if UNITY_EDITOR
        #region Context Menu

        [ContextMenu("테스트/확인형 열기")]
        private void DebugShowConfirmOnly()
        {
            if (Application.isPlaying && CanShow) StartCoroutine(DebugAfterFrame(() => Show("설정이 적용되었습니다.")));
        }

        [ContextMenu("테스트/확인 취소형 열기")]
        private void DebugShowWithCancel()
        {
            if (Application.isPlaying && CanShow) StartCoroutine(DebugAfterFrame(() => Show("메인 화면으로 돌아가시겠습니까?", true, "메인 화면으로 이동")));
        }

        [ContextMenu("테스트/닫기")]
        private void DebugHide()
        {
            if (Application.isPlaying && isActiveAndEnabled) StartCoroutine(DebugAfterFrame(() => Hide()));
        }

        private System.Collections.IEnumerator DebugAfterFrame(Action testAction)
        {
            yield return null;
            if (isActiveAndEnabled) testAction();
        }

        #endregion
#endif
    }
}
