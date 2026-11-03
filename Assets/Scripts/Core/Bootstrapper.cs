using UnityEngine;

namespace MainSystem
{
    /// <summary>
    /// Boot 씬에 배치. [Managers] 프리팹과 함께 두면, 매니저가 모두 준비된 뒤 첫 씬으로 이동한다.
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private string firstScene = "MainMenu";
        [SerializeField, Min(0f)] private float fadeDuration = 0f;

        private void Start()
        {
            //SceneLoader.Instance.LoadScene(firstScene, fadeDuration);
        }
    }
}
