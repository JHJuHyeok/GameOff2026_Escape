using UnityEngine;

public class ItemInstance
{
    public ItemData Data { get; }

    // 좌상단 기준 셀 좌표
    public Vector2Int Origin { get; internal set; }
    public int Rotation { get; internal set; }
    public GridInventory Owner { get; internal set; }

    // 회전 반영한 실제 점유 크기
    public ItemShape Shape => Data.GetShape(Rotation);
    public int Width => Shape.Width;
    public int Height => Shape.Height;

    public ItemInstance(ItemData data, int rotation = 0)
    { 
        Data = data;
        int n = data.RotationCount;
        Rotation = ((rotation % n) + n) % n;
    }
}
