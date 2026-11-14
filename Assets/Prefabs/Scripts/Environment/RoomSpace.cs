using UnityEngine;
using Ju.RuleSystem;
using Ju.Player;

namespace Ju.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class RoomSpace : MonoBehaviour, ITargetable
    {
        [SerializeField] int id;
        [SerializeField] int floor;

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
    }
}
