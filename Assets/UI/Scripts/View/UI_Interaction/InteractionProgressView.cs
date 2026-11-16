using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class InteractionProgressView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _fill;

        #region Properties

        //>CanvasGroup과 Fill Image가 연결되어 있고, Fill Image가 Filled 타입인지 나타냅니다
        public bool IsConfigured => _canvasGroup != null && _fill != null && _fill.type == Image.Type.Filled;

        #endregion

        #region Public API

        //전달받은 진행값을 0~1 범위로 제한하고 게이지 채움에 적용합니다
        public void SetProgress(float normalizedProgress)
        {
            _fill.fillAmount = Mathf.Clamp01(normalizedProgress);
        }

        //CanvasGroup의 알파를 변경해 진행도 패널을 표시하거나 숨깁니다
        public void SetVisible(bool visible)
        {
            float alpha = visible ? 1f : 0f;
            if (_canvasGroup.alpha != alpha) _canvasGroup.alpha = alpha;
        }

        #endregion

        #region Unity Messages

        // 컴포넌트를 추가하거나 Reset할 때 같은 오브젝트의 CanvasGroup을 찾습니다.
        private void Reset()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        #endregion
    }
}
