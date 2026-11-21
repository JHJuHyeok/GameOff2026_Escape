using UnityEngine;
using UnityEngine.UI;

namespace SH.Project.UI.MainHUD
{
    [DisallowMultipleComponent]
    public sealed class LetterboxFeedbackView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _topBar;
        [SerializeField] private Image _bottomBar;

        [Header("Colors")]
        [SerializeField] private Color _baseColor = Color.black;
        [SerializeField] private Color _pulseColor = Color.red;
        [SerializeField, Range(0f, 1f)] private float _intensity = 0.65f;

        [Header("Heartbeat")]
        [SerializeField, Min(0.1f)] private float _cycleDuration = 1.2f;
        [SerializeField, Min(0.01f)] private float _beatDuration = 0.18f;
        [SerializeField, Min(0f)] private float _secondBeatDelay = 0.24f;

        [SerializeField, Range(0f, 1f)]
        private float _secondBeatStrength = 0.65f;

        private bool _initialized;
        private bool _isPlaying;
        private float _elapsed;
        private float _pulse;

        /// <summary>심박 연출이 재생 중인지 알려줍니다.</summary>
        public bool IsPlaying => _isPlaying;

        private void Awake()
        {
            _initialized = _topBar != null && _bottomBar != null && _topBar != _bottomBar;
        }

        private void OnEnable()
        {
            StopHeartbeat();
        }

        private void OnDisable()
        {
            StopHeartbeat();
        }

        private void Update()
        {
            if (!_isPlaying || !_initialized) return;
            _elapsed += Time.unscaledDeltaTime;
            float delay = Mathf.Max(0f, _secondBeatDelay);
            // 두 번째 박동까지 끝난 뒤 다음 주기가 시작되도록 보장합니다.
            float cycleDuration = Mathf.Max(_cycleDuration,delay + Mathf.Max(0.01f, _beatDuration) + 0.01f);
            float cycleTime = Mathf.Repeat(_elapsed, cycleDuration);
            float firstBeat = EvaluateBeat(cycleTime);
            float secondBeat = EvaluateBeat(cycleTime - delay) * Mathf.Clamp01(_secondBeatStrength);

            ApplyPulse(Mathf.Clamp01(firstBeat + secondBeat));
        }

        /// <summary>
        /// 심박 연출을 처음부터 재생합니다.
        /// 참조가 준비되지 않았거나 비활성 상태면 false를 반환합니다.
        /// </summary>
        public bool PlayHeartbeat()
        {
            if (!isActiveAndEnabled || !_initialized) return false;

            _elapsed = 0f;
            _isPlaying = true;
            ApplyPulse(0f);

            return true;
        }

        /// <summary>심박 연출을 멈추고 기본 색으로 즉시 복원합니다.</summary>
        public void StopHeartbeat()
        {
            _isPlaying = false;
            _elapsed = 0f;
            ApplyPulse(0f);
        }

        /// <summary>
        /// 연출 강도를 0~1 범위로 지정합니다.
        /// 재생 중 호출해도 심박 리듬은 다시 시작하지 않습니다.
        /// </summary>
        public void SetIntensity(float intensity)
        {
            _intensity = Mathf.Clamp01(intensity);
            ApplyPulse(_pulse);
        }

        private float EvaluateBeat(float time)
        {
            float duration = Mathf.Max(0.01f, _beatDuration);
            if (time < 0f || time >= duration) return 0f;
            float value = Mathf.Sin(time / duration * Mathf.PI);
            return value * value;
        }

        private void ApplyPulse(float pulse)
        {
            _pulse = pulse;

            if (!_initialized || _topBar == null || _bottomBar == null) return;
            float strength = Mathf.Clamp01(pulse) * Mathf.Clamp01(_intensity);
            Color color = Color.Lerp(_baseColor, _pulseColor, strength);
            _topBar.color = color;
            _bottomBar.color = color;
        }

#if UNITY_EDITOR
        [ContextMenu("Letterbox/Play Heartbeat")]
        private void PlayHeartbeatForTest()
        {
            if (!Application.isPlaying) return;
            PlayHeartbeat();
        }

        [ContextMenu("Letterbox/Stop Heartbeat")]
        private void StopHeartbeatForTest()
        {
            if (!Application.isPlaying) return;
            StopHeartbeat();
        }
#endif
    }
}
