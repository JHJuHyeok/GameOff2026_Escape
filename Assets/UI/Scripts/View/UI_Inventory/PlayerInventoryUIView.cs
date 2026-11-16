using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.Inventory
{
    [DisallowMultipleComponent]
    public sealed class PlayerInventoryUIView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private InventoryView _inventoryView;
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private GameObject _weightGroup;
        [SerializeField] private TMP_Text _weightValueText;
        [SerializeField] private InventoryUIAnimation _animation;

        private bool _isAnimating;

        #region Events

        public event Action OnOpened; // 등장 완료 후 발생
        public event Action OnClosed; // 퇴장 및 격자 연결 해제 후 발생

        #endregion

        #region Properties

        public bool IsOpen { get; private set; } // 등장 요청 상태
        public InventoryView GridView => _inventoryView; // 내부 격자 View

        #endregion

        #region Public API

        /// <summary>플레이어 인벤토리 데이터를 연결하고 창을 표시합니다.</summary>
        public bool Show(GridInventory inventory)
        {
            if (!isActiveAndEnabled || inventory == null) return false;
            if (_canvasGroup == null || _inventoryView == null || _animation == null)
            {
                return false;
            }

            if (!IsOpen && !PlayVisibility(true)) return false;
            if (_inventoryView.Inventory != inventory) _inventoryView.Bind(inventory);

            return true;
        }

        /// <summary>퇴장 연출 후 격자 연결을 해제합니다.</summary>
        public bool Close()
        {
            if (!isActiveAndEnabled || !IsOpen) return false;
            return PlayVisibility(false);
        }

        /// <summary>연출을 중단하고 창과 격자를 즉시 정리합니다.</summary>
        public void HideImmediately()
        {
            bool wasVisible = IsOpen || _isAnimating;

            IsOpen = false;
            _isAnimating = false;
            ApplyVisibility();

            if (_inventoryView != null) _inventoryView.Unbind();
            if (wasVisible) OnClosed?.Invoke();
        }

        /// <summary>가방의 표시 이름을 설정합니다.</summary>
        public void SetTitle(string title)
        {
            if (_nameText != null) _nameText.text = title ?? string.Empty;
        }

        /// <summary>가방 배경을 설정합니다. null이면 숨깁니다.</summary>
        public void SetBackground(Sprite sprite)
        {
            if (_background == null) return;

            _background.sprite = sprite;
            _background.enabled = sprite != null;
        }

        /// <summary>무게 문자열을 표시합니다. null 또는 공백이면 숨깁니다.</summary>
        public void SetWeight(string displayValue)
        {
            bool visible = !string.IsNullOrWhiteSpace(displayValue);

            if (_weightValueText != null) _weightValueText.text = displayValue ?? string.Empty;
            if (_weightGroup != null) _weightGroup.SetActive(visible);
        }

        #endregion

        #region Unity Messages

        private void OnEnable()
        {
            ApplyVisibility();
        }

        private void OnDisable()
        {
            HideImmediately();
        }

        #endregion

        #region Presentation

        private bool PlayVisibility(bool visible)
        {
            if (_canvasGroup == null || _animation == null) return false;
            if (!_animation.TryPlay(_canvasGroup,visible,() => CompleteVisibility(visible),HandleAnimationInterrupted))
            {
                return false;
            }

            IsOpen = visible;
            _isAnimating = true;
            ApplyInputState();
            return true;
        }

        private void CompleteVisibility(bool visible)
        {
            IsOpen = visible && isActiveAndEnabled;
            _isAnimating = false;

            if (!IsOpen && _inventoryView != null) _inventoryView.Unbind();
            ApplyInputState();

            if (IsOpen) OnOpened?.Invoke();
            else OnClosed?.Invoke();
        }

        private void HandleAnimationInterrupted()
        {
            IsOpen = IsOpen && isActiveAndEnabled;
            _isAnimating = false;

            ApplyVisibility();
            CompleteVisibility(IsOpen);
        }

        private void ApplyVisibility()
        {
            bool visible = IsOpen && isActiveAndEnabled;

            if (_animation != null)
            {
                _animation.SetVisibleImmediately(_canvasGroup, visible);
            }
            else if (_canvasGroup != null)
            {
                if (visible) _canvasGroup.alpha = 1f;
                else _canvasGroup.alpha = 0f;
            }

            ApplyInputState();
        }

        private void ApplyInputState()
        {
            if (_canvasGroup == null) return;

            bool enabled = isActiveAndEnabled && IsOpen && !_isAnimating;

            _canvasGroup.interactable = enabled;
            _canvasGroup.blocksRaycasts = enabled;
        }

        #endregion
    }
}