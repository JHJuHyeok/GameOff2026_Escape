using UnityEngine;
using Ju.RuleSystem;
using Ju.Player;

namespace Ju.Environment
{
    /// 플레이어가 상호작용 가능한 물체 기본형.
    [RequireComponent(typeof(Collider2D))]
    public abstract class Interactable : MonoBehaviour, ITargetable
    {
        [SerializeField] private int id;
        [Tooltip("사용 완료 여부(해당 내용은 파생 클래스에서 결정)")]
        [SerializeField] private bool singleUse = true;
        
        private bool used;

        public int Id => id;
        public virtual TargetKind Kind => TargetKind.Object;
        public virtual bool CanInteract => !(singleUse && used);

        public void Interact(PlayerController player)
        {
            if (!CanInteract) return;
            OnInteract(player);
        }

        protected abstract void OnInteract(PlayerController player);

        /// 규칙 시스템에 의한 상호작용 이벤트 발행. 발행 시점은 파생 클래스에서 작성
        protected void RaiseInteracted() => PlayerController.Emit(ActionType.Interact, this);

        /// 사용 완료 처리. singleUse가 꺼져 있으면 효과 X
        protected void MarkUsed() => used = true;
    }
}
