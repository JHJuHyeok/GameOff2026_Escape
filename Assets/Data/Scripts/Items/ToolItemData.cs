using UnityEngine;
using Ju.RuleSystem;

namespace Ju.Player
{
    [CreateAssetMenu(menuName = "Data/Tool Item Data", fileName = "NewTool")]
    public class ToolItemData : ItemData, IExpandableItem, IInteractionUseHook
    {
        [Min(0f)] public float staminaCost = 0f;
        public bool consumeDurability = true;

        public ExpandableUseTiming UseTiming => ExpandableUseTiming.OnInteract;
        public bool ConsumeDurability => consumeDurability;

        private void Reset() => type = ItemType.Expendable;
        protected override void OnValidate()
        {
            base.OnValidate();
        }

        // 연출 없으면 그냥 냅두기
        public void OnEquip(ItemUseContext ctx) { }
        public void OnUnequip(ItemUseContext ctx) { }

        public void OnInteractionUsed(ItemUseContext ctx)
        {
            if (staminaCost <= 0f) return;
            var stats = ctx.Player != null ? ctx.Player.Stats : null;
            if (stats != null) stats.Modify(StatType.Stamina, -staminaCost);
        }
    }

}