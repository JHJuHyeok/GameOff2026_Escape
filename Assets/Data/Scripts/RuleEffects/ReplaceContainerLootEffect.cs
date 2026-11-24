using System.Collections.Generic;
using UnityEngine;
using Ju.Environment;

namespace Ju.RuleSystem
{
    /// 컨테이너 내용물 비우거나 교체
    [CreateAssetMenu(menuName = "Effects/Replace Container Loot")]
    public class ReplaceContainerLootEffect : RuleEffect
    {
        public enum Scope { TriggerTarget, CurrentRoom }

        public Scope scope = Scope.TriggerTarget;
        public bool clearExisting = true;
        [Tooltip("비워 두면 비우기만 수행")]
        public List<LootEntry> replaceWith = new();

        public override void Apply(RuleContext ctx, RuleEvent trigger)
        {
            var targets = new List<ContainerObject>();
            if (scope == Scope.TriggerTarget)
            {
                if (trigger.Target is ContainerObject c) targets.Add(c);
            }
            else if (ctx.Player.CurrentSpace != null)
            {
                targets.AddRange(ctx.Player.CurrentSpace.FindContainers());
            }

            foreach (var c in targets)
            {
                var grid = c.Container;
                if (clearExisting)
                    foreach (var it in new List<ItemInstance>(grid.Items)) grid.Remove(it);

                foreach (var entry in replaceWith)
                {
                    if (entry.item == null || Random.value > entry.chance) continue;
                    if (!grid.TryAutoPlace(new ItemInstance(entry.item)))
                        Debug.LogWarning($"[ReplaceContainerLootEffect] '{entry.item.ItemName}' 공간 부족");
                }
            }
        }
    }
}
