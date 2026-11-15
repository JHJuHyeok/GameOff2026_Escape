using System;
using System.Collections.Generic;

/// <summary>플레이어가 가진 분할된 인벤토리 컨테이너</summary>
public class PlayerInventory
{
    private readonly List<GridInventory> inventories = new();
    private readonly IPlacementRule weightRule;
    private float totalWeight;
    private float maxWeight;

    public IReadOnlyList<GridInventory> Inventories => inventories;

    public event Action<GridInventory> InventoryAdded;
    public event Action<GridInventory> InventoryRemoved;
    public event Action WeightChanged;

    // ---------------- 무게 ----------------
    public bool HardWeightLimit { get; set; }
    public float TotalWeight => totalWeight;
    public float MaxWeight
    {
        get => maxWeight;
        set { maxWeight = Math.Max(0f, value); WeightChanged?.Invoke(); }
    }
    public float WeightRatio => maxWeight <= 0f ? 0f : totalWeight / maxWeight;
    public bool IsOverweight => totalWeight > maxWeight;

    public PlayerInventory(float maxWeight = 100f, bool hardWeightLimit = false)
    {
        this.maxWeight = maxWeight;
        HardWeightLimit = hardWeightLimit;
        weightRule = new WeightRule(this);
    }

    // ---------------- 관리 ----------------
    public bool Add(GridInventory inv)
    {
        if (inv == null || inventories.Contains(inv) || GetById(inv.Id) != null) return false;
        inventories.Add(inv);
        inv.AddRule(weightRule);
        inv.ItemAdded += OnItemChanged;
        inv.ItemRemoved += OnItemChanged;
        RecalculateWeight();
        InventoryAdded?.Invoke(inv);
        return true;
    }

    public bool Remove(GridInventory inv)
    {
        if (!inventories.Remove(inv)) return false;
        inv.RemoveRule(weightRule);
        inv.ItemAdded -= OnItemChanged;
        inv.ItemRemoved -= OnItemChanged;
        RecalculateWeight();
        InventoryRemoved?.Invoke(inv);
        return true;
    }

    public GridInventory GetById(string id)
    {
        for (int i = 0; i < inventories.Count; i++)
            if (inventories[i].Id == id) return inventories[i];
        return null;
    }

    public bool Contains(GridInventory inv) => inv != null && inventories.Contains(inv);

    public bool TryAutoPlace(ItemInstance item, out GridInventory placedIn)
    {
        placedIn = null;
        if (item == null || item.Owner != null) return false;
        for (int i = 0; i < inventories.Count; i++)
            if (inventories[i].TryAutoPlace(item)) { placedIn = inventories[i]; return true; }
        return false;
    }

    // ---------------- 내부 ----------------
    private void OnItemChanged(ItemInstance _) => RecalculateWeight();

    private void RecalculateWeight()
    {
        float sum = 0f;
        for (int i = 0; i < inventories.Count; i++)
        {
            var items = inventories[i].Items;
            for (int j = 0; j < items.Count; j++) sum += items[j].Data.Weight;
        }
        if (Math.Abs(sum - totalWeight) < 0.0001f) return;
        totalWeight = sum;
        WeightChanged?.Invoke();
    }

    // 무게 규칙: 플레이어 인벤토리에서만 사용하므로 여기에 배치
    private sealed class WeightRule : IPlacementRule
    {
        private readonly PlayerInventory owner;
        public WeightRule(PlayerInventory owner) => this.owner = owner;

        public bool CanAccept(GridInventory inv, ItemInstance item, out string reason)
        {
            reason = null;
            if (!owner.HardWeightLimit) return true;
            if (owner.Contains(item.Owner)) return true;
            if (owner.totalWeight + item.Data.Weight <= owner.maxWeight) return true;
            reason = "무게 한도 초과";
            return false;
        }
    }
}
