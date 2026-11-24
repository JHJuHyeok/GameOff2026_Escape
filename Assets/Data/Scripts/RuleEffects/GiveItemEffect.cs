using UnityEngine;

namespace Ju.RuleSystem
{
    /// 플레이어 인벤토리에 아이템 강제 추가
    [CreateAssetMenu(menuName = "Effects/Give Item")]
    public class GiveItemEffect : RuleEffect
    {
        public ItemData item;
        [Min(1)] public int count = 1;

        public override void Apply(RuleContext ctx, RuleEvent trigger)
        {
            if (item == null) return;
            for (int i = 0; i < count; i++)
            {
                var inst = new ItemInstance(item);
                if (!ctx.Player.Inventories.TryAutoPlace(inst, out _))
                    Debug.LogWarning($"[GiveItemEffect] '{item.ItemName}'을(를) 넣을 공간 여유가 없습니다.");
            }
        }
    }
}
