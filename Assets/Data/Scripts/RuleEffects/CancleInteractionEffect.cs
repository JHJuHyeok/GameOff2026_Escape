using UnityEngine;

namespace Ju.RuleSystem
{
    /// 진행 중인 상호작용 강제 종료
    [CreateAssetMenu(menuName = "Effects/Cancle Interaction")]
    public class CancleInteractionEffect : RuleEffect
    {
        public bool closeOpenContainer = true;

        public override void Apply(RuleContext ctx, RuleEvent trigger)
        {
            var p = ctx.Player;
            if (p.CurrentInteraction != null) p.CurrentInteraction.CancelInteraction();
            if (closeOpenContainer && InventoryPanel.IsContainerOpen) InventoryPanel.Instance.CloseContainer();
        }
    }
}
