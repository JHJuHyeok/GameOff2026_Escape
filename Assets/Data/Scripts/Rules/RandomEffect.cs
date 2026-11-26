using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ju.RuleSystem
{
    // 가중치에 따라 랜덤 효과 실행
    [CreateAssetMenu(menuName = "Effects/Random Effect")]
    public class RandomEffect : RuleEffect
    {
        [Serializable]
        public struct Entry
        {
            public RuleEffect effect;
            [Min(0f)] public float weight;
        }

        public List<Entry> entries = new();

        public override void Apply(RuleContext ctx, RuleEvent trigger)
        {
            float total = 0f;
            foreach (var e in entries) if (e.effect != null) total += e.weight;
            if (total <= 0f) return;

            float roll = UnityEngine.Random.value * total;
            foreach (var e in entries)
            {
                if (e.effect == null) return;
                roll -= e.weight;
                if (roll <= 0f) { e.effect.Apply(ctx, trigger); return; }
            }
        }
    }
}
