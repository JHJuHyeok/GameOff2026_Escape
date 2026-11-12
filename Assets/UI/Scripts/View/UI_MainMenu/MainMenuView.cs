using System;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.MainMenu
{
    public enum MainMenuState
    {
        Hidden,
        Showing,
        Visible,
        Hiding
    }

    [DisallowMultipleComponent]
    public sealed class MainMenuView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _backgroundGroup;
        [SerializeField] private CanvasGroup _contentGroup;
        [SerializeField] private MainMenuAnimation _menuAnimation;

        [SerializeField] private Button _startButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        private bool _inputEnabled = true;

        #region Events

        public event Action OnMenuStarted; //메인 메뉴 등장 연출 시작 시 발생
        public event Action OnMenuCompleted; //메인 메뉴 등장 연출 완료 시 발생
        public event Action OnMenuHidden; //메인 메뉴 퇴장 연출 완료 시 발생
        public event Action OnStartRequested; ///시작 버튼 클릭 완료 시 발생
        public event Action OnSettingsRequested; ///설정 버튼 클릭 완료 시 발생
        public event Action OnQuitRequested; ///종료 버튼 클릭 완료 시 발생

        #endregion

        #region Properties

        public MainMenuState State { get; private set; } = MainMenuState.Hidden; //메인 메뉴 상태 체크용 프로퍼티
        private bool CanReceiveInput => isActiveAndEnabled && State == MainMenuState.Visible && _inputEnabled; //메인 메뉴가 표시되고, 버튼 입력 처리가 가능한지 체크용 프로퍼티

        #endregion

        #region Public API

        //메인 메뉴 연출 처리, 퇴장 중이면 현재 알파에서 등장 연출로 전환
        public bool Show()
        {
            if (State == MainMenuState.Showing || State == MainMenuState.Visible) return false;

            return PlayTransition(true);
        }

        ///메인 메뉴 연출 처리, 등장 중이면 현재 알파에서 퇴장 연출로 전환
        public bool Hide()
        {
            if (State == MainMenuState.Hiding || State == MainMenuState.Hidden) return false;

            return PlayTransition(false);
        }

        ///메인 메뉴 버튼 입력 처리 가능 여부 설정, false면 버튼 클릭 무시
        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;
            ApplyInputState();
        }

        #endregion

        #region Unity Messages

        private void OnEnable()
        {
            if (_startButton != null) _startButton.onClick.AddListener(HandleStartClicked);
            if (_settingsButton != null) _settingsButton.onClick.AddListener(HandleSettingsClicked);
            if (_quitButton != null) _quitButton.onClick.AddListener(HandleQuitClicked);
        }

        private void OnDisable()
        {
            if (_startButton != null) _startButton.onClick.RemoveListener(HandleStartClicked);
            if (_settingsButton != null) _settingsButton.onClick.RemoveListener(HandleSettingsClicked);
            if (_quitButton != null) _quitButton.onClick.RemoveListener(HandleQuitClicked);
            if (_menuAnimation != null) _menuAnimation.Stop();

            ResetToHidden();
        }

        #endregion

        #region Transitions

        private bool PlayTransition(bool show)
        {
            if (!isActiveAndEnabled || _menuAnimation == null ||_backgroundGroup == null || _contentGroup == null ||
                _startButton == null || _settingsButton == null || _quitButton == null) return false;
            if (!_menuAnimation.TryPlay(_backgroundGroup, _contentGroup, show,() => CompleteTransition(show), ResetToHidden)) return false;

            State = show ? MainMenuState.Showing : MainMenuState.Hiding;
            ApplyInputState();

            if (show) OnMenuStarted?.Invoke();

            return true;
        }

        private void CompleteTransition(bool shown)
        {
            State = shown ? MainMenuState.Visible : MainMenuState.Hidden;
            ApplyInputState();

            if (shown) OnMenuCompleted?.Invoke();
            else OnMenuHidden?.Invoke();
        }

        private void ResetToHidden()
        {
            State = MainMenuState.Hidden;

            if (_backgroundGroup != null) _backgroundGroup.alpha = 0f;
            if (_contentGroup != null) _contentGroup.alpha = 0f;

            ApplyInputState();
        }

        private void ApplyInputState()
        {
            if (_contentGroup == null) return;

            bool canReceiveInput = CanReceiveInput;
            _contentGroup.interactable = canReceiveInput;
            _contentGroup.blocksRaycasts = canReceiveInput;
        }

        #endregion

        #region Button Callbacks

        private void HandleStartClicked()
        {
            if (CanReceiveInput) OnStartRequested?.Invoke();
        }

        private void HandleSettingsClicked()
        {
            if (CanReceiveInput) OnSettingsRequested?.Invoke();
        }

        private void HandleQuitClicked()
        {
            if (CanReceiveInput) OnQuitRequested?.Invoke();
        }

        #endregion

#if UNITY_EDITOR
        #region Context Menu

        [ContextMenu("Main Menu/Show")]
        private void ShowFromContextMenu()
        {
            if (Application.isPlaying && !Show()) Debug.LogWarning("메인 메뉴 등장 연출을 시작할 수 없습니다. 표시 상태와 Inspector 참조를 확인하세요.", this);
        }

        [ContextMenu("Main Menu/Hide")]
        private void HideFromContextMenu()
        {
            if (Application.isPlaying && !Hide()) Debug.LogWarning("메인 메뉴 퇴장 연출을 시작할 수 없습니다. 표시 상태와 Inspector 참조를 확인하세요.", this);
        }

        #endregion
#endif
    }
}
