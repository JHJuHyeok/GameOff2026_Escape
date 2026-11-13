using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SH.Project.UI.QuickSlotHUD
{
    [DisallowMultipleComponent]
    public sealed class QuickSlotView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _itemIcon;
        [SerializeField] private TMP_Text _durabilityText;
        [SerializeField] private QuickSlotAnimation _slotAnimation;

        private bool _hasItem;

        #region Events

        /// <summary>슬롯을 왼쪽 포인터 버튼으로 클릭했을 때 발생합니다.</summary>
        internal event Action<QuickSlotView> OnClicked;

        #endregion

        #region View API

        /// <summary>아이템 아이콘과 선택적인 내구도 숫자를 표시합니다.</summary>
        /// <param name="icon">표시할 아이템 아이콘입니다.</param>
        /// <param name="durability">표시할 내구도입니다. null이면 숫자를 숨깁니다.</param>
        internal void SetItem(Sprite icon, int? durability)
        {
            _hasItem = true;

            if (_itemIcon != null)
            {
                _itemIcon.sprite = icon;
                _itemIcon.enabled = icon != null;
            }

            SetDurability(durability);
        }

        /// <summary>아이템과 내구도 표시를 지웁니다.</summary>
        /// <returns>표시하던 아이템을 지웠으면 true입니다.</returns>
        internal bool ClearItem()
        {
            bool hadItem = _hasItem;
            _hasItem = false;

            if (_itemIcon != null)
            {
                _itemIcon.sprite = null;
                _itemIcon.enabled = false;
            }

            ApplyDurability(null);
            return hadItem;
        }

        /// <summary>현재 아이템의 내구도 표시를 갱신합니다.</summary>
        /// <param name="durability">표시할 내구도입니다. null이면 숫자를 숨깁니다.</param>
        internal void SetDurability(int? durability)
        {
            if (_hasItem)
                ApplyDurability(durability);
        }

        /// <summary>슬롯 상태에 맞춰 입력과 강조 표시를 갱신합니다.</summary>
        /// <param name="expanded">퀵 슬롯 HUD가 열려 있으면 true입니다.</param>
        /// <param name="selected">이 슬롯이 선택되어 있으면 true입니다.</param>
        /// <param name="animate">알파를 트윈으로 바꾸려면 true입니다.</param>
        internal void SetPresentation(bool expanded, bool selected, bool animate)
        {
            SetPointerEnabled(expanded);

            if (_slotAnimation != null)
                _slotAnimation.SetPresentation(_canvasGroup, expanded || selected, !expanded && selected, animate);
        }

        /// <summary>슬롯의 포인터 입력을 허용하거나 차단합니다.</summary>
        /// <param name="enabled">포인터 입력을 허용하려면 true입니다.</param>
        internal void SetPointerEnabled(bool enabled)
        {
            if (_canvasGroup == null)
                return;

            _canvasGroup.interactable = enabled;
            _canvasGroup.blocksRaycasts = enabled;
        }

        #endregion

        #region Unity Messages

        private void Reset()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Awake()
        {
            if (!_hasItem)
                ClearItem();
        }

        #endregion

        #region Pointer Callbacks

        /// <summary>슬롯이 포인터 입력을 받을 수 있을 때 왼쪽 클릭을 상위 HUD에 전달합니다.</summary>
        /// <param name="eventData">클릭 이벤트 데이터입니다.</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || _canvasGroup == null ||
                !_canvasGroup.interactable || !_canvasGroup.blocksRaycasts)
                return;

            OnClicked?.Invoke(this);
        }

        #endregion

        #region Display

        private void ApplyDurability(int? durability)
        {
            if (_durabilityText == null)
                return;

            _durabilityText.text = durability?.ToString() ?? string.Empty;
            _durabilityText.enabled = durability.HasValue;
        }

        #endregion
    }
}
