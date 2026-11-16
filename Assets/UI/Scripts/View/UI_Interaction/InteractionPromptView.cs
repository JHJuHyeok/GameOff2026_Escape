using TMPro;
using UnityEngine;

namespace SH.Project.UI.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class InteractionPromptView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _keyLabel;

        #region Properties

        //CanvasGroup과 키 라벨이 연결되어 있는지 나타냅니다
        public bool IsConfigured => _canvasGroup != null && _keyLabel != null;

        #endregion

        #region Public API

        //전달받은 색을 상호작용 키 라벨에 적용합니다
        public void SetColor(Color color)
        {
            _keyLabel.color = color;
        }

        //CanvasGroup의 알파를 변경해 프롬프트를 표시하거나 숨깁니다
        public void SetVisible(bool visible)
        {
            float alpha = visible ? 1f : 0f;
            if (_canvasGroup.alpha != alpha) _canvasGroup.alpha = alpha;
        }

        #endregion

        #region Unity Messages

        // 컴포넌트를 추가하거나 Reset할 때 CanvasGroup과 하위 키 라벨을 찾습니다.
        private void Reset()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _keyLabel = GetComponentInChildren<TMP_Text>(true);
        }

        #endregion
    }
}
