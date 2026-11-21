using UnityEngine;
using UnityEngine.UI;

namespace Ju.Environment
{
    /// 상호작용 진행도 표시 슬라이더. 항상 활성 상태인 오브젝트에 붙이고 root를 자식으로 둘 것.
    public class InteractionProgressView : MonoBehaviour
    {
        public static InteractionProgressView Instance { get; private set; }

        [SerializeField] private GameObject root;
        [SerializeField] private Slider slider;

        private void Awake()
        {
            Instance = this;

            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;

            root.SetActive(false);
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        public void Show()
        {
            slider.value = 0f;
            root.SetActive(true);
        }

        public void SetProgress(float t) => slider.value = Mathf.Clamp01(t);

        public void Hide() => root.SetActive(false);
    }
}
