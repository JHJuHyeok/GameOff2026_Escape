using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MainSystem;

[RequireComponent(typeof(CanvasGroup))]
public class ItemView : MonoBehaviour, IPoolable
{
    [SerializeField] private Image tileTemplate;        // 프리팹 안의 비활성 템플릿
    [SerializeField] private RectTransform iconRect;
    [SerializeField] private Image iconImage;

    public ItemInstance Item { get; private set; }
    public InventoryView Owner { get; private set; }
    public RectTransform Rect => rect != null ? rect : rect = (RectTransform)transform;

    private InventoryView owner;
    private RectTransform rect;
    private CanvasGroup group;
    private CanvasGroup Group => group != null ? group : group = GetComponent<CanvasGroup>();
    private readonly List<RectTransform> tiles = new();
    private int shownRotation;

    //private ItemShape CurrentShape => Item.Data.GetShape(dragging ? dragRotation : Item.Rotation);

    // ---------- 풀 생명주기 ----------
    public void OnSpawn() { Group.alpha = 1f; Group.blocksRaycasts = true; }
    public void OnDespawn() { Item = null; Owner = null; }


    public void Bind(InventoryView owner, ItemInstance item)
    {
        Owner = owner;
        Item = item;
        iconImage.sprite = item.Data.Icon;
        EnsureTiles(item.Data.GetShape(0).Cells.Count);
        Refresh();
    }

    /// 인벤토리에 확정된 상태(위치/회전)로 UI를 맞추기
    public void Refresh()
    {
        ApplyShape(Item.Rotation);
        rect.anchoredPosition = Owner.CellToLocal(Item.Origin);
    }

    /// 드래그 중 임시 회전 표시
    public void ShowRotation(int rotation) => ApplyShape(rotation);

    // ---------- 드래그 연출 담당 ----------
    public void BeginDragVisual(RectTransform dragLayer)
    {
        Rect.SetParent(dragLayer, false);
        Rect.SetAsLastSibling();
        Group.alpha = 0.8f;
        Group.blocksRaycasts = false;
    }

    public void EndDragVisual()
    {
        Rect.SetParent(Owner.Rect, false);
        Group.alpha = 1.0f;
        Group.blocksRaycasts = true;
    }

    // ---------- 실제 점유 칸만 클릭 대상 ----------
    public bool ContainsScreenPoint(Vector2 screenPoint, Camera cam)
    {
        if (Item == null || Owner == null) return false;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect, screenPoint, cam, out var local);
        int x = Mathf.FloorToInt(local.x / Owner.CellSize);
        int y = Mathf.FloorToInt(-local.y / Owner.CellSize);
        return Item.Data.GetShape(shownRotation).Contains(x, y);
    }

    // ---------- 내부 ----------
    private void EnsureTiles(int count)
    {
        while (tiles.Count < count)
        {
            var t = Instantiate(tileTemplate, Rect);
            t.gameObject.SetActive(true);
            tiles.Add(t.rectTransform);
        }
        for (int i = 0; i < tiles.Count; i++) tiles[i].gameObject.SetActive(i < count);
        iconRect.SetAsLastSibling();
    }

    private void ApplyShape(int rotation)
    {
        shownRotation = rotation;
        var data = Item.Data;
        var shape = data.GetShape(rotation);
        var baseShape = data.GetShape(0);
        float c = Owner.CellSize;

        Rect.sizeDelta = new Vector2(shape.Width * c, shape.Height * c);             // 점유 영역

        for (int i = 0; i < tiles.Count && i < shape.Cells.Count; i++)
        {
            tiles[i].anchoredPosition = Owner.CellToLocal(shape.Cells[i]);
            tiles[i].sizeDelta = Vector2.one * c;
        }

        // 아이콘은 "회전 전" 바운딩 박스 크기로 만든 뒤 회전 → "회전 후" 박스와 일치
        iconRect.sizeDelta = new Vector2(baseShape.Width * c, baseShape.Height * c);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.localRotation = Quaternion.Euler(0, 0, -90f * rotation);
    }
}
