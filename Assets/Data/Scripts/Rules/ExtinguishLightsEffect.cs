using System.Collections;
using UnityEngine;
using Ju.Visual;

namespace Ju.RuleSystem
{
    /// 모든 광원을 끈다. suppressDuration 동안은 새 광원도 켤 수 없다. (손전등이 먹통이 되는 연출)
    [CreateAssetMenu(menuName = "Effects/Extinguish Lights")]
    public class ExtinguishLightsEffect : RuleEffect
    {
        [Tooltip("0이면 지금 켜진 광원만 끔")]
        public float suppressDuration = 0f;

        public override void Apply(RuleContext ctx, RuleEvent trigger)
        {
            var o = BlackoutOverlay.Instance;
            if (o == null) return;
            o.ReleaseAllLights();
            if (suppressDuration > 0f)
            {
                o.BeginSuppressLights();
                ctx.Runner.StartCoroutine(Resume());
            }
        }

        private IEnumerator Resume()
        {
            yield return new WaitForSeconds(suppressDuration);
            var o = BlackoutOverlay.Instance;
            if (o != null) o.EndSuppressLights();
        }
    }
}
