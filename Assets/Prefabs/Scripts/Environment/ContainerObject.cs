using System;
using System.Collections.Generic;
using UnityEngine;
using Ju.Player;

namespace Ju.Environment
{
    [Serializable] public struct LootEntry { public ItemData item;[Range(0, 1)] public float chance; }

    /// 상호작용하면 아이템이 담긴 GridInventory가 열리는 물체
    public class ContainerObject : Interactable
    {
        [SerializeField] private Vector2Int gridSize = new(4, 3);
        [SerializeField] private List<LootEntry> loot = new();

        private GridInventory container;
        private bool interactRaised;

        public bool IsOpen { get; private set; }

        public GridInventory Container => container ??= CreateContainer();

        private void OnValidate() => gridSize = Vector2Int.Max(gridSize, Vector2Int.one);

        private void OnDestroy()
        {
            if (IsOpen && InventoryPanel.Instance != null) InventoryPanel.Instance.CloseContainer();
        }

        protected override void OnInteract(PlayerController player)
        {
            if (IsOpen) return;

            var panel = InventoryPanel.Instance;
            if (panel == null) return;

            IsOpen = true;

            if (!interactRaised)
            {
                interactRaised = true;
                RaiseInteracted();
            }

            panel.OpenContainer(Container, OnPanelClosed);
        }

        private void OnPanelClosed()
        {
            IsOpen = false;

            // 컨테이너의 '사용 완료' = 전부 꺼내서 비었을 때
            if (container != null && container.Items.Count == 0) MarkUsed();
        }

        private GridInventory CreateContainer()
        {
            var inv = new GridInventory(gridSize.x, gridSize.y, $"container_{Id}");

            foreach (var entry in loot)
            {
                if (entry.item == null) continue;
                if (UnityEngine.Random.value > entry.chance) continue;

                var instance = new ItemInstance(entry.item);
                if (!inv.TryAutoPlace(instance))
                    Debug.LogWarning($"[{name}] '{entry.item.ItemName}'이(가) 들어갈 공간이 없습니다.", this);
            }
            return inv;
        }
    }
}
