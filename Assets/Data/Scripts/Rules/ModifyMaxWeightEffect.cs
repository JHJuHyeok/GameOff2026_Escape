using System.Collections;
using UnityEngine;

namespace Ju.RuleSystem
{
    /// 최대 무게 증감. duration > 0 이면 시간이 지나면서 원복
    [CreateAssetMenu(menuName = "Effects/Modify Max Weight")]
    public class ModifyMaxWeightEffect : RuleEffect
    {
        public float delta = -20f;
        [Tooltip("0이면 영구 적용")] public float duration = 0f;

        public override void Apply(RuleContext ctx, RuleEvent trigger)
        {
            var inv = ctx.Player.Inventories;
            float before = inv.MaxWeight;
            inv.MaxWeight = before + delta;
            float applied = inv.MaxWeight - before;

            if (duration > 0f && applied != 0f)
                ctx.Runner.StartCoroutine(Revert(inv, applied));
        }

        private IEnumerator Revert(PlayerInventory inv, float applied)
        {
            yield return new WaitForSeconds(duration);
            inv.MaxWeight -= applied;
        }
    }
}
