using System.Collections.Generic;
using UnityEngine;
using Ju.RuleSystem;
using Ju.Player;

namespace Ju.Environment
{
    public class RoomSpace : MonoBehaviour, ITargetable
    {
        [SerializeField] private int id;
        [SerializeField] private int floor;

        [Header("내부 Interactable")]
        [SerializeField] private List<Interactable> interactables;

        public int Id => id;
        public TargetKind Kind => TargetKind.Space;
        public int Floor => floor;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.TryGetComponent(out PlayerController p)) return;
            p.CurrentSpace = this;
            PlayerController.Emit(ActionType.Enter, this, floor);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.TryGetComponent(out PlayerController p)) return;
            if (p.CurrentSpace == this) p.CurrentSpace = null;
            PlayerController.Emit(ActionType.Exit, this, floor);
        }

        // TeleportObject를 제외한 Interactable 랜덤 활성화

        [ContextMenu("방 내부 오브젝트 등록")]
        private void RegistInteractables()
        {
            foreach (var it in transform.GetComponentsInChildren<Interactable>())
                if (!interactables.Contains(it)) interactables.Add(it);
        }
    }
}
