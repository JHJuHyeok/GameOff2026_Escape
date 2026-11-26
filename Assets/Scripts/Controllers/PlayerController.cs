using UnityEngine;
using Ju.Environment;
using Ju.RuleSystem;
#if ENABLE_INPUT_SYSTEM
using MainSystem;
#endif
using SH.Project.UI.Checklist;

namespace Ju.Player
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float interactRadius = 1f;
        [SerializeField] private Vector2Int inventorySize = new(10, 10);
        [SerializeField] private float maxWeight = 100f;
        [SerializeField] private bool hardWeightLimit = false;

        private PlayerStats stats;
        private int movingCount;

        public int CurrentFloor { get; private set; }
        public RoomSpace CurrentSpace { get; set; }
        public PlayerInventory Inventories { get; private set; }
        public GridInventory Inventory { get; private set; }
        public Interactable CurrentInteraction { get; set; }
        public PlayerEquipment Equipment { get; private set; }
        public bool IsDead => stats.IsDead;
        public int MovingCount => movingCount;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (this.TryGetComponent<PlayerStats>(out var st)) stats = st;
            else stats = this.GetComponentInParent<PlayerStats>();
            Inventories = new PlayerInventory(maxWeight, hardWeightLimit);
            Inventory = new GridInventory(inventorySize.x, inventorySize.y, "player");
            Inventories.Add(Inventory);

            var itemUser = GetComponent<PlayerItemUser>();
            Equipment = new PlayerEquipment(this, itemUser);
            Equipment.Bind(Inventory);
        }

        private void Start()
        {
            stats.BindInventory(Inventories);
            stats.Died += OnDied;
        }

        private void Update()
        {
            if (stats.IsDead) return;

            float h = ReadMoveX();
            if (h != 0f)
                transform.Translate(Vector3.right * h * moveSpeed * stats.MoveSpeedMultiplier * Time.deltaTime);

            if (ReadInventoryPressed() && InventoryPanel.Instance != null)
                InventoryPanel.Instance.ToggleInventory();

            if (ReadInteractPressed())
            {
                if (CurrentInteraction != null) CurrentInteraction.CancelInteraction();     // 여는 중이면 취소
                else if (InventoryPanel.IsContainerOpen) InventoryPanel.Instance.CloseContainer();
                else TryInteract();
            }

            int equipSlot = ReadEquipSlotPressed();
            if (equipSlot >= 0) Equipment.Toggle(equipSlot);

            if (ReadChecklistPressed() && ChecklistView.Instance != null)
                ChecklistView.Instance.Toggle();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Equipment?.Unbind();
            if (stats != null) stats.Died -= OnDied;
        }

        // ---------------- 입력 읽기 ----------------
#if ENABLE_INPUT_SYSTEM
        private static float ReadMoveX()
        {
            var input = InputManager.Instance;
            if (input == null)
                Debug.Log("InputManager가 비어있음");
            return input != null ? input.Move.x : 0f;
        }
        private static bool ReadInteractPressed()
        {
            var input = InputManager.Instance;
            return input != null && input.InteractPressed;
        }
        private static bool ReadInventoryPressed()
        {
            var input = InputManager.Instance;
            return input != null && input.InventoryPressed;
        }
        private static int ReadEquipSlotPressed()
        {
            var input = InputManager.Instance;
            return input != null ? input.EquipSlotPressed : -1;
        }
        private static bool ReadChecklistPressed()
        {
            var input = InputManager.Instance;
            return input != null && input.ChecklistPressed;
        }
#else
        // Input System이 비활성 중일 때
        private static float ReadMoveX() => Input.GetAxisRaw("Horizontal");
        private static bool ReadInteractPressed() => Input.GetKeyDown(KeyCode.E);
        private static bool ReadInventoryPressed() => Input.GetKeyDown(KeyCode.Tab);
        private static int ReadEquipSlotPressed()
        {
            for (int i = 0; i < PlayerEquipment.SlotCount; i++)
                if (Input.GetKeyDown(KeyCode.Alpha1 + i)) return i;
            return -1;
        }
        private static bool ReadChecklistPressed() => Input.GetKeyDown(KeyCode.I);
#endif

        private void OnDied()
        {
            Equipment.Deactivate();
            // 입력 차단
            // 사망 UI
            // 게임 오버 처리
        }

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
            if (best != null)
            {
                if (best is TeleportObject) movingCount++;
                best.Interact(this);
            }
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
