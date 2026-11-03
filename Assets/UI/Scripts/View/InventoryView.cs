using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MainSystem;

[RequireComponent(typeof(RectTransform))]
public class InventoryView : MonoBehaviour
{
    [SerializeField] private InventoryViewConfig config;
    [SerializeField] private InventoryDragController dragController;

    private RectTransform rect;
    private RectTransform previewRoot;

    public float CellSize => config.cellSize;
    public RectTransform Rect => rect;
    public GridInventory Inventory { get; private set; }

    private readonly List<Image> activePreview = new();
    private readonly Dictionary<ItemInstance, ItemView> views = new();

    private void Awake()
    {
        rect = (RectTransform)transform;
        // 앵커/피벗 좌상단으로 고정 -> 좌상단 기준 좌표계 쓸 예정
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
    }

    private void OnDestroy() => Unbind();

    // ---------- 바인딩 ----------
    public void Bind(GridInventory inventory)
    {
        Unbind();
        Inventory = inventory;
        config.Prewarm();

        rect.sizeDelta = new Vector2(inventory.Width * CellSize, inventory.Height * CellSize);
        BuildCells();
        BuildPreviewRoot();

        inventory.ItemAdded     += CreateView;
        inventory.ItemMoved     += RefreshView;
        inventory.ItemRemoved   += ReleaseView;

        foreach (var item in inventory.Items) CreateView(item);
        dragController.Register(this);
    }

    private void Unbind()
    {
        if (Inventory == null) return;
        if (dragController != null) dragController.UnRegister(this);
        Inventory.ItemAdded     -= CreateView;
        Inventory.ItemMoved     -= RefreshView;
        Inventory.ItemRemoved   -= ReleaseView;
        Inventory = null;

        HidePreview();
        foreach (var v in views.Values) Release(v.gameObject);
        views.Clear();

        previewRoot = null;
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }

    /// 드래그 컨트롤러 판정용
    public bool ContainsScreenPoint(Vector2 screenPos, Camera cam)
        => RectTransformUtility.RectangleContainsScreenPoint(rect, screenPos, cam);

    // ---------- 좌표 변환 ----------
    public Vector2 CellToLocal(Vector2Int cell) => new Vector2(cell.x * CellSize, -cell.y * CellSize);

    public Vector2Int LocalToCell(Vector2 local)
        => new Vector2Int(Mathf.FloorToInt(local.x / CellSize), Mathf.FloorToInt(-local.y / CellSize));

    public Vector2 ScreenToLocal(Vector2 screenPos, Camera cam)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPos, cam, out var local);
        return local;
    }

    // ---------- 배치 미리보기 ----------
    public void ShowPreview(ItemShape shape, Vector2Int origin, bool valid)
    {
        previewRoot.SetAsLastSibling();
        var color = valid ? config.validColor : config.invalidColor;
        var cells = shape.Cells;

        int used = 0;
        for (int i = 0; i < cells.Count; i++)
        {
            var cell = origin + cells[i];
            if (cell.x < 0 || cell.y < 0 || cell.x >= Inventory.Width || cell.y >= Inventory.Height)
                continue;

            if (used == activePreview.Count)
                activePreview.Add(PoolManager.Instance.Spawn(config.previewCellPrefab, Vector3.zero, Quaternion.identity, previewRoot));

            var img = activePreview[used++];
            var r = img.rectTransform;
            r.anchoredPosition = CellToLocal(cell);
            r.sizeDelta = Vector2.one * CellSize;
            img.color = color;
        }
        for (int i = activePreview.Count - 1; i >= used; i--)
        {
            Release(activePreview[i].gameObject);
            activePreview.RemoveAt(i);
        }
    }

    public void HidePreview()
    {
        for (int i = activePreview.Count - 1; i >= 0; i--) Release(activePreview[i].gameObject);
        activePreview.Clear();
    }

    private Image GetPreviewImage(int index)
    {
        while (activePreview.Count <= index)
        {
            var go = new GameObject("PreviewCell", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(previewRoot, false);
            var r = (RectTransform)go.transform;
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0f, 1f);
            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            activePreview.Add(img);
        }
        return activePreview[index];
    }

    // ---------- 생성 ----------
    private void BuildCells()
    {
        for (int y = 0; y < Inventory.Height; y++)
            for (int x = 0; x < Inventory.Width; x++)
            {
                var go = new GameObject($"Cell_{x}_{y}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);

                var r = (RectTransform)go.transform;
                r.anchorMin = r.anchorMax = r.pivot = new Vector2(0f, 1f);
                r.sizeDelta = Vector2.one * (CellSize - 2f);
                r.anchoredPosition = CellToLocal(new Vector2Int(x, y)) + new Vector2(1f, -1f);

                var img = go.GetComponent<Image>();
                img.color = config.cellColor;
                img.raycastTarget = false;
            }
    }

    private void BuildPreviewRoot()
    {
        var go = new GameObject("Preview", typeof(RectTransform));
        go.transform.SetParent(transform, false);
        previewRoot = (RectTransform)go.transform;
        previewRoot.anchorMin = previewRoot.anchorMax = previewRoot.pivot = new Vector2(0f, 1f);
        previewRoot.anchoredPosition = Vector2.zero;
        previewRoot.sizeDelta = Vector2.zero;
    }

    private void CreateView(ItemInstance item)
    {
        var view = PoolManager.Instance.Spawn(config.itemViewPrefab, Vector3.zero, Quaternion.identity, transform);

        if (view.TryGetComponent(out ItemInputHandler input))
            input.Init(dragController);

        view.Bind(this, item);
        views[item] = view;
    }

    private void ReleaseView(ItemInstance item)
    {
        if (views.Remove(item, out var v)) Release(v.gameObject);
    }

    private void RefreshView(ItemInstance item)
    {
        if (views.TryGetValue(item, out var v)) v.Refresh();
    }

    private static void Release(GameObject go)
    {
        if (PoolManager.Instance != null) PoolManager.Instance.Despawn(go);
    }
}
