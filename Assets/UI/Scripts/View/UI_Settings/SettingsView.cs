using System;
using UnityEngine;

namespace SH.Project.UI.Settings
{
    public enum SettingsContext
    {
        MainMenu,
        InGame
    }

    public enum SettingsViewState
    {
        Closed,
        Opening,
        Open,
        Closing
    }

    public enum SettingsScreenMode
    {
        Windowed,
        Fullscreen
    }

    public enum SettingsLanguage
    {
        English,
        Korean
    }

    [DisallowMultipleComponent]
    public sealed class SettingsView : MonoBehaviour
    {
        private static readonly Vector2Int[] ResolutionOptions =
        {
            new (1920, 1080),
            new (1600, 900),
            new (1280, 720)
        };

        [SerializeField] private CanvasGroup _backgroundGroup;
        [SerializeField] private CanvasGroup _contentGroup;
        [SerializeField] private SettingsAnimation _settingsAnimation;

        [SerializeField] private SettingsVolumeSliderView _bgmSlider;
        [SerializeField] private SettingsVolumeSliderView _sfxSlider;
        [SerializeField] private SettingsMuteToggleView _allMuteToggle;
        [SerializeField] private SettingsOptionRowView _screenModeRow;
        [SerializeField] private SettingsOptionRowView _resolutionRow;
        [SerializeField] private SettingsOptionRowView _languageRow;
        [SerializeField] private SettingsNavigationView _navigation;

        private SettingsScreenMode _screenMode = SettingsScreenMode.Windowed;
        private Vector2Int _resolution = new (1920, 1080); //기본 해상도는 1920x1080으로 설정
        private SettingsLanguage _language = SettingsLanguage.English;
        private bool _subscribed;

        #region Events

        public event Action OnSettingsOpened; //설정창이 열리고 뒤쪽 UI의 포인터 차단을 시작했을 때 발생
        public event Action OnSettingsClosed; //설정창이 닫히고 뒤쪽 UI의 포인터 차단을 해제했을 때 발생
        public event Action<float> OnBgmVolumeChanged; //사용자가 BGM 슬라이더를 조작하면 0부터 1까지의 값을 전달
        public event Action<float> OnSfxVolumeChanged; //사용자가 SFX 슬라이더를 조작하면 0부터 1까지의 값을 전달
        public event Action<bool> OnAllMuteChanged; //사용자가 전체 음소거를 켜거나 끄면 새 상태를 전달
        public event Action<SettingsScreenMode> OnScreenModeChanged; //사용자가 화면 모드를 바꾸면 새 선택을 전달
        public event Action<Vector2Int> OnResolutionChanged; //사용자가 해상도를 바꾸면 너비와 높이를 전달
        public event Action<SettingsLanguage> OnLanguageChanged; //사용자가 언어를 바꾸면 새 선택을 전달
        public event Action OnBackRequested; //BACK를 눌렀을 때 이전 화면으로 돌아가기를 요청
        public event Action OnMainRequested; //Main을 눌렀을 때 메인 메뉴로 이동하기를 요청

        #endregion

        #region Properties

        public SettingsViewState State { get; private set; } = SettingsViewState.Closed; //설정창의 현재 표시 및 전환 상태를 가져옴
        public SettingsContext Context { get; private set; } = SettingsContext.MainMenu; //마지막 연 화면을 가져옴

        //참조 확인용 프로퍼티, 라이플 사이클 Awake에서 사용합니다
        private bool IsConfigured => _backgroundGroup != null && _contentGroup != null && _settingsAnimation != null &&
            _bgmSlider != null && _bgmSlider.IsConfigured && _sfxSlider != null && _sfxSlider.IsConfigured &&
            _allMuteToggle != null && _allMuteToggle.IsConfigured && _screenModeRow != null && _screenModeRow.IsConfigured &&
            _resolutionRow != null && _resolutionRow.IsConfigured && _languageRow != null && _languageRow.IsConfigured &&
            _navigation != null && _navigation.IsConfigured;

        ///입력을 받을 수 있는지 확인용 
        private bool CanReceiveInput => isActiveAndEnabled && State == SettingsViewState.Open;

        #endregion

        #region Public API

        //설정창을 엽니다, 연출이 진행 중이면 즉시 중단하고 열린 상태로 전환
        public bool Open(SettingsContext context)
        {
            if (!isActiveAndEnabled || !_subscribed) return false;
            if (State == SettingsViewState.Opening || State == SettingsViewState.Open)
            {
                ApplyContext(context); return false;
            }
            if (!PlayTransition(true)) return false;

            ApplyContext(context);
            return true;
        }

        //설정창을 닫습니다 ,연출이 진행 중이면 즉시 중단하고 닫힌 상태로 전환
        public bool Close()
        {
            if (State == SettingsViewState.Closed || State == SettingsViewState.Closing)
                return false;

            return PlayTransition(false);
        }

        //BGM, SFX, 전체 음소거 표시 API
        public void SetAudio(float bgmVolume, float sfxVolume, bool allMuted)
        {
            if (_bgmSlider != null) _bgmSlider.SetVolume(bgmVolume);
            if (_sfxSlider != null) _sfxSlider.SetVolume(sfxVolume);
            if (_allMuteToggle != null) _allMuteToggle.SetMuted(allMuted);
        }

        //화면 모드 표시 API
        public void SetScreenMode(SettingsScreenMode mode)
        {
            _screenMode = mode;

            if (_screenModeRow != null) _screenModeRow.SetText(mode == SettingsScreenMode.Fullscreen ? "FULLSCREEN" : "WINDOW");
        }

        //해상도 표시 API
        public void SetResolution(Vector2Int resolution)
        {
            if (resolution.x <= 0 || resolution.y <= 0) return;

            _resolution = resolution;

            if (_resolutionRow != null) _resolutionRow.SetText($"{resolution.x} × {resolution.y}");
        }

        //언어 표시 API
        public void SetLanguage(SettingsLanguage language)
        {
            _language = language;
            if (_languageRow != null) _languageRow.SetText(language == SettingsLanguage.Korean ? "한국어" : "English");
        }

        #endregion

        #region Unity Messages

        private void Awake()
        {
            ResetToClosed();
            ApplyContext(Context);
            SetScreenMode(_screenMode);
            SetResolution(_resolution);
            SetLanguage(_language);
        }

        private void OnEnable()
        {
            if (!IsConfigured) return;

            _bgmSlider.OnValueChanged += HandleBgmChanged;
            _sfxSlider.OnValueChanged += HandleSfxChanged;
            _allMuteToggle.OnValueChanged += HandleMuteChanged;
            _screenModeRow.OnStepRequested += HandleScreenModeStep;
            _resolutionRow.OnStepRequested += HandleResolutionStep;
            _languageRow.OnStepRequested += HandleLanguageStep;
            _navigation.OnBackClicked += HandleBackClicked;
            _navigation.OnMainClicked += HandleMainClicked;
            _subscribed = true;
        }

        private void OnDisable()
        {
            if (_subscribed)
            {
                _bgmSlider.OnValueChanged -= HandleBgmChanged;
                _sfxSlider.OnValueChanged -= HandleSfxChanged;
                _allMuteToggle.OnValueChanged -= HandleMuteChanged;
                _screenModeRow.OnStepRequested -= HandleScreenModeStep;
                _resolutionRow.OnStepRequested -= HandleResolutionStep;
                _languageRow.OnStepRequested -= HandleLanguageStep;
                _navigation.OnBackClicked -= HandleBackClicked;
                _navigation.OnMainClicked -= HandleMainClicked;
                _subscribed = false;
            }

            ResetToClosed();
        }

        #endregion

        #region Transition

        private void ApplyContext(SettingsContext context)
        {
            Context = context;

            if (_navigation != null) _navigation.SetMainVisible(context == SettingsContext.InGame);
        }

        private bool PlayTransition(bool open)
        {
            if (!_settingsAnimation.TryPlay(_backgroundGroup, _contentGroup, open,() => CompleteTransition(open), ResetToClosed)) return false;

            State = open ? SettingsViewState.Opening : SettingsViewState.Closing;
            ApplyInputState();
            return true;
        }

        private void CompleteTransition(bool opened)
        {
            State = opened ? SettingsViewState.Open : SettingsViewState.Closed;
            ApplyInputState();

            if (opened) OnSettingsOpened?.Invoke();
            else OnSettingsClosed?.Invoke();
        }

        private void ResetToClosed()
        {
            State = SettingsViewState.Closed;

            if (_settingsAnimation != null) _settingsAnimation.ResetToHidden(_backgroundGroup, _contentGroup);
            if (_backgroundGroup != null)
            {
                _backgroundGroup.interactable = false;
                _backgroundGroup.blocksRaycasts = false;
            }

            ApplyInputState();
        }

        private void ApplyInputState()
        {
            if (_contentGroup == null) return;

            _contentGroup.interactable = CanReceiveInput;
            _contentGroup.blocksRaycasts = isActiveAndEnabled && State != SettingsViewState.Closed;
        }

        #endregion

        #region Input Callbacks

        private void HandleBgmChanged(float value)
        {
            if (CanReceiveInput) OnBgmVolumeChanged?.Invoke(value);
        }

        private void HandleSfxChanged(float value)
        {
            if (CanReceiveInput) OnSfxVolumeChanged?.Invoke(value);
        }

        private void HandleMuteChanged(bool value)
        {
            if (CanReceiveInput) OnAllMuteChanged?.Invoke(value);
        }

        private void HandleScreenModeStep(int _)
        {
            if (!CanReceiveInput) return;

            SettingsScreenMode next = _screenMode == SettingsScreenMode.Windowed ? SettingsScreenMode.Fullscreen : SettingsScreenMode.Windowed;
            SetScreenMode(next);
            OnScreenModeChanged?.Invoke(next);
        }

        private void HandleResolutionStep(int step)
        {
            if (!CanReceiveInput) return;

            int index = Array.IndexOf(ResolutionOptions, _resolution);
            // 프리셋에 없는 해상도에서 이동하면 누른 방향에 맞춰 목록의 첫 항목이나 끝 항목으로 진입
            if (index < 0) index = step > 0 ? ResolutionOptions.Length - 1 : 0;

            int nextIndex = (index + step + ResolutionOptions.Length) % ResolutionOptions.Length;
            Vector2Int next = ResolutionOptions[nextIndex];
            SetResolution(next);
            OnResolutionChanged?.Invoke(next);
        }

        private void HandleLanguageStep(int _)
        {
            if (!CanReceiveInput) return;

            SettingsLanguage next = _language == SettingsLanguage.English ? SettingsLanguage.Korean : SettingsLanguage.English;
            SetLanguage(next);
            OnLanguageChanged?.Invoke(next);
        }

        private void HandleBackClicked()
        {
            if (CanReceiveInput) OnBackRequested?.Invoke();
        }

        private void HandleMainClicked()
        {
            if (CanReceiveInput && Context == SettingsContext.InGame) OnMainRequested?.Invoke();
        }

        #endregion

#if UNITY_EDITOR
        #region Context Menu

        [ContextMenu("Settings/Open From Main Menu")]
        private void OpenFromMainMenuForTest()
        {
            OpenForTest(SettingsContext.MainMenu);
        }

        [ContextMenu("Settings/Open From In Game")]
        private void OpenFromInGameForTest()
        {
            OpenForTest(SettingsContext.InGame);
        }

        [ContextMenu("Settings/Close")]
        private void CloseForTest()
        {
            if (Application.isPlaying) Close();
        }

        private void OpenForTest(SettingsContext context)
        {
            if (!Application.isPlaying) return;
            if (!_subscribed)
            {
                Debug.LogWarning("설정창과 각 행의 Inspector 참조를 확인하세요.", this); return;
            }

            Open(context);
        }

        #endregion
#endif
    }
}
