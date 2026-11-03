using System;
using System.Collections.Generic;

/// <summary>플레이어가 가진 분할된 인벤토리 컨테이너</summary>
public class PlayerInventory
{
    private readonly List<GridInventory> inventories = new();
    public IReadOnlyList<GridInventory> Inventories => inventories;

    public event Action<GridInventory> InventoryAdded;
    public event Action<GridInventory> InventoryRemoved;

    public void Add(GridInventory inv)
    {
        if (inv == null || inventories.Contains(inv)) return;
        inventories.Add(inv);
        InventoryAdded?.Invoke(inv);
    }

    public bool Remove(GridInventory inv)
    {
        if (!inventories.Remove(inv)) return false;
        InventoryRemoved.Invoke(inv);
        return true;
    }

    public GridInventory GetById(string id)
    {
        for (int i = 0; i < inventories.Count; i++)
            if (inventories[i].Id == id) return inventories[i];
        return null;
    }

    public bool TryAutoPlace(ItemInstance item, out GridInventory placedIn)
    {
        for (int i = 0; i < inventories.Count; i++)
            if (inventories[i].TryAutoPlace(item)) { placedIn = inventories[i]; return true; }
        placedIn = null;
        return false;
    }
}
