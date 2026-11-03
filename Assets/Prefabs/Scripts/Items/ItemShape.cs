using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>아이템의 형태 <- 아이템이 차지하는 셀의 집합</summary>
public class ItemShape
{
    private static readonly Vector2Int[] Dirs =
        { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    private readonly Vector2Int[] cells;
    private readonly HashSet<Vector2Int> lookup;

    public IReadOnlyList<Vector2Int> Cells => cells;
    public int Width { get; }
    public int Height { get; }

    public ItemShape(IEnumerable<Vector2Int> source)
    {
        var raw = new HashSet<Vector2Int>(source);      // 중복 셀 자동 제거
        if (raw.Count == 0) throw new ArgumentException("셀이 최소 1개 필요합니다.");

        int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
        foreach (var c in raw)
        {
            minX = Mathf.Min(minX, c.x); maxX = Mathf.Max(maxX, c.x);
            minY = Mathf.Min(minY, c.y); maxY = Mathf.Max(maxY, c.y);
        }

        var list = new List<Vector2Int>(raw.Count);
        foreach (var c in raw) list.Add(new Vector2Int(c.x - minX, c.y - minY));
        list.Sort((a, b) => a.y != b.y ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));

        cells = list.ToArray();
        lookup = new HashSet<Vector2Int>(cells);
        Width = maxX - minX + 1;
        Height = maxY - minY + 1;
    }

    public bool Contains(int x, int y) => lookup.Contains(new Vector2Int(x, y));

    /// <summary>시계 방향으로 90도 회전한 모양</summary>
    public ItemShape RotateCW()
    {
        var rotated = new List<Vector2Int>(cells.Length);
        foreach (var c in cells) rotated.Add(RotateCell(c));
        return new ItemShape(rotated);
    }

    /// <summary>모양 내부의 한 셀 좌표를 90도 회전 후 좌표</summary>
    public Vector2Int RotateCell(Vector2Int c) => new Vector2Int(Height - 1 - c.y, c.x);

    /// <summary>두 모양이 같은지 검사</summary>
    public bool SameAs(ItemShape other)
    {
        if (Width != other.Width || Height != other.Height || cells.Length != other.cells.Length) return false;
        for (int i = 0; i < cells.Length; i++)
            if (cells[i] != other.cells[i]) return false;
        return true;
    }

    /// <summary>상하좌우 연결성 검사 (데이터 검증용)</summary>
    public bool IsConnected()
    {
        var visited = new HashSet<Vector2Int> { cells[0] };
        var stack = new Stack<Vector2Int>();
        stack.Push(cells[0]);
        while (stack.Count > 0)
        {
            var cur = stack.Pop();
            foreach (var d in Dirs)
            {
                var n = cur + d;
                if (lookup.Contains(n) && visited.Add(n)) stack.Push(n);
            }
        }
        return visited.Count == cells.Length;
    }
}
