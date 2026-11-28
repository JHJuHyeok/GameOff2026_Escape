using UnityEngine;

namespace Ju.RuleSystem
{
    /// 이동 횟수에 비례해 스탯을 증감한다.
    /// 변화량 = amountPerCount × max(0, movingCount - freeCount), 절댓값 상한 maxAbsTotal(0이면 무제한)
    [CreateAssetMenu(menuName = "Effects/Modify Stat By Moving Count")]
    public class ModifyStatByMovingCountEffect : RuleEffect
    {
        public StatType stat = StatType.Stress;
        public float amountPerCount = 3f;
        [Min(0)] public int freeCount = 0;
        [Min(0f)] public float maxAbsTotal = 30f;
        [Tooltip("적용 후 이동 횟수를 0으로 초기화")]
        public bool resetAfter = false;

        public override void Apply(RuleContext ctx, RuleEvent trigger)
        {
            int effective = Mathf.Max(0, MovingCountAccess.Get(ctx.Player) - freeCount);
            float total = amountPerCount * effective;
            if (maxAbsTotal > 0f) total = Mathf.Clamp(total, -maxAbsTotal, maxAbsTotal);
            if (total != 0f) ctx.Stats.Modify(stat, total);
            if (resetAfter) MovingCountAccess.Set(ctx.Player, 0);
        }
    }
}
