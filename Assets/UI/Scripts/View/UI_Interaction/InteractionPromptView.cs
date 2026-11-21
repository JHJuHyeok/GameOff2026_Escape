using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class InteractionPromptView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _keyIcon;
        [SerializeField] private Color _availableColor = Color.white;
        [SerializeField] private Color _blockedColor = new Color32(128, 128, 128, 255);

        /// <summary>표시 그룹과 키캡 이미지가 연결되어 있는지 나타냅니다.</summary>
        public bool IsConfigured => _canvasGroup != null && _keyIcon != null;

        /// <summary>키캡 프롬프트를 표시하거나 숨깁니다.</summary>
        public void SetVisible(bool visible)
        {
            if (_canvasGroup == null) return;
            float alpha = 0f;

            if (visible)
            {
                alpha = 1f;
            }
            if (_canvasGroup.alpha != alpha)
            {
                _canvasGroup.alpha = alpha;
            }
        }

        /// <summary>상호작용 가능 여부에 따라 인스펙터에 지정된 키캡 색상을 적용합니다.</summary>
        public void SetInteractable(bool canInteract)
        {
            if (_keyIcon == null) return;
            if (canInteract)
            {
                _keyIcon.color = _availableColor;
            }
            else
            {
                _keyIcon.color = _blockedColor;
            }
        }

        private void Reset()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _keyIcon = GetComponentInChildren<Image>(true);
        }

#if UNITY_EDITOR
        [ContextMenu("Prompt/Show Available")]
        private void ShowAvailableForTest()
        {
            if (!Application.isPlaying || !IsConfigured) return;
            SetInteractable(true);
            SetVisible(true);
        }

        [ContextMenu("Prompt/Show Blocked")]
        private void ShowBlockedForTest()
        {
            if (!Application.isPlaying || !IsConfigured) return;
            SetInteractable(false);
            SetVisible(true);
        }

        [ContextMenu("Prompt/Hide")]
        private void HideForTest()
        {
            if (!Application.isPlaying || !IsConfigured) return;
            SetVisible(false);
        }
#endif
    }
}
