using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ItemView))]
public class ItemInputHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, ICanvasRaycastFilter
{
    private ItemView view;
    private InventoryDragController controller;

    private void Awake() => GetComponent<ItemView>();
    public void Init(InventoryDragController controller) => this.controller = controller;

    public void OnBeginDrag(PointerEventData e) { if (controller != null) controller.BeginDrag(view, e); }
    public void OnDrag(PointerEventData e) { if (controller != null) controller.Drag(e); }
    public void OnEndDrag(PointerEventData e) { if (controller != null) controller.EndDrag(e); }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera cam)
        => view != null && view.ContainsScreenPoint(screenPoint, cam);
}
