using System;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.ItemContextMenu
{
    [Flags]
    public enum ItemMenuAction
    {
        None = 0,
        Use = 1,
        Equip = 2,
        Discard = 4
    }

    [DisallowMultipleComponent]
    public sealed class ItemContextMenuView : MonoBehaviour
    {
        [Header("View")]
        [SerializeField] private RectTransform _root;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _useButton;
        [SerializeField] private Button _equipButton;
        [SerializeField] private Button _discardButton;

        [Header("Placement")]
        [SerializeField] private Vector2 _offset = new (8f, 0f);
        [SerializeField, Min(0f)] private float _edgePadding = 12f;

        [Header("UI 미리보기")]
        [SerializeField] private RectTransform _previewTarget;

        private RectTransform _parentRect;
        private Canvas _canvas;
        private ItemMenuAction _visibleActions;

        #region Events

        /// <summary>
        /// 버튼을 클릭하면 메뉴를 닫고 선택한 행동을 전달합니다.
        /// 실제 사용·장착·버리기는 구독하는 외부에서 처리합니다.
        /// </summary>
        public event Action<ItemMenuAction> OnActionRequested;

        #endregion

        #region Properties

        /// <summary>메뉴가 표시 중인지 나타냅니다.</summary>
        public bool IsVisible { get; private set; }

        #endregion

        #region Public API

        /// <summary>
        /// 대상 UI의 오른쪽 중앙을 기준으로 메뉴를 표시합니다.
        /// actions에는 표시할 행동을 조합해서 전달합니다.
        /// </summary>
        public bool Show(RectTransform target,ItemMenuAction actions)
        {
            if (target == null)
            {
                Hide();
                return false;
            }

            Canvas targetCanvas = target.GetComponentInParent<Canvas>();
            Camera targetCamera = GetCanvasCamera(targetCanvas);
            Vector3 worldPoint = target.TransformPoint(new Vector3(target.rect.xMax,target.rect.center.y,0f));
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(targetCamera, worldPoint);
            return ShowAtScreenPoint(screenPoint, actions);
        }

        /// <summary>
        /// 화면 좌표에 오프셋을 더해 메뉴를 표시합니다.
        /// 표시할 행동이 없으면 메뉴를 숨깁니다.
        /// </summary>
        public bool ShowAtScreenPoint(Vector2 screenPoint,ItemMenuAction actions)
        {
            if (!isActiveAndEnabled) return false;
            if (!PrepareReferences())
            {
                Hide();
                Debug.LogWarning("메뉴의 Root, Canvas Group, 버튼과 부모 Canvas를 확인해주세요.",this); return false;
            }

            actions &= ItemMenuAction.Use | ItemMenuAction.Equip | ItemMenuAction.Discard;

            if (actions == ItemMenuAction.None)
            {
                Hide();
                return false;
            }

            SetButtons(actions);

            // 활성화된 버튼 수에 맞춰 메뉴 높이를 갱신합니다.
            LayoutRebuilder.ForceRebuildLayoutImmediate(_root);

            if (!PlaceAtScreenPoint(screenPoint))
            {
                Hide();
                return false;
            }

            _visibleActions = actions;
            SetVisibility(true);
            return true;
        }

        /// <summary>메뉴를 숨기고 입력을 받지 않도록 설정합니다.</summary>
        public void Hide()
        {
            _visibleActions = ItemMenuAction.None;
            SetVisibility(false);
        }

        #endregion

        #region Unity Messages

        private void OnEnable()
        {
            Hide();
            if (_useButton != null) _useButton.onClick.AddListener(HandleUseClicked);
            if (_equipButton != null) _equipButton.onClick.AddListener(HandleEquipClicked);
            if (_discardButton != null) _discardButton.onClick.AddListener(HandleDiscardClicked);
        }

        private void OnDisable()
        {
            if (_useButton != null) _useButton.onClick.RemoveListener(HandleUseClicked);
            if (_equipButton != null) _equipButton.onClick.RemoveListener(HandleEquipClicked);
            if (_discardButton != null) _discardButton.onClick.RemoveListener(HandleDiscardClicked);
            Hide();
        }

        #endregion

        #region Presentation

        private bool PrepareReferences()
        {
            if (_root == null || _canvasGroup == null || _useButton == null || _equipButton == null || _discardButton == null)
            {
                return false;
            }

            _parentRect = _root.parent as RectTransform;
            _canvas = _root.GetComponentInParent<Canvas>();

            return _parentRect != null && _canvas != null;
        }

        private void SetButtons(ItemMenuAction actions)
        {
            _useButton.gameObject.SetActive((actions & ItemMenuAction.Use) != ItemMenuAction.None);
            _equipButton.gameObject.SetActive((actions & ItemMenuAction.Equip) != ItemMenuAction.None);
            _discardButton.gameObject.SetActive((actions & ItemMenuAction.Discard) != ItemMenuAction.None);
        }

        private void SetVisibility(bool visible)
        {
            IsVisible = visible;

            if (_canvasGroup == null) return;

            if (visible) _canvasGroup.alpha = 1f;
            else _canvasGroup.alpha = 0f;

            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
        }

        private bool PlaceAtScreenPoint(Vector2 screenPoint)
        {
            Camera uiCamera = GetCanvasCamera(_canvas);

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect,screenPoint,uiCamera,out Vector2 localPoint))
            {
                return false;
            }

            localPoint += _offset;
            Rect bounds = _parentRect.rect;
            float minimumX = bounds.xMin + _edgePadding;
            float maximumX = bounds.xMax - _edgePadding - _root.rect.width;
            float minimumY = bounds.yMin + _edgePadding + _root.rect.height;
            float maximumY = bounds.yMax - _edgePadding;

            if (minimumX <= maximumX) localPoint.x = Mathf.Clamp(localPoint.x, minimumX, maximumX);
            else localPoint.x = minimumX;

            if (minimumY <= maximumY) localPoint.y = Mathf.Clamp(localPoint.y, minimumY, maximumY);
            else localPoint.y = maximumY;

            _root.position = _parentRect.TransformPoint(new Vector3(localPoint.x, localPoint.y, 0f));

            return true;
        }

        private static Camera GetCanvasCamera(Canvas canvas)
        {
            if (canvas == null) return null;
            Canvas rootCanvas = canvas.rootCanvas;
            if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
            return rootCanvas.worldCamera;
        }

        #endregion

        #region Button Events

        private void HandleUseClicked()
        {
            RequestAction(ItemMenuAction.Use);
        }

        private void HandleEquipClicked()
        {
            RequestAction(ItemMenuAction.Equip);
        }

        private void HandleDiscardClicked()
        {
            RequestAction(ItemMenuAction.Discard);
        }

        private void RequestAction(ItemMenuAction action)
        {
            if (!IsVisible || (_visibleActions & action) == ItemMenuAction.None)
            {
                return;
            }

            Hide();

#if UNITY_EDITOR
            Debug.Log($"아이템 메뉴 행동 요청: {action}", this);
#endif

            OnActionRequested?.Invoke(action);
        }

        #endregion

#if UNITY_EDITOR
        #region Context Menu

        [ContextMenu("Item Context Menu/Preview Use Item")]
        private void PreviewUseItem()
        {
            ShowPreview(ItemMenuAction.Use | ItemMenuAction.Discard);
        }

        [ContextMenu("Item Context Menu/Preview Equipment")]
        private void PreviewEquipment()
        {
            ShowPreview(ItemMenuAction.Equip | ItemMenuAction.Discard);
        }

        [ContextMenu("Item Context Menu/Preview Luxury")]
        private void PreviewLuxury()
        {
            ShowPreview(ItemMenuAction.Discard);
        }

        [ContextMenu("Item Context Menu/Hide")]
        private void HideForTest()
        {
            if (Application.isPlaying) Hide();
        }

        private void ShowPreview(ItemMenuAction actions)
        {
            if (!Application.isPlaying || !isActiveAndEnabled) return;
            if (_previewTarget != null)
            {
                Show(_previewTarget, actions);
                return;
            }
            ShowAtScreenPoint(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f),actions);
        }

        #endregion
#endif
    }
}