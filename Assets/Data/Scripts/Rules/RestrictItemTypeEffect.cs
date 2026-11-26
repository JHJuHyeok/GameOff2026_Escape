using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ju.RuleSystem
{
    /// 특정 아이템 타입 획득을 차단. 이미 획득한 아이템은 상관 없음.
    [CreateAssetMenu(menuName = "Effects/Restrict Item Type")]
    public class RestrictItemTypeEffect : RuleEffect
    {
        public ItemType blockedType = ItemType.Luxury;
        [Tooltip("0이면 영구 차단")] public float duration = 0f;
        public string reasonText = "지금은 가져갈 수 없다.";

        public override void Apply(RuleContext ctx, RuleEvent trigger)
        {
            var owner = ctx.Player.Inventories;
            var rule = new TypeBlockRule(owner, blockedType, reasonText);
            var grids = new List<GridInventory>(owner.Inventories);
            foreach (var g in grids) g.AddRule(rule);

            if (duration > 0f) ctx.Runner.StartCoroutine(Release(grids, rule));
        }

        private IEnumerator Release(List<GridInventory> grids, IPlacementRule rule)
        {
            yield return new WaitForSeconds(duration);
            foreach (var g in grids) g.RemoveRule(rule);
        }

        private sealed class TypeBlockRule : IPlacementRule
        {
            private readonly PlayerInventory owner;
            private readonly ItemType blocked;
            private readonly string reasonText;

            public TypeBlockRule(PlayerInventory owner, ItemType blocked, string reasonText)
            { this.owner = owner; this.blocked = blocked; this.reasonText = reasonText; }

            public bool CanAccept(GridInventory inv, ItemInstance item, out string reason)
            {
                reason = null;
                if (item.Data.type != blocked) return true;
                if (item.Owner != null && owner.Contains(item.Owner)) return true;
                reason = reasonText;
                return false;
            }
        }
    }
}