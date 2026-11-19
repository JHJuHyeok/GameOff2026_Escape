using System;
using UnityEngine;
using Ju.Player;
using SH.Project.UI.Inventory;

/// 플레이어 인벤토리 뷰, 컨테이너 인벤토리 뷰를 각각 독립적으로 여닫는 View.
public class InventoryPanel : MonoBehaviour
{
    public static InventoryPanel Instance { get; private set; }
    public static bool IsPlayerOpen     => Instance != null && Instance.view.IsPlayerOpen;
    public static bool IsContainerOpen  => Instance != null && Instance.view.IsContainerOpen;
    public static bool IsAnyOpen        => IsPlayerOpen || IsContainerOpen;

    [SerializeField] private InventoryUIView view;

    private bool playerPinned;          // 사용자가 직접 열어둔 상태
    private Action onContainerClosed;

    private void Awake() => Instance = this;
    private void OnEnable() { view.OnContainerClosed += HandleContainerClosed; }
    private void OnDisable() { view.OnContainerClosed -= HandleContainerClosed; }

    private void OnDestroy() { if (Instance == this) Instance = null; }

    // ---------------- 인벤토리 ----------------
    /// 아무것도 안 열려 있으면 플레이어 인벤토리 열기, 하나라도 열려 있으면 전부 닫기
    public void ToggleInventory()
    {
        if (IsAnyOpen) CloseAll(); else view.ShowPlayer(PlayerController.Instance.Inventory);
    }

    // ---------------- 컨테이너 ----------------
    public bool OpenContainer(GridInventory container, Action onClosed = null, string title = null, Sprite bg = null)
    {
        var pc = PlayerController.Instance;
        if (pc == null) return false;

        if (view.IsContainerOpen) FinishContainerCallback();
        if (!view.IsPlayerOpen) playerPinned = false;

        if (!view.ShowContainer(pc.Inventory, container, title, bg)) return false;
        onContainerClosed = onClosed;
        return true;
    }

    public void CloseContainer() => view.CloseContainer();
    public void CloseAll() { playerPinned = false; view.CloseAll(); }

    private void HandleContainerClosed()
    {
        FinishContainerCallback();
        if (!playerPinned && view.IsPlayerOpen) view.CloseAll();
    }

    private void FinishContainerCallback()
    {
        var cb = onContainerClosed;
        onContainerClosed = null;
        cb?.Invoke();
    }
}
