using UnityEngine;

/// <summary>
/// 아이템을 인벤토리에 "새로 들일 수 있는가" 정의
/// (이미 들어있는 아이템의 내부 이동엔 호출되지 않음)
/// </summary>
public interface IPlacementRule
{
    bool CanAccept(GridInventory inventory, ItemInstance item, out string reason);
}

public class MaxWeightRule : IPlacementRule
{
    public float MaxWeight { get; set; }
    public MaxWeightRule(float maxWeight) { MaxWeight = maxWeight; }

    public float CurrentWeight(GridInventory inventory)
    {
        float sum = 0f;
        var items = inventory.Items;
        for (int i = 0; i < items.Count; i++) sum += items[i].Data.Weight;
        return sum;
    }

    public bool CanAccept(GridInventory inventory, ItemInstance item, out string reason)
    {
        float after = CurrentWeight(inventory) + item.Data.Weight;
        if (after > MaxWeight + 0.0001f)
        {
            reason = $"무게 초과 ({after:0.#} / {MaxWeight:0.#}";
            return false;
        }
        reason = null;
        return true;
    }
}
