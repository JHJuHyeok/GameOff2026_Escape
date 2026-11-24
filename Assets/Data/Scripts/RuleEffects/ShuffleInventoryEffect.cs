using System.Collections.Generic;
using UnityEngine;

namespace Ju.RuleSystem
{
    /// 인벤토리 안에 아이템 배치 무작위 재배열
    [CreateAssetMenu(menuName = "Effects/Shuffle Inventory")]
    public class ShuffleInventoryEffect : RuleEffect
    {
        private struct Snap 
        {
            public ItemInstance item;
            public Vector2Int origin;
            public int rotation;
        }

        public override void Apply(RuleContext ctx, RuleEvent trigger)
        {
            foreach (var grid in ctx.Player.Inventories.Inventories) Shuffle(grid);
        }

        private static void Shuffle(GridInventory grid)
        {
            var list = new List<ItemInstance>(grid.Items);
            if (list.Count < 2) return;

            var snaps = new List<Snap>(list.Count);
            foreach (var it in list) snaps.Add(new Snap { item = it, origin = it.Origin, rotation = it.Rotation });

            foreach (var it in list) grid.Remove(it);

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }

            var placed = new List<ItemInstance>(list.Count);
            bool ok = true;
            foreach (var it in list)
            {
                if (grid.TryAutoPlace(it)) placed.Add(it);
                else { ok = false; break; }
            }
            if (ok) return;

            // 실패 시 원상복구
            foreach (var p in placed) grid.Remove(p);
            foreach (var s in snaps)
            {
                s.item.Rotation = s.rotation;
                if (!grid.TryPlace(s.item, s.origin))
                    Debug.LogError($"[ShuffleInventoryEffect] 복구 실패: {s.item.Data.ItemName}");
            }
        }
    }
}