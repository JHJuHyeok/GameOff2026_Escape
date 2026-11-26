using System.Collections;
using UnityEngine;
using Ju.Visual;

namespace Ju.RuleSystem
{
    /// 모든 광원 반지름에 배수를 곱한다. (0.5 = 시야 절반으로 축소, 1.5 = 확대)
    [CreateAssetMenu(menuName = "Ju/RuleSystem/Effects/Scale Light Radius")]
    public class ScaleLightRadiusEffect : RuleEffect
    {
        [Min(0.05f)] public float multiplier = 0.5f;
        [Tooltip("0이면 영구")] public float duration = 15f;

        public override void Apply(RuleContext ctx, RuleEvent trigger)
        {
            var o = BlackoutOverlay.Instance;
            if (o == null) return;
            o.LightRadiusScale *= multiplier;
            if (duration > 0f) ctx.Runner.StartCoroutine(Revert());
        }

        private IEnumerator Revert()
        {
            yield return new WaitForSeconds(duration);
            var o = BlackoutOverlay.Instance;
            if (o != null) o.LightRadiusScale /= multiplier;
        }
    }
}
