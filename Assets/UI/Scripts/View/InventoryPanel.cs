using System;
using UnityEngine;
using Ju.Player;

/// 플레이어 인벤토리 뷰, 컨테이너 인벤토리 뷰를 각각 독립적으로 여닫는 View.
public class InventoryPanel : MonoBehaviour
{
    public static InventoryPanel Instance { get; private set; }
    public static bool IsPlayerOpen     => Instance != null && Instance.playerOpen;
    public static bool IsContainerOpen  => Instance != null && Instance.containerOpen;
    public static bool IsAnyOpen        => IsPlayerOpen || IsContainerOpen;

    [SerializeField] private InventoryView playerView;
    [SerializeField] private InventoryView containerView;

    private bool playerOpen, containerOpen;
    private bool playerPinned;          // 사용자가 직접 열어둔 상태
    private Action onContainerClosed;

    private void Awake()
    {
        Instance = this;
        playerView.gameObject.SetActive(false);
        containerView.gameObject.SetActive(false);
    }

    private void OnDestroy() { if (Instance == this) Instance = null; }

    // ---------------- 인벤토리 ----------------
    /// 아무것도 안 열려 있으면 플레이어 인벤토리 열기, 하나라도 열려 있으면 전부 닫기
    public void ToggleInventory()
    {
        if (IsAnyOpen) CloseAll();
        else ShowPlayer(true);
    }

    // ---------------- 컨테이너 ----------------
    public void OpenContainer(GridInventory container, Action onClosed = null)
    {
        // 다른 컨테이너로 전환
        if (containerOpen) ReleaseContainer(true);

        if (!playerOpen) ShowPlayer(false);

        onContainerClosed = onClosed;
        containerView.gameObject.SetActive(true);
        containerView.Bind(container);
        containerOpen = true;
    }

    public void CloseContainer() => ReleaseContainer(false);

    public void CloseAll() 
    {
        ReleaseContainer(true);
        HidePlayer();
    }

    // ---------------- 내부 ----------------
    private void ShowPlayer(bool pinned)
    {
        var inventory = PlayerController.Instance != null ? PlayerController.Instance.Inventory : null;
        if (inventory == null) return;

        playerPinned = pinned;
        playerView.gameObject.SetActive(true);
        playerView.Bind(inventory);
        playerOpen = true;
    }

    private void HidePlayer()
    {
        if (!playerOpen) return;
        playerOpen = false;
        playerPinned = false;
        playerView.Unbind();
        playerView.gameObject.SetActive(false);
    }

    private void ReleaseContainer(bool keepPlayerView)
    {
        if (!containerOpen) return;
        containerOpen = false;

        containerView.Unbind();
        containerView.gameObject.SetActive(false);

        if (!keepPlayerView && !playerPinned) HidePlayer();

        var cb = onContainerClosed;
        onContainerClosed = null;
        cb?.Invoke();
    }
}
