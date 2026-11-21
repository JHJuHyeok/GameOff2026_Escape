using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ju.RuleSystem;
using Ju.Player;

namespace Ju.Environment
{
    /// 상호작용에 사용할 수 있는 아이템, 사용 시간
    [Serializable]
    public struct ItemRequirement
    {
        public ItemData item;
        [Min(0f)] public float duration;
    }

    /// 플레이어가 상호작용 가능한 물체 기본형.
    [RequireComponent(typeof(Collider2D))]
    public abstract class Interactable : MonoBehaviour, ITargetable
    {
        public const int MaxRequirement = 5;

        [SerializeField] private int id;
        [Tooltip("사용 완료 여부(해당 내용은 파생 클래스에서 결정)")]
        [SerializeField] private bool singleUse = true;

        [Header("Requirement")]
        [Tooltip("비어 있으면 조건 없음. 목록이 하나라도 존재할 경우 아이템 필요.")]
        [SerializeField] private List<ItemRequirement> requirements = new(5);

        [Header("Progress")]
        [Tooltip("이 거리보다 멀어지면 취소. 플레이어 interactRadius보다 클 것.")]
        [SerializeField] protected float cancelDistance = 1.5f;
        
        private bool used;
        private Coroutine running;
        private PlayerController activePlayer;

        protected Collider2D Col { get; private set; }

        public int Id => id;
        public virtual TargetKind Kind => TargetKind.Object;
        public IReadOnlyList<ItemRequirement> Requirements => requirements;
        public bool IsInteracting => running != null;

        // 플레이어와 무관한 상호작용 가능 여부
        public virtual bool CanInteract => !(singleUse && used);

        // ---------------- 파생 클래스용 훅 ----------------
        // false면 아이템 조건과 대기 시간 전부 건너뜀
        protected virtual bool IsGated => true;
        // 아이템 사용 시간과 별도로 Container 열 때 추가되는 소요 시간
        protected virtual float ExtraDuration => 0f;

        protected abstract void OnInteract(PlayerController player);
        protected virtual void OnInteractionCancled() { }

        protected virtual void Awake() => Col = GetComponent<Collider2D>();

        protected virtual void OnValidate()
        {
            if (requirements.Count > MaxRequirement)
                requirements.RemoveRange(MaxRequirement, requirements.Count - MaxRequirement);
        }

        protected virtual void OnDisable() => CancelInteraction();

        // ---------------- 진입점 ----------------
        public void Interact(PlayerController player)
        {
            if (running != null || !CanInteract) return;

            ItemData usedItem = null;
            float duration = 0f;

            if (IsGated)
            {
                if (!TryResolveRequirment(player, out usedItem, out float ItemTime))
                {
                    OnRequirementFailed(player);
                    return;
                }
                duration = ItemTime + ExtraDuration;
            }

            if (duration <= 0f) { OnInteract(player); return; }

            activePlayer = player;
            player.CurrentInteraction = this;
            running = StartCoroutine(Run(player, usedItem, duration));
        }

        public void CancelInteraction()
        {
            if (running == null) return;
            StopCoroutine(running);
            EndProgress();
            OnInteractionCancled();
        }

        // ---------------- 조건 ----------------
        public bool MeetsRequirement(PlayerController player)
            => TryResolveRequirment(player, out _, out _);

        /// 조건이 없으면 true. 조건은 현재 소요 시간 가장 짧은 것 반환(이후 수정 예정).
        private bool TryResolveRequirment(PlayerController player, out ItemData usedItem, out float duration)
        {
            usedItem = null;
            duration = 0f;

            bool hasAny = false;
            float best = float.MaxValue;
            var inventory = player != null ? player.Inventory : null;

            for (int i = 0; i < requirements.Count; i++)
            {
                var r = requirements[i];
                if (r.item == null) continue;               // 빈 슬롯은 무시 (null 허용)
                hasAny = true;

                if (inventory == null || !inventory.Contains(r.item)) continue;
                if (r.duration < best) { best = r.duration; usedItem = r.item; }
            }

            if (!hasAny) return true;
            if (usedItem == null) return false;

            duration = best;
            return true;
        }

        /// 조건 미충족 시 피드백. 소리와 UI 등 여기에 연결.
        protected virtual void OnRequirementFailed(PlayerController player)
        {
            var names = new List<string>();
            foreach (var r in requirements)
                if (r.item != null) names.Add(r.item.ItemName);

            Debug.Log($"[{name}] 필요한 아이템: {string.Join(", ", names)}", this);
        }

        // ---------------- 진행 ----------------
        private IEnumerator Run(PlayerController player, ItemData usedItem, float duration)
        {
            var view = InteractionProgressView.Instance;
            if (view != null) view.Show();

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (!IsStillValid(player, usedItem))
                {
                    CancelInteraction();
                    yield break;
                }

                elapsed += Time.deltaTime;
                if (view != null) view.SetProgress(elapsed / duration);
                yield return null;
            }

            EndProgress();
            OnInteract(player);         // 완료 시점에 결과 실행 (규칙 이벤트도 이때 발행)
        }

        private bool IsStillValid(PlayerController player, ItemData usedItem)
        {
            if (player == null || player.IsDead) return false;
            if (usedItem != null && !player.Inventory.Contains(usedItem)) return false;

            Vector2 p = player.transform.position;
            return (p - Col.ClosestPoint(p)).sqrMagnitude <= cancelDistance * cancelDistance;
        }

        private void EndProgress()
        {
            running = null;
            if (activePlayer != null && activePlayer.CurrentInteraction == this)
                activePlayer.CurrentInteraction = null;
            activePlayer = null;

            var view = InteractionProgressView.Instance;
            if (view != null) view.Hide();
        }

        /// 규칙 시스템에 의한 상호작용 이벤트 발행. 발행 시점은 파생 클래스에서 작성
        protected void RaiseInteracted() => PlayerController.Emit(ActionType.Interact, this);

        /// 사용 완료 처리. singleUse가 꺼져 있으면 효과 X
        protected void MarkUsed() => used = true;
    }
}
