using System;
using System.Collections.Generic;
using UnityEngine;

public class GridInventory
{
    public string Id { get; }       // 세이브/UI 식별용
    public int Width { get; }
    public int Height { get; }

    private readonly ItemInstance[,] cells;     // 각 칸을 점유한 아이템
    private readonly List<ItemInstance> items = new();
    public IReadOnlyList<ItemInstance> Items => items;

    private readonly List<IPlacementRule> rules = new();
    public IReadOnlyList<IPlacementRule> Rules => rules;

    // ----------- 이벤트 -----------
    public event Action<ItemInstance> ItemAdded;
    public event Action<ItemInstance> ItemRemoved;
    public event Action<ItemInstance> ItemMoved;

    public GridInventory(int width, int height, string id = null)
    {
        Width = width;
        Height = height;
        Id = id ?? "inventory";
        cells = new ItemInstance[width, height];
    }

    public ItemInstance GetItemAt(int x, int y)
        => (x < 0 || y < 0 || x >= Width || y >= Height) ? null : cells[x, y];

    /// <summary>(x, y)에서 wxh 영역에 놓을 수 있는지 판단.</summary>
    /// <param name="ignore">"인스턴스 자신"을 무시하기 위한 인자</param>
    public bool CanPlace(ItemShape shape, Vector2Int origin, ItemInstance ignore = null)
    {
        foreach (var c in shape.Cells)
        {
            int x = origin.x + c.x, y = origin.y + c.y;
            if (x < 0 || y < 0 || x >= Width || y >= Height) return false;

            var ocuupant = cells[x, y];
            if (ocuupant != null && ocuupant != ignore) return false;
        }
        return true;
    }

    /// <summary>지정 위치에 배치</summary>
    public bool TryPlace(ItemInstance item, Vector2Int origin)
    {
        if (items.Contains(item)) return false;
        if (!CanAccept(item, out _)) return false;
        if (!CanPlace(item.Shape, origin)) return false;
        Commit(item, origin);
        return true;
    }

    /// <summary>빈 공간 자동 탐색 후 배치</summary>
    public bool TryAutoPlace(ItemInstance item)
    {
        if (items.Contains(item)) return false;
        if (item.Owner != null || !CanAccept(item, out _)) return false;

        int original = item.Rotation;
        int count = item.Data.RotationCount;
        for (int i = 0; i < count; i++)
        {
            item.Rotation = (original + i) % count;
            if (TryScan(item)) return true;
        }
        item.Rotation = original;       // 실패 시 원복
        return false;
    }

    /// <summary>인벤토리 내부 이동 + 회전 변경 처리</summary>
    public bool TryMove(ItemInstance item, Vector2Int newOrigin, int rotation)
    {
        if (item.Owner != this) return false;
        if (rotation < 0 || rotation >= item.Data.RotationCount) return false;

        if (!CanPlace(item.Data.GetShape(rotation), newOrigin, item)) return false;

        Clear(item);
        item.Rotation = rotation;
        item.Origin = newOrigin;
        Fill(item);
        ItemMoved?.Invoke(item);
        return true;
    }

    /// <summary>인벤토리 간 이전</summary>
    public bool TryTransfer(ItemInstance item, GridInventory target, Vector2Int origin, int rotation)
    {
        if (target == this) return TryMove(item, origin, rotation);
        if (item.Owner != this) return false;
        if (rotation < 0 || rotation >= item.Data.RotationCount) return false;

        // 조건 검사
        if (!target.CanAccept(item, out _)) return false;
        if (!target.CanPlace(item.Data.GetShape(rotation), origin)) return false;

        Remove(item);                   // 이 인벤토리: ItemRemoved
        item.Rotation = rotation;
        target.Commit(item, origin);    // 대상 인벤토리: ItemAdded
        return true;
    }

    public bool Remove(ItemInstance item)
    {
        if (!items.Remove(item)) return false;
        items.Remove(item);
        Clear(item);
        item.Owner = null;
        ItemRemoved?.Invoke(item);
        return true;
    }

    // ---------- 수납 규칙 ----------
    public GridInventory AddRule(IPlacementRule rule)
    {
        if (rule != null) rules.Add(rule);
        return this;
    }

    /// <summary>수납 규칙 검사. 이미 이 인벤토리 소속이면 이동이니 통과</summary>
    public bool CanAccept(ItemInstance item, out string reason)
    {
        reason = null;
        if (item.Owner == this) return true;
        for (int i = 0; i < rules.Count; i++)
            if (!rules[i].CanAccept(this, item, out reason)) return false;
        return true;
    }

    // ---------- 내부 ----------
    private bool TryScan(ItemInstance item)
    {
        var shape = item.Shape;
        for (int y = 0; y <= Height - shape.Height; y++)
        {
            for (int x = 0; x <= Width - shape.Width; x++)
            {
                var origin = new Vector2Int(x, y);
                if (CanPlace(shape, origin)) { Commit(item, origin); return true; }
            }
        }
        return false;
    }

    private void Commit(ItemInstance item, Vector2Int origin)
    {
        item.Origin = origin;
        item.Owner = this;
        Fill(item);
        items.Add(item);
        ItemAdded?.Invoke(item);
    }

    private void Fill(ItemInstance item)
    {
        foreach (var c in item.Shape.Cells)
            cells[item.Origin.x + c.x, item.Origin.y + c.y] = item;
    }

    private void Clear(ItemInstance item)
    {
        foreach (var c in item.Shape.Cells)
            cells[item.Origin.x + c.x, item.Origin.y + c.y] = null;
    }
}
