using System;
using SH.Project.UI.PlayerInfoHUD;
using SH.Project.UI.QuickSlotHUD;
using UnityEngine;

namespace SH.Project.UI.MainHUD
{
    public enum MainHUDState
    {
        Hidden,
        Showing,
        Visible,
        Hiding
    }

    [DisallowMultipleComponent]
    public sealed class MainHUDView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _hudGroup;
        [SerializeField] private MainHUDAnimation _hudAnimation;
        [SerializeField] private PlayerInfoHUDView _playerInfo;
        [SerializeField] private QuickSlotHUDView _quickSlots;
        [SerializeField] private MainHUDSettingsButtonView _settingsButton;
        [SerializeField] private GameObject _shortcutGuideGroup;
        [SerializeField] private bool _startVisible = true;

        private bool _requestedVisible;

        #region Events

        /// <summary>HUD 등장 연출이 끝나고 포인터 입력이 활성화됐을 때 발생합니다.</summary>
        public event Action OnHUDOpened;

        /// <summary>HUD 퇴장 연출이 끝나고 화면에서 숨겨졌을 때 발생합니다.</summary>
        public event Action OnHUDClosed;

        /// <summary>HUD의 설정 버튼을 눌러 설정 화면 열기를 요청했을 때 발생합니다.</summary>
        public event Action OnSettingsRequested;

        #endregion

        #region Properties

        /// <summary>HUD 전체의 표시 및 전환 상태입니다.</summary>
        public MainHUDState State { get; private set; } = MainHUDState.Hidden;

        /// <summary>플레이어 상태와 상세 정보를 갱신할 하위 View입니다.</summary>
        public PlayerInfoHUDView PlayerInfo => _playerInfo;

        /// <summary>슬롯 아이템, 내구도, 선택 표시를 갱신할 하위 View입니다.</summary>
        public QuickSlotHUDView QuickSlots => _quickSlots;

        #endregion

        #region Public API

        /// <summary>HUD 전체를 페이드 인합니다. 퇴장 중이면 현재 알파에서 방향을 바꿉니다.</summary>
        /// <returns>등장 연출을 시작했으면 true입니다.</returns>
        public bool Open()
        {
            if (State == MainHUDState.Showing || State == MainHUDState.Visible)
                return false;

            return PlayVisibility(true);
        }

        /// <summary>HUD 전체를 페이드 아웃합니다. 등장 중이면 현재 알파에서 방향을 바꿉니다.</summary>
        /// <returns>퇴장 연출을 시작했으면 true입니다.</returns>
        public bool Close()
        {
            if (State == MainHUDState.Hiding || State == MainHUDState.Hidden)
                return false;

            return PlayVisibility(false);
        }

        /// <summary>플레이어 상태 HUD의 열림 또는 닫힘 상태를 변경합니다.</summary>
        /// <param name="state">플레이어 상태 HUD에 적용할 상태입니다.</param>
        public void SetPlayerInfoState(PlayerInfoHUDState state)
        {
            if (_playerInfo != null)
                _playerInfo.SetState(state);
        }

        /// <summary>퀵 슬롯 HUD의 열림 또는 닫힘 상태를 변경합니다.</summary>
        /// <param name="state">퀵 슬롯 HUD에 적용할 상태입니다.</param>
        public void SetQuickSlotState(QuickSlotHUDState state)
        {
            if (_quickSlots != null)
                _quickSlots.SetState(state);
        }

        /// <summary>인벤토리의 표시 상태에 맞춰 플레이어 상태와 퀵 슬롯 HUD를 함께 변경합니다.</summary>
        /// <param name="isOpen">인벤토리가 열려 있으면 true입니다.</param>
        public void SetInventoryOpen(bool isOpen)
        {
            SetPlayerInfoState(isOpen ? PlayerInfoHUDState.Expanded : PlayerInfoHUDState.Collapsed);
            SetQuickSlotState(isOpen ? QuickSlotHUDState.Expanded : QuickSlotHUDState.Collapsed);
        }

        /// <summary>우하단 설정과 체크리스트 단축키 안내를 표시하거나 숨깁니다.</summary>
        /// <param name="visible">단축키 안내를 표시하려면 true입니다.</param>
        public void SetShortcutGuideVisible(bool visible)
        {
            if (_shortcutGuideGroup != null)
                _shortcutGuideGroup.SetActive(visible);
        }

        #endregion

        #region Unity Messages

        private void Awake()
        {
            _requestedVisible = _startVisible;
        }

        private void OnEnable()
        {
            if (_settingsButton != null)
                _settingsButton.OnClicked += HandleSettingsClicked;

            ApplyVisibilityImmediately(_requestedVisible);
        }

        private void OnDisable()
        {
            if (_settingsButton != null)
                _settingsButton.OnClicked -= HandleSettingsClicked;

            ApplyVisibilityImmediately(false);
        }

        #endregion

        #region Visibility and Input

        private bool PlayVisibility(bool visible)
        {
            if (!isActiveAndEnabled || _hudGroup == null || _hudAnimation == null)
                return false;

            if (!_hudAnimation.TryPlay(_hudGroup, visible,
                    () => CompleteVisibility(visible), HandleAnimationInterrupted))
                return false;

            _requestedVisible = visible;
            State = visible ? MainHUDState.Showing : MainHUDState.Hiding;
            ApplyInputState();
            return true;
        }

        private void CompleteVisibility(bool visible)
        {
            State = visible ? MainHUDState.Visible : MainHUDState.Hidden;
            ApplyInputState();

            if (visible)
                OnHUDOpened?.Invoke();
            else
                OnHUDClosed?.Invoke();
        }

        private void ApplyVisibilityImmediately(bool visible)
        {
            if (_hudAnimation != null)
                _hudAnimation.SetVisibleImmediately(_hudGroup, visible);

            State = visible ? MainHUDState.Visible : MainHUDState.Hidden;
            ApplyInputState();
        }

        private void HandleAnimationInterrupted()
        {
            ApplyVisibilityImmediately(isActiveAndEnabled && _requestedVisible);
        }

        private void ApplyInputState()
        {
            if (_hudGroup == null)
                return;

            bool canReceiveInput = isActiveAndEnabled && State == MainHUDState.Visible;
            _hudGroup.interactable = canReceiveInput;
            _hudGroup.blocksRaycasts = canReceiveInput;
        }

        private void HandleSettingsClicked()
        {
            if (isActiveAndEnabled && State == MainHUDState.Visible)
                OnSettingsRequested?.Invoke();
        }

        #endregion

#if UNITY_EDITOR
        #region Context Menu

        [ContextMenu("Main HUD/Open")]
        private void OpenForTest()
        {
            if (Application.isPlaying)
                Open();
        }

        [ContextMenu("Main HUD/Close")]
        private void CloseForTest()
        {
            if (Application.isPlaying)
                Close();
        }

        [ContextMenu("Main HUD/Open Inventory Layout")]
        private void OpenInventoryLayoutForTest()
        {
            if (Application.isPlaying)
                SetInventoryOpen(true);
        }

        [ContextMenu("Main HUD/Close Inventory Layout")]
        private void CloseInventoryLayoutForTest()
        {
            if (Application.isPlaying)
                SetInventoryOpen(false);
        }

        #endregion
#endif
    }
}
