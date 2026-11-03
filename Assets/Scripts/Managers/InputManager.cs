#if ENABLE_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;

namespace MainSystem
{
    /// <summary>
    /// 새 Input System 래퍼. InputActionAsset(.inputactions)을 인스펙터에 할당해서 사용.
    /// 게임플레이 코드가 InputAction을 직접 다루지 않고 이 매니저의 프로퍼티만 읽도록 한다.
    ///
    /// 필요한 액션 (이름은 인스펙터에서 변경 가능):
    ///   Player 맵: Move(Vector2), Look(Vector2), Jump, Attack, Interact, Pause
    ///
    /// 사용 예:
    ///   Vector2 dir = InputManager.Instance.Move;
    ///   if (InputManager.Instance.JumpPressed) Jump();
    ///
    /// ※ Project Settings > Player > Active Input Handling이 Input System Package(New) 또는 Both여야 함.
    /// </summary>
    public class InputManager : Singleton<InputManager>
    {
        [SerializeField] private InputActionAsset inputActions;

        [Header("Action Map 이름")]
        [SerializeField] private string gameplayMap = "Player";
        [SerializeField] private string uiMap = "UI";

        [Header("Action 이름 (Gameplay 맵 안)")]
        [SerializeField] private string moveName = "Move";
        [SerializeField] private string lookName = "Look";
        [SerializeField] private string jumpName = "Jump";
        [SerializeField] private string attackName = "Attack";
        [SerializeField] private string interactName = "Interact";
        [SerializeField] private string pauseName = "Pause";

        private InputActionMap _gameplay, _ui;
        private InputAction _move, _look, _jump, _attack, _interact, _pause;

        public bool GameplayEnabled { get; private set; }

        // ---------------- 값 읽기 ----------------

        public Vector2 Move => _move != null ? _move.ReadValue<Vector2>() : Vector2.zero;
        public Vector2 Look => _look != null ? _look.ReadValue<Vector2>() : Vector2.zero;

        public bool JumpPressed => _jump != null && _jump.WasPressedThisFrame();
        public bool JumpHeld => _jump != null && _jump.IsPressed();
        public bool JumpReleased => _jump != null && _jump.WasReleasedThisFrame();

        public bool AttackPressed => _attack != null && _attack.WasPressedThisFrame();
        public bool AttackHeld => _attack != null && _attack.IsPressed();

        public bool InteractPressed => _interact != null && _interact.WasPressedThisFrame();
        public bool PausePressed => _pause != null && _pause.WasPressedThisFrame();

        // ---------------- 초기화 ----------------

        protected override void Init()
        {
            if (inputActions == null)
            {
                Debug.LogWarning("[InputManager] InputActionAsset이 할당되지 않았습니다. 입력이 동작하지 않습니다.");
                return;
            }

            // 원본 에셋의 활성 상태가 에디터 플레이 간에 남지 않도록 복제본 사용
            inputActions = Instantiate(inputActions);

            _gameplay = inputActions.FindActionMap(gameplayMap, false);
            _ui = inputActions.FindActionMap(uiMap, false);

            if (_gameplay == null)
            {
                Debug.LogWarning($"[InputManager] '{gameplayMap}' 액션 맵을 찾을 수 없습니다.");
                return;
            }

            _move = _gameplay.FindAction(moveName, false);
            _look = _gameplay.FindAction(lookName, false);
            _jump = _gameplay.FindAction(jumpName, false);
            _attack = _gameplay.FindAction(attackName, false);
            _interact = _gameplay.FindAction(interactName, false);
            _pause = _gameplay.FindAction(pauseName, false);

            SetGameplayEnabled(true);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _gameplay?.Disable();
            _ui?.Disable();
        }

        // ---------------- 입력 전환 ----------------

        /// <summary>
        /// 게임플레이 입력 켜기/끄기 (컷신, 대화, 일시정지 중 등).
        /// 일시정지 해제를 위해 Pause 액션은 항상 활성 상태로 유지한다.
        /// </summary>
        public void SetGameplayEnabled(bool value)
        {
            GameplayEnabled = value;
            if (value) _gameplay?.Enable();
            else _gameplay?.Disable();

            _pause?.Enable();
        }

        public void SetUIEnabled(bool value)
        {
            if (value) _ui?.Enable();
            else _ui?.Disable();
        }

        /// <summary>임의의 액션 접근 (리바인딩 등 확장용)</summary>
        public InputAction FindAction(string actionName) => inputActions != null ? inputActions.FindAction(actionName, false) : null;
    }
}
#endif
