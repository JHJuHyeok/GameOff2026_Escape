using UnityEditor;
using UnityEngine;

/// ShapeMask 필드를 클릭식 그리드로 보여주는 인스펙터
[CustomPropertyDrawer(typeof(ShapeMask))]
public class ShapeMaskDrawer : PropertyDrawer
{
    private const float CellPx = 22f;
    private const float Line = 20f;

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        int h = Mathf.Max(1, prop.FindPropertyRelative("Height").intValue);
        return Line * 3 + h * CellPx + 6f;
    }

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        EditorGUI.BeginProperty(pos, label, prop);

        var wProp = prop.FindPropertyRelative("Width");
        var hProp = prop.FindPropertyRelative("Height");
        var fProp = prop.FindPropertyRelative("Filled");

        int w = Mathf.Max(1, wProp.intValue), h = Mathf.Max(1, hProp.intValue);
        if (fProp.arraySize != w * h) Resize(fProp, w, h, w, h);   // 길이 불일치 보정

        var r = new Rect(pos.x, pos.y, pos.width, Line);
        EditorGUI.LabelField(r, "Shape (클릭해서 칸 채우기)", EditorStyles.boldLabel);

        // 가로/세로 크기
        r.y += Line;
        float half = r.width * 0.5f;
        int newW = Mathf.Clamp(EditorGUI.IntField(new Rect(r.x, r.y, half - 4, Line), "Width", w), 1, 16);
        int newH = Mathf.Clamp(EditorGUI.IntField(new Rect(r.x + half, r.y, half, Line), "Height", h), 1, 16);
        if (newW != w || newH != h)
        {
            Resize(fProp, w, h, newW, newH);
            wProp.intValue = w = newW;
            hProp.intValue = h = newH;
        }

        // 일괄 버튼
        r.y += Line;
        if (GUI.Button(new Rect(r.x, r.y, half - 4, Line - 2), "전체 채우기")) SetAll(fProp, true);
        if (GUI.Button(new Rect(r.x + half, r.y, half, Line - 2), "전체 비우기")) SetAll(fProp, false);

        // 토글 그리드
        r.y += Line + 4f;
        var prevColor = GUI.backgroundColor;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                var el = fProp.GetArrayElementAtIndex(y * w + x);
                var cell = new Rect(r.x + x * CellPx, r.y + y * CellPx, CellPx - 1, CellPx - 1);
                GUI.backgroundColor = el.boolValue ? new Color(0.4f, 0.85f, 0.4f) : new Color(0.35f, 0.35f, 0.35f);
                bool next = GUI.Toggle(cell, el.boolValue, GUIContent.none, "Button");
                if (next != el.boolValue) el.boolValue = next;
            }
        GUI.backgroundColor = prevColor;

        EditorGUI.EndProperty();
    }

    // 크기 변경 시 기존 칸의 (x,y) 위치를 보존한다
    private static void Resize(SerializedProperty f, int oldW, int oldH, int newW, int newH)
    {
        var old = new bool[oldW * oldH];
        for (int i = 0; i < old.Length && i < f.arraySize; i++) old[i] = f.GetArrayElementAtIndex(i).boolValue;

        f.arraySize = newW * newH;
        for (int y = 0; y < newH; y++)
            for (int x = 0; x < newW; x++)
            {
                bool v = x < oldW && y < oldH && old[y * oldW + x];
                f.GetArrayElementAtIndex(y * newW + x).boolValue = v;
            }
    }

    private static void SetAll(SerializedProperty f, bool value)
    {
        for (int i = 0; i < f.arraySize; i++) f.GetArrayElementAtIndex(i).boolValue = value;
    }
}