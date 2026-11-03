using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MainSystem
{
    /// <summary>
    /// 비동기 씬 로딩 + 페이드 인/아웃.
    /// 페이드용 Canvas는 코드로 자동 생성되므로 별도 프리팹이 필요 없음.
    /// 모든 연출은 unscaledTime 기반이라 timeScale = 0 상태에서도 동작.
    ///
    /// 사용 예: SceneLoader.Instance.LoadScene("Game");
    /// </summary>
    public class SceneLoader : Singleton<SceneLoader>
    {
        [SerializeField, Min(0f)] private float defaultFadeDuration = 0.4f;
        [SerializeField] private Color fadeColor = Color.black;
        [Tooltip("로딩이 너무 빨라도 최소 이 시간(초)은 로딩 화면 유지")]
        [SerializeField, Min(0f)] private float minLoadTime = 0f;

        public bool IsLoading { get; private set; }

        public event Action<string> OnLoadStarted;
        public event Action<string> OnLoadCompleted;
        /// <summary>0 ~ 1 로딩 진행률 (로딩 바 연동용)</summary>
        public event Action<float> OnProgress;

        private CanvasGroup _fadeGroup;

        protected override void Init() => BuildFadeCanvas();

        // ---------------- Public API ----------------

        public void LoadScene(string sceneName, float fadeDuration = -1f)
            => Begin(sceneName, () => SceneManager.LoadSceneAsync(sceneName), fadeDuration);

        public void LoadScene(int buildIndex, float fadeDuration = -1f)
            => Begin($"Scene#{buildIndex}", () => SceneManager.LoadSceneAsync(buildIndex), fadeDuration);

        public void ReloadScene(float fadeDuration = -1f)
            => LoadScene(SceneManager.GetActiveScene().buildIndex, fadeDuration);

        /// <summary>화면을 덮는다 (컷신, 연출용)</summary>
        public Coroutine FadeOut(float duration = -1f) => StartCoroutine(Fade(1f, duration < 0f ? defaultFadeDuration : duration));

        /// <summary>화면을 다시 보여준다</summary>
        public Coroutine FadeIn(float duration = -1f) => StartCoroutine(Fade(0f, duration < 0f ? defaultFadeDuration : duration));

        // ---------------- 내부 구현 ----------------

        private void Begin(string label, Func<AsyncOperation> startOperation, float fadeDuration)
        {
            if (IsLoading)
            {
                Debug.LogWarning($"[SceneLoader] 이미 로딩 중입니다. 요청 무시: {label}");
                return;
            }
            StartCoroutine(LoadRoutine(label, startOperation, fadeDuration < 0f ? defaultFadeDuration : fadeDuration));
        }

        private IEnumerator LoadRoutine(string label, Func<AsyncOperation> startOperation, float fade)
        {
            IsLoading = true;
            OnLoadStarted?.Invoke(label);

            yield return Fade(1f, fade);

            float startTime = Time.unscaledTime;
            AsyncOperation op = startOperation();

            if (op == null) // 존재하지 않는 씬 등
            {
                Debug.LogError($"[SceneLoader] 씬을 로드할 수 없습니다: {label} (Build Settings 등록 여부 확인)");
                yield return Fade(0f, fade);
                IsLoading = false;
                yield break;
            }

            op.allowSceneActivation = false;
            while (op.progress < 0.9f || Time.unscaledTime - startTime < minLoadTime)
            {
                OnProgress?.Invoke(Mathf.Clamp01(op.progress / 0.9f));
                yield return null;
            }

            OnProgress?.Invoke(1f);
            op.allowSceneActivation = true;
            while (!op.isDone) yield return null;

            OnLoadCompleted?.Invoke(label);
            yield return Fade(0f, fade);

            IsLoading = false;
        }

        private IEnumerator Fade(float targetAlpha, float duration)
        {
            float from = _fadeGroup.alpha;
            _fadeGroup.blocksRaycasts = true; // 페이드 중 UI 클릭 차단

            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                _fadeGroup.alpha = Mathf.Lerp(from, targetAlpha, t / duration);
                yield return null;
            }

            _fadeGroup.alpha = targetAlpha;
            _fadeGroup.blocksRaycasts = targetAlpha > 0f;
        }

        private void BuildFadeCanvas()
        {
            GameObject canvasObj = new GameObject("FadeCanvas");
            canvasObj.transform.SetParent(transform, false);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            _fadeGroup = canvasObj.AddComponent<CanvasGroup>();
            _fadeGroup.alpha = 0f;
            _fadeGroup.blocksRaycasts = false;
            _fadeGroup.interactable = false;

            GameObject imageObj = new GameObject("FadeImage");
            imageObj.transform.SetParent(canvasObj.transform, false);

            Image image = imageObj.AddComponent<Image>();
            image.color = fadeColor;
            image.raycastTarget = true;

            RectTransform rt = image.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
