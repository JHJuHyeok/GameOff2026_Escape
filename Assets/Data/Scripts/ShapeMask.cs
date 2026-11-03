using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>아이템이 지니는 모양 데이터</summary>
[Serializable]
public class ShapeMask
{
    [Range(1, 16)] public int Width = 1;
    [Range(1, 16)] public int Height = 1;
    public bool[] Filled = { true };

    public bool IsFilled(int x, int y)
    {
        int i = y * Width + x;
        return Filled != null && i >= 0 && i < Filled.Length && Filled[i];
    }

    public ItemShape ToShape()
    {
        var list = new List<Vector2Int>();
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                if (IsFilled(x, y)) list.Add(new Vector2Int(x, y));
        return list.Count > 0 ? new ItemShape(list) : null;
    }

    public void Sanitize()
    {
        Width = Mathf.Max(1, Width);
        Height = Mathf.Max(1, Height);
        if (Filled == null || Filled.Length != Width * Height)
            Array.Resize(ref Filled, Width * Height);
    }
}
