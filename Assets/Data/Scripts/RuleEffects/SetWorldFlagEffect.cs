using System;
using System.Collections;
using UnityEngine;

namespace Ju.RuleSystem
{
    [CreateAssetMenu(menuName = "Effects/Set World Flag")]
    public class SetWorldFlagEffect : RuleEffect
    {
        public string flag = "L_Off";
        public bool value = true;
        [Tooltip("0이면 영구, 0보다 크면 반대 값으로 복귀")]
        public float duration = 0f;

        private WaitForSeconds waitFor => new WaitForSeconds(duration);

        public override void Apply(RuleContext ctx, RuleEvent trigger)
        {
            ctx.World.Set(flag, value);
            if (duration > 0f) ctx.Runner.StartCoroutine(Revert(ctx));
        }

        private IEnumerator Revert(RuleContext ctx)
        {
            yield return waitFor;
            ctx.World.Set(flag, !value);
        }
    }
}