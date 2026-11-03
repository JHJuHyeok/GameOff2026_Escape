using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Item Data", fileName = "NewItem")]
public class ItemData : ScriptableObject
{
    public string ItemName;
    public Sprite Icon;
    [Min(0f)] public float Weight = 1f;
    public ShapeMask Shape = new();
    public bool CanRotate = true;

    [NonSerialized] private ItemShape[] rotations;  // 0˚, 90˚, 180˚, 270˚
    [NonSerialized] private int period;             // 모양이 반복되는 회전 주기

    private void OnEnable() => rotations = null;

    private void OnValidate()
    {
        Shape ??= new ShapeMask();
        Shape.Sanitize();
        rotations = null;

        var s = Shape.ToShape();
        if (s == null) Debug.LogWarning($"[{name}] 채워진 칸이 없습니다. 1x1로 처리됩니다.", this);
        else if (!s.IsConnected()) Debug.LogWarning($"[{name}] 모양이 서로 떨어진 조각을 포함합니다.", this);
    }

    // 회전 변경점에 따른 형태 수 / 정사각형 = 1, 직사각형 = 2, 그 외 = 4
    public int RotationCount { get { Build(); return CanRotate ? period : 1; } }

    public ItemShape GetShape(int rotation)
    {
        Build();
        return rotations[((rotation % 4) + 4) % 4];
    }

    public int NextRotation(int current) => (current + 1) % RotationCount;

    private void Build()
    {
        if (rotations != null) return;

        var baseShape = Shape.ToShape() ?? new ItemShape(new[] { Vector2Int.zero });
        rotations = new ItemShape[4];
        rotations[0] = baseShape;
        for (int i = 1; i < 4; i++) rotations[i] = rotations[i - 1].RotateCW();

        // 순환 주기는 4의 약수
        if (rotations[1].SameAs(rotations[0])) period = 1;
        else if (rotations[2].SameAs(rotations[0])) period = 2;
        else period = 4;
    }
}
