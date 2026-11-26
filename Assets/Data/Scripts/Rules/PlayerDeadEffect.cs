using UnityEngine;

namespace Ju.RuleSystem
{
    [CreateAssetMenu(menuName = "Effects/Player Dead")]
    public class PlayerDeadEffect : RuleEffect
    {
        public override void Apply(RuleContext ctx, RuleEvent trigger)
        {
            ctx.Stats.Killed();
        }
    }

}