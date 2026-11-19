using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.ItemTooltip
{
    [DisallowMultipleComponent]
    public sealed class ItemTooltipView : MonoBehaviour
    {
        [Header("View")]
        [SerializeField] private RectTransform _root;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _categoryText;
        [SerializeField] private TMP_Text _descriptionText;

        [Header("Layout")]
        [SerializeField] private VerticalLayoutGroup _rootLayout;
        [SerializeField] private HorizontalLayoutGroup _headerLayout;
        [SerializeField, Min(1f)] private float _minimumWidth = 280f;
        [SerializeField, Min(1f)] private float _maximumWidth = 560f;
        [SerializeField, Min(0f)] private float _edgePadding = 12f;

        [Header("UI 미리보기")]
        [SerializeField] private RectTransform _previewTarget;
        [SerializeField] private string _previewName = "bottled water";
        [SerializeField] private string _previewCategory = "FOOD";

        [SerializeField, TextArea(2, 6)]
        private string _previewDescription = "It’s a common bottle of water.\n" + "Thirst increases by <color=#FFD34D>10</color>.";
        private RectTransform _parentRect;
        private Canvas _canvas;

        #region Properties

        /// <summary>툴팁이 표시 중인지 나타냅니다.</summary>
        public bool IsVisible { get; private set; }

        #endregion

        #region Public API

        /// <summary>
        /// 대상 UI의 중앙을 기준으로 툴팁을 표시합니다.
        /// 화면 경계를 넘는 경우 안쪽으로 위치를 보정합니다.
        /// </summary>
        public bool Show(RectTransform target,string displayName,string category,string description)
        {
            if (target == null)
            {
                Hide();
                return false;
            }

            Canvas targetCanvas = target.GetComponentInParent<Canvas>();
            Camera targetCamera = GetCanvasCamera(targetCanvas);
            Vector3 worldCenter = target.TransformPoint(target.rect.center);
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(targetCamera, worldCenter);
            return ShowAtScreenPoint(screenPoint, displayName, category, description);
        }

        /// <summary>
        /// 전달받은 화면 좌표를 기준으로 툴팁을 표시합니다.
        /// 설명에 포함된 TMP Rich Text 태그를 그대로 사용합니다.
        /// </summary>
        public bool ShowAtScreenPoint(Vector2 screenPoint,string displayName,string category,string description)
        {
            if (!isActiveAndEnabled) return false;
            if (!PrepareReferences())
            {
                Debug.LogWarning("툴팁의 View·Layout 참조와 부모 Canvas를 확인해주세요.",this);
                return false;
            }

            SetContent(displayName, category, description);
            ResizeToContent();

            if (!PlaceAtScreenPoint(screenPoint))
            {
                Hide();
                return false;
            }

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 1f;
            IsVisible = true;
            return true;
        }

        /// <summary>툴팁을 숨기고 포인터 입력을 차단하지 않도록 유지합니다.</summary>
        public void Hide()
        {
            IsVisible = false;

            if (_canvasGroup == null) return;

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        #endregion

        #region Unity Messages

        private void OnEnable()
        {
            Hide();
        }

        private void OnDisable()
        {
            Hide();
        }

        #endregion

        #region Content

        private bool PrepareReferences()
        {
            if (_root == null || _canvasGroup == null || _nameText == null || _categoryText == null ||
                _descriptionText == null || _rootLayout == null || _headerLayout == null)
            {
                return false;
            }

            _parentRect = _root.parent as RectTransform;
            _canvas = _root.GetComponentInParent<Canvas>();
            return _parentRect != null && _canvas != null;
        }

        private void SetContent(string displayName,string category,string description)
        {
            _nameText.text = displayName ?? string.Empty;
            _categoryText.text = category ?? string.Empty;
            _descriptionText.text = description ?? string.Empty;
            _categoryText.gameObject.SetActive(!string.IsNullOrWhiteSpace(category));
            _descriptionText.gameObject.SetActive(!string.IsNullOrWhiteSpace(description));
        }

        #endregion

        #region Layout

        private void ResizeToContent()
        {
            float headerWidth = GetNaturalWidth(_nameText);
            headerWidth += _headerLayout.padding.horizontal;

            if (_categoryText.gameObject.activeSelf)
            {
                headerWidth += _headerLayout.spacing;
                headerWidth += LayoutUtility.GetPreferredWidth(_categoryText.rectTransform);
            }

            float descriptionWidth = 0f;

            if (_descriptionText.gameObject.activeSelf) descriptionWidth = GetNaturalWidth(_descriptionText);

            float contentWidth = Mathf.Max(headerWidth, descriptionWidth);
            float availableWidth = Mathf.Max(1f,_parentRect.rect.width - _edgePadding * 2f);
            float maximumWidth = Mathf.Min(Mathf.Max(1f, _maximumWidth),availableWidth);
            float minimumWidth = Mathf.Clamp(_minimumWidth, 1f, maximumWidth);
            float width = Mathf.Clamp(contentWidth + _rootLayout.padding.horizontal,minimumWidth,maximumWidth);

            _root.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

            // 지정한 너비에서 줄바꿈과 필요한 높이를 다시 계산합니다
            LayoutRebuilder.ForceRebuildLayoutImmediate(_root);
        }

        private static float GetNaturalWidth(TMP_Text text)
        {
            return text.GetPreferredValues(text.text,Mathf.Infinity,Mathf.Infinity).x;
        }

        private bool PlaceAtScreenPoint(Vector2 screenPoint)
        {
            Camera uiCamera = GetCanvasCamera(_canvas);

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect,screenPoint,uiCamera,out Vector2 localPoint))
            {
                return false;
            }

            Rect bounds = _parentRect.rect;

            float minimumX = bounds.xMin + _edgePadding;
            float maximumX = bounds.xMax - _edgePadding - _root.rect.width;
            float minimumY = bounds.yMin + _edgePadding + _root.rect.height;
            float maximumY = bounds.yMax - _edgePadding;

            localPoint.x = Mathf.Clamp(localPoint.x, minimumX, maximumX);

            if (minimumY <= maximumY) localPoint.y = Mathf.Clamp(localPoint.y, minimumY, maximumY);
            else localPoint.y = maximumY;

            // Root의 Pivot이 좌상단이므로 이 위치가 툴팁의 좌상단이 됩니다
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

#if UNITY_EDITOR
        #region Context Menu

        [ContextMenu("Item Tooltip/Show Preview")]
        private void ShowPreviewForTest()
        {
            if (!Application.isPlaying || !isActiveAndEnabled) return;
            if (_previewTarget != null)
            {
                Show(_previewTarget,_previewName,_previewCategory,_previewDescription); return;
            }
            ShowAtScreenPoint(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f),_previewName,_previewCategory,_previewDescription);
        }

        [ContextMenu("Item Tooltip/Hide")]
        private void HideForTest()
        {
            if (Application.isPlaying) Hide();
        }

        #endregion
#endif
    }
}