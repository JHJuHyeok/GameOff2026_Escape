using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using MainSystem;
#endif

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float interactRadius = 1f;
        [SerializeField] private PlayerStats stats;
        [SerializeField] private GridInventory inventory;

        public int CurrentFloor { get; private set; }
        public GridInventory Inventory => inventory;

        private void Awake() => Instance = this;

        private void Update()
        {
            if (stats.IsDead) return;

            float h = ReadMoveX();
            //if (h != 0f)
            //    transform.Translate(Vector3.right * h * moveSpeed * stats.MoveSpeedMultiplier * Time.deltaTime);

            if (ReadInteractPressed()) TryInteract();
        }

        // ---------------- 입력 읽기 ----------------
#if ENABLE_INPUT_SYSTEM
        private static float ReadMoveX()
        {
            var input = InputManager.Instance;
            return input != null ? input.Move.x : 0f;
        }

        private static bool ReadInteractPressed()
        {
            var input = InputManager.Instance;
            return input != null && input.InteractPressed;
        }
#else
        // Input System이 비활성 중일 때
        private static float ReadMoveX() => Input.GetAxisRaw("Horizontal");
        private static bool ReadInteractPressed() => Input.GetKeyDown(KeyCode.E);
#endif

        // ---------------- 상호작용 ----------------
        private void TryInteract()
        {
            // Interactable best = null;
        }
    }
}
