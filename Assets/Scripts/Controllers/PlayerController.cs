using UnityEngine;
using Ju.Environment;
using Ju.RuleSystem;
#if ENABLE_INPUT_SYSTEM
using MainSystem;
#endif

namespace Ju.Player
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float interactRadius = 1f;
        [SerializeField] private PlayerStats stats;
        [SerializeField] private GridInventory inventory;
        [SerializeField] private Vector2Int inventorySize = new(10, 10);

        public int CurrentFloor { get; private set; }
        public RoomSpace CurrentSpace { get; set; }
        public GridInventory Inventory { get; private set; }

        private static InputManager s_input = InputManager.Instance;

        private void Awake()
        {
            Instance = this;
            Inventory = new GridInventory(inventorySize.x, inventorySize.y, "player");
        }

        private void Update()
        {
            if (stats.IsDead) return;

            float h = ReadMoveX();
            //if (h != 0f)
            //    transform.Translate(Vector3.right * h * moveSpeed * stats.MoveSpeedMultiplier * Time.deltaTime);

            if (ReadInventoryPressed() && InventoryPanel.Instance != null)
                InventoryPanel.Instance.ToggleInventory();

            if (ReadInteractPressed()) TryInteract();
        }

        // ---------------- 입력 읽기 ----------------
#if ENABLE_INPUT_SYSTEM
        private static float ReadMoveX() => s_input != null ? s_input.Move.x : 0f;
        private static bool ReadInteractPressed() => s_input != null && s_input.InteractPressed;
        private static bool ReadInventoryPressed() => s_input != null && s_input.InventoryPressed;
        
#else
        // Input System이 비활성 중일 때
        private static float ReadMoveX() => Input.GetAxisRaw("Horizontal");
        private static bool ReadInteractPressed() => Input.GetKeyDown(KeyCode.E);
        private static bool ReadInventoryPressed() => Input.GetKeyDown(KeyCode.Tab);
#endif

        // ---------------- 상호작용 ----------------
        private void TryInteract()
        {
            Interactable best = null; float bestDict = float.MaxValue;
            foreach (var col in Physics2D.OverlapCircleAll(transform.position, interactRadius))
            {
                if (!col.TryGetComponent(out Interactable it) || !it.CanInteract) continue;
                float d = ((Vector2)col.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (d < bestDict) { best = it; bestDict = d; }
            }
            if (best != null) best.Interact(this);
        }

        public void TeleportTo(Vector3 position, int floor)
        {
            transform.position = position;
            CurrentFloor = floor;
        }

        /// 규칙 이벤트 발행 단축 메서드
        public static void Emit(ActionType action, ITargetable target, int? floorOverride = null)
        {
            int floor = floorOverride ?? (Instance != null ? Instance.CurrentFloor : 0);
            EventBus<RuleEvent>.Raise(new RuleEvent(action, target, floor));
        }
    }
}
