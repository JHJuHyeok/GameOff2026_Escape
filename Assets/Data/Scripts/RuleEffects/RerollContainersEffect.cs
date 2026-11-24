using UnityEngine;

namespace Ju.RuleSystem
{
    /// 플레이어가 있는 방의 컨테이너 다시 뽑고 재구성
    [CreateAssetMenu(menuName = "Effects/Reroll Containers")]
    public class RerollContainersEffect : RuleEffect
    {
        public override void Apply(RuleContext ctx, RuleEvent trigger)
        {
            var room = ctx.Player.CurrentSpace;
            if (room == null) return;
            room.ApplyRandomContainers(new System.Random());
        }
    }
}
