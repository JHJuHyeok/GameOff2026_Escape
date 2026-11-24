using UnityEngine;

namespace Ju.RuleSystem
{
    /// 스탯 즉시 증감
    [CreateAssetMenu(menuName = "Effects/Modify Stat")]
    public class ModifyStatEffect : RuleEffect
    {
        public StatType stat = StatType.Stress;
        public float amount = 20f;

        public override void Apply(RuleContext ctx, RuleEvent trigger)
            => ctx.Stats.Modify(stat, amount);
    }
}
