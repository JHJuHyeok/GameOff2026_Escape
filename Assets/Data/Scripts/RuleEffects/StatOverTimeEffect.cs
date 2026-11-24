using System.Collections;
using UnityEngine;

namespace Ju.RuleSystem
{
    /// 지정 시간 동안 스탯에 초당 변화량 추가 부여
    [CreateAssetMenu(menuName = "Effects/Stat Over Time")]
    public class StatOverTimeEffect : RuleEffect
    {
        public StatType stat = StatType.Hunger;
        public float perSecond = -0.5f;
        [Min(0.1f)] public float duration = 20f;

        public override void Apply(RuleContext ctx, RuleEvent trigger)
            => ctx.Runner.StartCoroutine(Run(ctx));

        private IEnumerator Run(RuleContext ctx)
        {
            float t = 0f;
            while (t < duration)
            {
                float dt = Time.deltaTime;
                t += dt;
                ctx.Stats.Modify(stat, perSecond * dt);
                yield return null;
            }
        }
    }

}