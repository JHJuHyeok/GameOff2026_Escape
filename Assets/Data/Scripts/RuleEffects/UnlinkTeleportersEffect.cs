using System.Collections.Generic;
using UnityEngine;
using Ju.Environment;

namespace Ju.RuleSystem
{
    /// 문/계단의 연결을 끊어 사용 불가능하게 만듦
    [CreateAssetMenu(menuName = "Effects/Unlink Teleporters")]
    public class UnlinkTeleportersEffect : RuleEffect
    {
        public enum Scope { CurrentRoom, TriggerTarget }

        public Scope scope = Scope.CurrentRoom;
        [Tooltip("CurrentRoom 범위에서 대상 역할. 비우면 전부")]
        public List<TeleportRole> roles = new();

        public override void Apply(RuleContext ctx, RuleEvent trigger)
        {
            if (scope == Scope.TriggerTarget)
            {
                if (trigger.Target is TeleportObject single) single.Unlink();
                return;
            }

            var room = ctx.Player.CurrentSpace;
            if (room == null) return;
            foreach (var t in room.FindTeleporters())
                if (roles.Count == 0 || roles.Contains(t.Role)) t.Unlink();
        }
    }

}