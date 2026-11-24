using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ju.RuleSystem
{
    /// 여러 효과를 순서대로 실행. delay는 직전 효과 이후 대기 시간.
    /// 예: 암전 -> 3초 -> 스트레스 + 20 -> 2초 -> 전등 On
    [CreateAssetMenu(menuName = "Effects/Sequence")]
    public class SequenceEffect : RuleEffect
    {
        [Serializable]
        public struct Step
        {
            public RuleEffect effect;
            [Min(0f)] public float delay;
        }

        public List<Step> steps = new();

        public override void Apply(RuleContext ctx, RuleEvent trigger)
            => ctx.Runner.StartCoroutine(Run(ctx, trigger));

        private IEnumerator Run(RuleContext ctx, RuleEvent trigger)
        {
            foreach (var s in steps)
            {
                if (s.delay > 0f) yield return new WaitForSeconds(s.delay);
                if (s.effect != null) s.effect.Apply(ctx, trigger);
            }
        }
    }
}