using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// 드래그의 판단을 맡는 중재자
/// </summary>
public class InventoryDragController : MonoBehaviour
{
    [SerializeField] private RectTransform dragLayer;

    private readonly List<InventoryView> views = new();

    private ItemView dragView;
    private ItemInstance item;
    private InventoryView source, hover;
    private int dragRotation;
    private Vector2Int grabOffset, targetOrigin;
    private Vector2 screenPos;
    private Camera cam;
    private bool hoverAccepts, targetValid;
    private string rejectReason;

    public bool IsDragging => dragView != null;
    public string RejectReason => hoverAccepts ? null : rejectReason;

    // ---------- View 등록 ----------
    public void Register(InventoryView v) { if (!views.Contains(v)) views.Add(v); }

    public void UnRegister(InventoryView v)
    {
        views.Remove(v);
        if (source == v) CancleDrag();
        else if (hover == v) hover = null;
    }

    // ---------- 드래그 흐름 ----------
    public void BeginDrag(ItemView view, PointerEventData e)
    {
        if (IsDragging) return;
        dragView = view;
        item = view.Item;
        source = view.Owner;
        dragRotation = item.Rotation;
        Store(e);

        grabOffset = source.LocalToCell(source.ScreenToLocal(screenPos, cam)) - item.Origin;
        view.BeginDragVisual(dragLayer);
        UpdateDrag();
    }

    public void Drag(PointerEventData e)
    {
        if (!IsDragging) return;
        Store(e);
        UpdateDrag();
    }

    public void EndDrag(PointerEventData e)
    {
        if (!IsDragging) return;

        var view = dragView; var dragged = item; var target = hover;
        int rotation = dragRotation; var origin = targetOrigin; bool valid = targetValid;

        // 연출 / 상태 정리
        if (target != null) target.HidePreview();
        view.EndDragVisual();
        ResetState();

        // 같은 인벤토리면 이동, 다르면 이전
        bool ok = valid && target != null
            && dragged.Owner.TryTransfer(dragged, target.Inventory, origin, rotation);

        // 실패 시 원위치
        if (!ok) view.Refresh();
    }

    private void Update()
    {
        if (!IsDragging || item.Data.RotationCount <= 1 || !RotatePressed()) return;

        grabOffset = item.Data.GetShape(dragRotation).RotateCell(grabOffset);
        dragRotation = item.Data.NextRotation(dragRotation);
        dragView.ShowRotation(dragRotation);
        UpdateDrag();
    }

    private void UpdateDrag()
    {
        var shape = item.Data.GetShape(dragRotation);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer, screenPos, cam, out var layerLocal);
        float c = source.CellSize;
        var grabCenter = new Vector2((grabOffset.x + 0.5f) * c, -(grabOffset.y + 0.5f) * c);
        dragView.Rect.anchoredPosition = layerLocal - grabCenter;

        var target = FindViewAt();
        if (target != hover)
        {
            if (hover != null) hover.HidePreview();
            hover = target;
            hoverAccepts = hover != null && hover.Inventory.CanAccept(item, out rejectReason);
        }
        if (hover == null) { targetValid = false; return; }

        var local = hover.ScreenToLocal(screenPos, cam);
        targetOrigin = hover.LocalToCell(local) - grabOffset;
        targetValid = hoverAccepts && hover.Inventory.CanPlace(shape, targetOrigin, item);
        hover.ShowPreview(shape, targetOrigin, targetValid);
    }

    private InventoryView FindViewAt()
    {
        for (int i = views.Count - 1; i >= 0; i--)
            if (views[i].ContainsScreenPoint(screenPos, cam)) return views[i];
        return null;
    }

    private void CancleDrag()
    {
        if (!IsDragging) return;
        var view = dragView;
        if (hover != null) hover.HidePreview();
        view.EndDragVisual();
        view.Refresh();
        ResetState();
    }

    private void ResetState() { dragView = null; item = null; source = null; hover = null; hoverAccepts = false; }
    private void Store(PointerEventData e) { screenPos = e.position; cam = e.pressEventCamera; }

    private static bool RotatePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.R);
#endif
    }
}
