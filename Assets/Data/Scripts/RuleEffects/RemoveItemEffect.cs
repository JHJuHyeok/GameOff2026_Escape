using System.Collections.Generic;
using UnityEngine;

namespace Ju.RuleSystem
{
    /// 플레이어의 인벤토리에서 아이템 제거(랜덤, 특정 아이템, 타입별)
    [CreateAssetMenu(menuName = "Effects/Remove Item")]
    public class RemoveItemEffect : RuleEffect
    {
        public enum Mode { Random, SpecificItem, ByType }

        public Mode mode = Mode.Random;
        [Min(1)] public int count = 1;
        [Tooltip("Mode = SpecificItem")] public ItemData specificItem;
        [Tooltip("Mode = ByType")] public ItemType itemType;

        public override void Apply(RuleContext ctx, RuleEvent trigger)
        {
            var pool = new List<ItemInstance>();
            foreach (var grid in ctx.Player.Inventories.Inventories)
                foreach (var item in grid.Items)
                    if (Match(item)) pool.Add(item);

            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                int idx = UnityEngine.Random.Range(0, pool.Count);
                var item = pool[idx];
                pool.RemoveAt(idx);
                item.Owner?.Remove(item);
            }
        }

        private bool Match(ItemInstance item) => mode switch
        {
            Mode.SpecificItem => item.Data == specificItem,
            Mode.ByType => item.Data.type == itemType,
            _ => true
        };
    }
}
