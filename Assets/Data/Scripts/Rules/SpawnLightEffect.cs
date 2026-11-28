using UnityEngine;
using Ju.Visual;

namespace Ju.RuleSystem
{
    /// 원형 마스크 광원을 만든다. 암전 중에만 눈에 보인다.
    /// (예: 전등 스위치를 올리면 그 주변만 밝아짐 / 규칙 준수 보상으로 플레이어 주변 광원)
    [CreateAssetMenu(menuName = "Effects/Spawn Light")]
    public class SpawnLightEffect : RuleEffect
    {
        public enum Anchor { Player, TriggerTarget }

        [Tooltip("TriggerTarget: 사건의 대상(오브젝트/방 등) 위치. 대상이 없으면 플레이어로 대체")]
        public Anchor anchor = Anchor.Player;
        [Min(0.1f)] public float radius = 4f;
        [Range(0f, 1f)] public float softness = 0.35f;
        [Tooltip("0이면 무기한 (ExtinguishLightsEffect로 끄기)")]
        public float duration = 20f;
        public float fadeIn = 0.3f;
        public float fadeOut = 1.5f;

        public override void Apply(RuleContext ctx, RuleEvent trigger)
        {
            var o = BlackoutOverlay.Instance;
            if (o == null) return;

            Transform t = null;
            if (anchor == Anchor.TriggerTarget && trigger.Target is Component c && c != null) t = c.transform;
            if (t == null) t = ctx.Player.transform;

            o.AddLight(t, radius, softness, duration > 0f ? duration : -1f, fadeIn, fadeOut);
        }
    }
}
