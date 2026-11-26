using UnityEngine;

namespace Ju.Player
{
    public enum ExpandableUseTiming
    {
        OnEquip,        // 슬롯 전환 즉시 효과 발동
        OnInteract      // 장착하고 Interactable과 상호작용할 때 사용
    }

    public interface IExpandableItem
    {
        ExpandableUseTiming useTiming { get; }

        // 슬롯 전환으로 활성화될 때
        void OnEquip(ItemUseContext ctx);

        // 다른 슬롯으로 전환 / 해제 / 슬롯에서 제거될 때
        void OnUnequip(ItemUseContext ctx);
    }
}
