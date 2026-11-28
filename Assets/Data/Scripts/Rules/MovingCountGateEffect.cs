using System.Collections.Generic;
using UnityEngine;

namespace Ju.RuleSystem
{
    /// 이동 횟수가 조건을 만족하면 whenMet, 아니면 otherwise 효과를 실행한다.
    /// - AtLeast: movingCount >= threshold
    /// - EveryN : movingCount가 threshold의 배수 (0 제외)
    /// 주의: resetWhenMet은 효과 실행 '직후' 초기화한다. whenMet 안에 지연(Sequence delay) 뒤 movingCount를 읽는
    ///       효과가 있으면 0을 읽게 되므로, 그런 조합에서는 resetWhenMet 대신 SetMovingCountEffect를 마지막 단계에 넣을 것.
    [CreateAssetMenu(menuName = "Effects/Moving Count Gate")]
    public class MovingCountGateEffect : RuleEffect
    {
        public enum Check { AtLeast, EveryN }

        public Check check = Check.AtLeast;
        [Min(1)] public int threshold = 5;
        public bool resetWhenMet = true;
        public List<RuleEffect> whenMet = new();
        public List<RuleEffect> otherwise = new();

        public override void Apply(RuleContext ctx, RuleEvent trigger)
        {
            int count = MovingCountAccess.Get(ctx.Player);
            bool met = check == Check.AtLeast ? count >= threshold : (count > 0 && count % threshold == 0);

            var list = met ? whenMet : otherwise;
            foreach (var fx in list) if (fx != null) fx.Apply(ctx, trigger);

            if (met && resetWhenMet) MovingCountAccess.Set(ctx.Player, 0);
        }
    }
}
