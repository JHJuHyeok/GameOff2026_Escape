using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.Inventory
{
    [DisallowMultipleComponent]
    public sealed class ContainerInventoryUIView : MonoBehaviour
    {
        [Header("View")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private InventoryView _inventoryView;
        [SerializeField] private InventoryUIAnimation _animation;
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _nameText;

        [Header("Layout")]
        [SerializeField] private RectTransform _frame;
        [SerializeField] private RectTransform _header;
        [SerializeField] private RectTransform _gridArea;
        [SerializeField] private Vector2 _padding = new Vector2(12f, 12f);
        [SerializeField, Min(0f)] private float _headerHeight = 32f;
        [SerializeField, Min(0f)] private float _headerSpacing = 8f;
        [SerializeField, Min(0f)] private float _minimumWidth = 128f;

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

        /// <summary>
        /// 컨테이너 데이터를 연결하고 이름·배경·크기를 갱신한 뒤 표시합니다.
        /// 배경이 null이면 배경 이미지를 숨깁니다.
        /// </summary>
        public bool Show(GridInventory inventory,string title,Sprite background = null)
        {
            if (!isActiveAndEnabled || inventory == null)
                return false;

            if (_canvasGroup == null ||_inventoryView == null ||_animation == null ||_frame == null ||
                _header == null ||_gridArea == null ||_nameText == null)
            {
                return false;
            }

            if (!IsOpen && !PlayVisibility(true)) return false;
            if (_inventoryView.Inventory != inventory) _inventoryView.Bind(inventory);

            _nameText.text = title ?? string.Empty;

            if (_background != null)
            {
                _background.sprite = background;
                _background.enabled = background != null;
            }

            RefreshLayout(inventory);
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

        #region Layout

        private void RefreshLayout(GridInventory inventory)
        {
            float cellSize = _inventoryView.CellSize;
            Vector2 gridSize = new Vector2(inventory.Width * cellSize,inventory.Height * cellSize);
            float paddingX = Mathf.Max(0f, _padding.x);
            float paddingY = Mathf.Max(0f, _padding.y);
            float headerHeight = Mathf.Max(0f, _headerHeight);
            float spacing = Mathf.Max(0f, _headerSpacing);
            float width = Mathf.Max(_minimumWidth,gridSize.x + paddingX * 2f);
            float height = paddingY * 2f + headerHeight + spacing + gridSize.y;

            _frame.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            _frame.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            SetTopLeftRect(_header,new Vector2(paddingX, -paddingY),new Vector2(width - paddingX * 2f, headerHeight));
            SetTopLeftRect(_gridArea,new Vector2((width - gridSize.x) * 0.5f,-(paddingY + headerHeight + spacing)),gridSize);
            _inventoryView.Rect.anchoredPosition = Vector2.zero;
        }

        private static void SetTopLeftRect(RectTransform target,Vector2 position,Vector2 size)
        {
            target.anchorMin = Vector2.up;
            target.anchorMax = Vector2.up;
            target.pivot = Vector2.up;
            target.anchoredPosition = position;
            target.sizeDelta = size;
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