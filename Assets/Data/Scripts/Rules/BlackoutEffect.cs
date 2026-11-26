using System.Collections;
using UnityEngine;
using Ju.Visual;

namespace Ju.RuleSystem
{
    /// 화면 암전. 여러 암전이 겹치면 모두 끝나야 해제된다.
    [CreateAssetMenu(menuName = "Ju/RuleSystem/Effects/Blackout")]
    public class BlackoutEffect : RuleEffect
    {
        [Tooltip("0이면 ClearBlackoutEffect가 실행될 때까지 유지")]
        public float duration = 15f;
        public float fadeIn = 0.5f;
        public float fadeOut = 1.5f;
        [Tooltip("암전 동안 켜 둘 월드 플래그. 비우면 사용 안 함 (지속 상태 규칙의 조건용)")]
        public string worldFlag = "Blackout";

        [Header("Flicker")]
        public bool flicker = true;
        public float flickerDuration = 1.5f;
        public float flickerInterval = 0.08f;

        public override void Apply(RuleContext ctx, RuleEvent trigger)
        {
            var o = BlackoutOverlay.Instance;
            if (o == null) { Debug.LogWarning("[BlackoutEffect] BlackoutOverlay가 씬에 없습니다."); return; }

            o.BeginBlackout(fadeIn);
            if (!string.IsNullOrEmpty(worldFlag)) ctx.World.Set(worldFlag, true);
            if (flicker) o.Flicker(flickerDuration, flickerInterval);
            if (duration > 0f) ctx.Runner.StartCoroutine(End(ctx));
        }

        private IEnumerator End(RuleContext ctx)
        {
            yield return new WaitForSeconds(duration);
            var o = BlackoutOverlay.Instance;
            if (o == null) yield break;
            o.EndBlackout(fadeOut);
            if (!o.IsBlackout && !string.IsNullOrEmpty(worldFlag)) ctx.World.Set(worldFlag, false);
        }
    }
}
