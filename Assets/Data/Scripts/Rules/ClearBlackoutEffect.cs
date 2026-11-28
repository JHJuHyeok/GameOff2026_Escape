using UnityEngine;
using Ju.Visual;

namespace Ju.RuleSystem
{
    /// 진행 중인 모든 암전을 즉시 해제한다. (규칙 준수 보상, 전력 복구 등)
    [CreateAssetMenu(menuName = "Effects/Clear Blackout")]
    public class ClearBlackoutEffect : RuleEffect
    {
        public float fadeOut = 1f;
        public string worldFlag = "Blackout";

        public override void Apply(RuleContext ctx, RuleEvent trigger)
        {
            var o = BlackoutOverlay.Instance;
            if (o == null) return;
            o.ClearBlackout(fadeOut);
            if (!string.IsNullOrEmpty(worldFlag)) ctx.World.Set(worldFlag, false);
        }
    }
}
