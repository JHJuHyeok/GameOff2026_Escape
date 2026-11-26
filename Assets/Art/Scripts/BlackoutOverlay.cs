using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ju.Visual
{
    /// 암전 오버레이 + 원형 마스크 광원 관리.
    /// 화면 전체를 덮는 Image(Canvas: Screen Space - Overlay)에 붙인다.
    /// - 암전: BeginBlackout / EndBlackout (중첩 가능, 요청 수가 0이 되면 해제)
    /// - 광원: AddLight(Transform) — 월드 반지름 기준, 대상 Transform을 따라다님
    [RequireComponent(typeof(Image))]
    public class BlackoutOverlay : MonoBehaviour
    {
        public static BlackoutOverlay Instance { get; private set; }
        public const int MaxLights = 8;

        private static readonly int LightsId = Shader.PropertyToID("_Lights");
        private static readonly int LightCountId = Shader.PropertyToID("_LightCount");
        private static readonly int AspectId = Shader.PropertyToID("_Aspect");
        private static readonly int DarknessId = Shader.PropertyToID("_Darkness");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [Tooltip("Ju/UI/BlackoutOverlay 셰이더로 만든 머티리얼 (런타임에 인스턴스 복제)")]
        [SerializeField] private Material baseMaterial;
        [Tooltip("비우면 Camera.main")]
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Color overlayColor = Color.black;
        [SerializeField, Range(0f, 1f)] private float maxDarkness = 0.97f;

        private class LightSource
        {
            public int id;
            public Transform follow;
            public float radius;        // 월드 단위
            public float softness;      // 0~1
            public float remaining;     // < 0이면 무기한
            public float age;
            public float fadeIn, fadeOut;
        }

        private readonly List<LightSource> lights = new();
        private readonly Vector4[] uploaded = new Vector4[MaxLights];
        private Image image;
        private Material mat;
        private int nextId = 1;

        private int blackoutRequests;
        private float current, target, fadeSpeed = 2f;
        private float flickerMul = 1f;
        private Coroutine flickerRoutine;

        private int suppressCount;
        private float scaleTarget = 1f, scaleSmoothed = 1f;

        public bool IsBlackout => blackoutRequests > 0;
        public bool LightsSuppressed => suppressCount > 0;
        /// 모든 광원 반지름에 곱해지는 배수 (배터리 약화 같은 연출). 부드럽게 보간됨.
        public float LightRadiusScale { get => scaleTarget; set => scaleTarget = Mathf.Max(0.01f, value); }
        public event Action<bool> BlackoutChanged;

        private Camera Cam => worldCamera != null ? worldCamera : (worldCamera = Camera.main);

        // ---------------- 수명 ----------------
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            image = GetComponent<Image>();
            image.raycastTarget = false;
            mat = new Material(baseMaterial);
            image.material = mat;
            image.enabled = false;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (mat != null) Destroy(mat);
        }

        // ---------------- 암전 ----------------
        public void BeginBlackout(float fadeIn)
        {
            blackoutRequests++;
            target = maxDarkness;
            fadeSpeed = maxDarkness / Mathf.Max(0.01f, fadeIn);
            if (blackoutRequests == 1) BlackoutChanged?.Invoke(true);
        }

        public void EndBlackout(float fadeOut)
        {
            if (blackoutRequests <= 0) return;
            blackoutRequests--;
            if (blackoutRequests > 0) return;
            target = 0f;
            fadeSpeed = maxDarkness / Mathf.Max(0.01f, fadeOut);
            BlackoutChanged?.Invoke(false);
        }

        public void ClearBlackout(float fadeOut)
        {
            if (blackoutRequests <= 0) return;
            blackoutRequests = 1;
            EndBlackout(fadeOut);
        }

        /// 암전 농도를 불규칙하게 깜빡인다. (암전 중일 때만 보임)
        public void Flicker(float duration, float interval)
        {
            if (flickerRoutine != null) StopCoroutine(flickerRoutine);
            flickerRoutine = StartCoroutine(FlickerRoutine(duration, interval));
        }

        private IEnumerator FlickerRoutine(float duration, float interval)
        {
            float t = 0f;
            while (t < duration)
            {
                flickerMul = UnityEngine.Random.Range(0.15f, 1f);
                float w = Mathf.Max(0.02f, interval * UnityEngine.Random.Range(0.5f, 1.5f));
                yield return new WaitForSeconds(w);
                t += w;
            }
            flickerMul = 1f;
            flickerRoutine = null;
        }

        // ---------------- 광원 ----------------
        /// <returns>광원 ID. 실패(최대 개수 / 억제 중)하면 -1</returns>
        public int AddLight(Transform follow, float worldRadius, float softness = 0.35f,
                            float duration = -1f, float fadeIn = 0.3f, float fadeOut = 1.5f)
        {
            if (follow == null || LightsSuppressed) return -1;
            if (lights.Count >= MaxLights)
            {
                Debug.LogWarning("[BlackoutOverlay] 광원 최대 개수 초과");
                return -1;
            }
            lights.Add(new LightSource
            {
                id = nextId,
                follow = follow,
                radius = worldRadius,
                softness = Mathf.Clamp01(softness),
                remaining = duration > 0f ? duration : -1f,
                fadeIn = fadeIn,
                fadeOut = fadeOut
            });
            return nextId++;
        }

        public bool HasLight(int id) => lights.Exists(l => l.id == id);

        /// 페이드아웃과 함께 광원을 끈다.
        public void ReleaseLight(int id)
        {
            var l = lights.Find(x => x.id == id);
            if (l != null) Release(l);
        }

        public void ReleaseAllLights() { foreach (var l in lights) Release(l); }

        private static void Release(LightSource l)
        {
            float fo = Mathf.Max(l.fadeOut, 0.01f);
            if (l.remaining < 0f || l.remaining > fo) l.remaining = fo;
        }

        public void BeginSuppressLights() => suppressCount++;
        public void EndSuppressLights() => suppressCount = Mathf.Max(0, suppressCount - 1);

        /// 해당 월드 좌표가 밝은 상태인가? (암전이 아니거나 광원 반경 안) — 추후 '어둠 속 규칙' 조건용
        public bool IsLit(Vector3 worldPos)
        {
            if (current < maxDarkness * 0.3f) return true;
            foreach (var l in lights)
            {
                float r = l.radius * Intensity(l) * scaleSmoothed * 0.9f;
                if (((Vector2)(l.follow.position - worldPos)).sqrMagnitude <= r * r) return true;
            }
            return false;
        }

        private static float Intensity(LightSource l)
        {
            float a = l.fadeIn > 0f ? Mathf.Clamp01(l.age / l.fadeIn) : 1f;
            float b = (l.remaining >= 0f && l.fadeOut > 0f) ? Mathf.Clamp01(l.remaining / l.fadeOut) : 1f;
            return Mathf.SmoothStep(0f, 1f, a) * Mathf.SmoothStep(0f, 1f, b);
        }

        // ---------------- 갱신 ----------------
        private void Update()
        {
            float dt = Time.deltaTime;
            current = Mathf.MoveTowards(current, target, fadeSpeed * dt);
            scaleSmoothed = Mathf.MoveTowards(scaleSmoothed, scaleTarget, 2f * dt);
            TickLights(dt);

            bool visible = current > 0.001f;
            if (image.enabled != visible) image.enabled = visible;
            if (visible) Upload();
        }

        private void TickLights(float dt)
        {
            for (int i = lights.Count - 1; i >= 0; i--)
            {
                var l = lights[i];
                l.age += dt;
                if (l.follow == null) { lights.RemoveAt(i); continue; }
                if (l.remaining < 0f) continue;
                l.remaining -= dt;
                if (l.remaining <= 0f) lights.RemoveAt(i);
            }
        }

        private void Upload()
        {
            int n = 0;
            var cam = Cam;
            if (cam != null)
            {
                foreach (var l in lights)
                {
                    float s = Intensity(l) * scaleSmoothed;
                    if (s <= 0.001f) continue;

                    Vector3 vp = cam.WorldToViewportPoint(l.follow.position);
                    if (vp.z <= 0f) continue;

                    float worldHeight = cam.orthographic
                        ? cam.orthographicSize * 2f
                        : 2f * vp.z * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
                    float r = Mathf.Max(0.0001f, l.radius * s / worldHeight);

                    uploaded[n++] = new Vector4(vp.x, vp.y, r, l.softness);
                    if (n >= MaxLights) break;
                }
            }
            mat.SetVectorArray(LightsId, uploaded);
            mat.SetFloat(LightCountId, n);
            mat.SetFloat(AspectId, (float)Screen.width / Screen.height);
            mat.SetFloat(DarknessId, current * flickerMul);
            mat.SetColor(ColorId, overlayColor);
        }
    }
}
