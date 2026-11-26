using System;
using UnityEngine;

namespace Ju.Player
{
    /// 장착 슬롯 관리. 슬롯 N개 중 활성 슬롯은 최대 1개.
    /// 장착된 아이템은 인벤토리에 그대로 남아 있다고 가정한다.
    public class PlayerEquipment
    {
        public const int SlotCount = 4;     // 숫자키 1~4

        private struct Slot
        {
            public ItemInstance Instance;
            public IExpandableItem Expandable;
            public bool IsEmpty => Instance == null;
        }

        private readonly Slot[] slots = new Slot[SlotCount];
        private readonly PlayerController player;
        private readonly PlayerItemUser user;
        private GridInventory inventory;
        private int activeIndex = -1;

        public event Action<int> SlotChanged;       // 슬롯 내용 변경 (UI 갱신용)
        public event Action<int> ActiveChanged;     // 활성 슬롯 변경 (-1 = 없음)

        public int ActiveIndex => activeIndex;
        public ItemInstance ActiveInstance => activeIndex < 0 ? null : slots[activeIndex].Instance;
        public ItemData ActiveItem => activeIndex < 0 ? null : slots[activeIndex].Instance.Data;
        public IExpandableItem ActiveExpandable => activeIndex < 0 ? null : slots[activeIndex].Expandable;

        public PlayerEquipment(PlayerController player, PlayerItemUser user)
        {
            this.player = player;
            this.user = user;
        }

        // ---------------- 인벤토리 이벤트 연결 ----------------
        public void Bind(GridInventory inv)
        {
            Unbind();
            inventory = inv;
            if (inventory != null) inventory.ItemRemoved += HandleItemRemoved;
        }

        public void Unbind()
        {
            if (inventory != null) inventory.ItemRemoved -= HandleItemRemoved;
            inventory = null;
        }

        // 버림 / 다른 인벤토리로 이전 / 내구도 소진으로 사라진 아이템 슬롯 해제
        private void HandleItemRemoved(ItemInstance item)
        {
            for (int i = 0; i < SlotCount; i++)
                if (slots[i].Instance == item) ClearSlot(i);
        }

        public ItemData GetItem(int index) => InRange(index) ? slots[index].Instance.Data : null;

        /// 지금 손에 들고 있고, '상호작용 시 사용' 타입인가? (Interactable 조건 판정용)
        public bool IsEquipped(ItemData data)
        {
            var ex = ActiveExpandable;
            return data != null && ActiveItem == data
                && ex != null && ex.UseTiming == ExpandableUseTiming.OnInteract;
        }

        // ---------------- 슬롯 등록 ----------------
        /// data가 IExpandableItem이 아니면 false
        public bool SetSlot(int index, ItemInstance item)
        {
            if (!InRange(index) || item == null || item.Data is not IExpandableItem ex ||
                inventory == null || item.Owner != inventory) return false;


            ClearSlot(index);
            slots[index] = new Slot { Instance = item, Expandable = ex };
            SlotChanged?.Invoke(index);
            return true;
        }

        public void ClearSlot(int index)
        {
            if (!InRange(index) || slots[index].IsEmpty) return;
            if (index == activeIndex) Deactivate();
            slots[index] = default;
            SlotChanged?.Invoke(index);
        }

        // ---------------- 전환 ----------------
        /// 숫자키 입력용. 비어 있으면 무시, 이미 활성이면 해제(토글).
        public void Toggle(int index)
        {
            if (!InRange(index) || slots[index].IsEmpty) return;

            if (index == activeIndex) { Deactivate(); return; }

            Deactivate();
            activeIndex = index;
            slots[index].Expandable.OnEquip(MakeContext(slots[index]));
            ActiveChanged?.Invoke(activeIndex);
        }

        public void Deactivate()
        {
            if (activeIndex < 0) return;
            var s = slots[activeIndex];
            activeIndex = -1;
            s.Expandable.OnUnequip(MakeContext(s));
            ActiveChanged?.Invoke(-1);
        }

        //---------------- 내구도 ----------------
        public void OnInteractionUsed(ItemData data)
        {
            if (!IsEquipped(data)) return;
            ConsumeDurability(slots[activeIndex].Instance);
        }

        private void ConsumeDurability(ItemInstance instance)
        {
            if (instance == null) return;
            if (!DecreaseDurability(instance)) return;

            instance.Owner?.Remove(instance);
        }

        /// 내구도가 0 이하가 되면 true
        private static bool DecreaseDurability(ItemInstance item)
        {
            item.Durability--;
            return item.Durability <= 0;
        }

        private ItemUseContext MakeContext(Slot s)
        {
            var instance = s.Instance;
            return new ItemUseContext()
            {
                Item = instance,
                Player = player,
                User = user,
                OnEffectEnded = () => ConsumeDurability(instance)
            };
        }

        private static bool InRange(int i) => i >= 0 && i < SlotCount;
    }
}