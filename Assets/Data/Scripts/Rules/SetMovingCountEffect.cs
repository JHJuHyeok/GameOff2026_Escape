using UnityEngine;

namespace Ju.RuleSystem
{
    /// 이동 횟수(movingCount)를 가감하거나 지정값으로 바꾼다. (Set + 0 = 초기화)
    [CreateAssetMenu(menuName = "Effects/Set Moving Count")]
    public class SetMovingCountEffect : RuleEffect
    {
        public enum Mode { Add, Set }

        public Mode mode = Mode.Set;
        public int value = 0;

        public override void Apply(RuleContext ctx, RuleEvent trigger)
        {
            int cur = MovingCountAccess.Get(ctx.Player);
            MovingCountAccess.Set(ctx.Player, mode == Mode.Add ? cur + value : value);
        }
    }
}
