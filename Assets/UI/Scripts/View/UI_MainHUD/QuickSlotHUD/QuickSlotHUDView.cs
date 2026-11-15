using System;
using UnityEngine;

namespace SH.Project.UI.QuickSlotHUD
{
    public enum QuickSlotHUDState
    {
        Collapsed,
        Expanded
    }

    [DisallowMultipleComponent]
    public sealed class QuickSlotHUDView : MonoBehaviour
    {
        private const int SlotCount = 4;

        [SerializeField] private QuickSlotView[] _slots = new QuickSlotView[SlotCount];
        [SerializeField] private QuickSlotHUDAnimation _hudAnimation;

        #region Events

        /// <summary>퀵 슬롯을 열림 상태로 전환하고 위치와 각 슬롯의 표시를 갱신한 뒤 발생합니다.</summary>
        public event Action OnOpened;

        /// <summary>퀵 슬롯을 닫힘 상태로 전환하고 위치와 각 슬롯의 표시를 갱신한 뒤 발생합니다.</summary>
        public event Action OnClosed;

        /// <summary>열림 상태에서 클릭한 슬롯의 번호를 전달합니다.</summary>
        public event Action<int> OnSlotClicked;

        /// <summary>선택 표시가 바뀌면 슬롯 번호를 전달합니다. 선택 해제 시에는 null입니다.</summary>
        public event Action<int?> OnSelectedSlotChanged;

        /// <summary>아이템 표시를 갱신하거나 지운 슬롯의 번호를 전달합니다.</summary>
        public event Action<int> OnSlotItemChanged;

        #endregion

        #region Properties

        /// <summary>퀵 슬롯 HUD의 열림 또는 닫힘 상태입니다.</summary>
        public QuickSlotHUDState State { get; private set; } = QuickSlotHUDState.Collapsed;

        /// <summary>선택된 슬롯의 번호입니다. 선택된 슬롯이 없으면 null입니다.</summary>
        public int? SelectedSlot { get; private set; }

        #endregion

        #region Public API

        /// <summary>퀵 슬롯을 위로 이동하고 네 슬롯을 선명하게 표시합니다.</summary>
        /// <returns>열림 상태로 전환했으면 true입니다.</returns>
        public bool Open()
        {
            return SetState(QuickSlotHUDState.Expanded);
        }

        /// <summary>퀵 슬롯을 아래로 이동하고 현재 슬롯 선택을 표시합니다.</summary>
        /// <returns>닫힘 상태로 전환했으면 true입니다.</returns>
        public bool Close()
        {
            return SetState(QuickSlotHUDState.Collapsed);
        }

        /// <summary>퀵 슬롯의 표시 상태를 설정합니다.</summary>
        /// <param name="state">적용할 열림 또는 닫힘 상태입니다.</param>
        /// <returns>표시 상태가 바뀌었으면 true입니다.</returns>
        public bool SetState(QuickSlotHUDState state)
        {
            if (State == state || (state != QuickSlotHUDState.Expanded && state != QuickSlotHUDState.Collapsed))
                return false;

            State = state;

            if (isActiveAndEnabled)
                RefreshPresentation(animate: true);

            if (state == QuickSlotHUDState.Expanded)
                OnOpened?.Invoke();
            else
                OnClosed?.Invoke();

            return true;
        }

        /// <summary>선택할 슬롯을 설정합니다. 열림 상태에서는 선택값을 저장하고 닫힐 때 강조를 표시합니다.</summary>
        /// <param name="slotNumber">1부터 4까지의 슬롯 번호입니다. null이면 선택을 해제합니다.</param>
        /// <returns>선택값이 바뀌었으면 true입니다.</returns>
        public bool SetSelectedSlot(int? slotNumber)
        {
            if (SelectedSlot == slotNumber || (slotNumber.HasValue && FindSlot(slotNumber.Value) == null))
                return false;

            SelectedSlot = slotNumber;

            if (isActiveAndEnabled)
                RefreshSlots(animate: true);

            OnSelectedSlotChanged?.Invoke(SelectedSlot);
            return true;
        }

        /// <summary>닫힘 상태에서 선택을 전환합니다. 이미 선택된 번호를 다시 받으면 선택을 해제합니다.</summary>
        /// <param name="slotNumber">Controller가 전달한 1부터 4까지의 슬롯 번호입니다.</param>
        /// <returns>선택값이 바뀌었으면 true입니다.</returns>
        public bool ToggleSelection(int slotNumber)
        {
            if (State != QuickSlotHUDState.Collapsed || FindSlot(slotNumber) == null)
                return false;

            return SetSelectedSlot(SelectedSlot == slotNumber ? (int?)null : slotNumber);
        }

        /// <summary>슬롯에 아이템 아이콘과 내구도 숫자를 표시합니다.</summary>
        /// <param name="slotNumber">1부터 4까지의 슬롯 번호입니다.</param>
        /// <param name="icon">표시할 아이템 아이콘입니다.</param>
        /// <param name="durability">표시할 내구도입니다. null이면 숫자를 숨깁니다.</param>
        public void SetSlotItem(int slotNumber, Sprite icon, int? durability = null)
        {
            QuickSlotView slot = FindSlot(slotNumber);

            if (slot == null)
                return;

            slot.SetItem(icon, durability);
            OnSlotItemChanged?.Invoke(slotNumber);
        }

        /// <summary>슬롯에서 아이템 아이콘과 내구도 숫자를 지웁니다.</summary>
        /// <param name="slotNumber">1부터 4까지의 슬롯 번호입니다.</param>
        public void ClearSlot(int slotNumber)
        {
            QuickSlotView slot = FindSlot(slotNumber);

            if (slot == null || !slot.ClearItem())
                return;

            OnSlotItemChanged?.Invoke(slotNumber);
        }

        /// <summary>아이템이 표시된 슬롯의 내구도 숫자를 갱신합니다.</summary>
        /// <param name="slotNumber">1부터 4까지의 슬롯 번호입니다.</param>
        /// <param name="durability">표시할 내구도입니다. null이면 숫자를 숨깁니다.</param>
        public void SetSlotDurability(int slotNumber, int? durability)
        {
            QuickSlotView slot = FindSlot(slotNumber);

            if (slot != null)
                slot.SetDurability(durability);
        }

        #endregion

        #region Unity Messages

        private void OnEnable()
        {
            for (int slotNumber = 1; slotNumber <= SlotCount; slotNumber++)
            {
                QuickSlotView slot = FindSlot(slotNumber);

                if (slot != null)
                    slot.OnClicked += HandleSlotClicked;
            }

            RefreshPresentation(animate: false);
        }

        private void OnDisable()
        {
            for (int slotNumber = 1; slotNumber <= SlotCount; slotNumber++)
            {
                QuickSlotView slot = FindSlot(slotNumber);

                if (slot == null)
                    continue;

                slot.OnClicked -= HandleSlotClicked;
                slot.SetPointerEnabled(false);
            }
        }

        #endregion

        #region Presentation

        private QuickSlotView FindSlot(int slotNumber)
        {
            if (_slots == null || slotNumber < 1 || slotNumber > SlotCount || slotNumber > _slots.Length)
                return null;

            return _slots[slotNumber - 1];
        }

        private void RefreshPresentation(bool animate)
        {
            if (_hudAnimation != null)
                _hudAnimation.SetExpanded(State == QuickSlotHUDState.Expanded, animate);

            RefreshSlots(animate);
        }

        private void RefreshSlots(bool animate)
        {
            bool expanded = State == QuickSlotHUDState.Expanded;

            for (int slotNumber = 1; slotNumber <= SlotCount; slotNumber++)
            {
                QuickSlotView slot = FindSlot(slotNumber);

                if (slot != null)
                    slot.SetPresentation(expanded, SelectedSlot == slotNumber, animate);
            }
        }

        private void HandleSlotClicked(QuickSlotView slot)
        {
            if (State != QuickSlotHUDState.Expanded)
                return;

            int slotNumber = Array.IndexOf(_slots, slot) + 1;

            if (slotNumber >= 1 && slotNumber <= SlotCount)
                OnSlotClicked?.Invoke(slotNumber);
        }

        #endregion

#if UNITY_EDITOR
        #region Context Menu

        [ContextMenu("Quick Slot HUD/Open")]
        private void OpenForTest()
        {
            if (Application.isPlaying)
                Open();
        }

        [ContextMenu("Quick Slot HUD/Close")]
        private void CloseForTest()
        {
            if (Application.isPlaying)
                Close();
        }

        [ContextMenu("Quick Slot HUD/Toggle Slot 1")]
        private void ToggleSlotOneForTest()
        {
            if (Application.isPlaying)
                ToggleSelection(1);
        }

        #endregion
#endif
    }
}
