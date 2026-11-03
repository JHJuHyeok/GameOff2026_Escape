using UnityEngine;

namespace UtilityTool
{
    /// <summary>
    /// Time.time 기반 쿨다운. Update에서 Tick을 호출할 필요가 없음.
    ///
    /// 사용 예:
    ///   [SerializeField] float attackDelay = 0.4f;
    ///   Cooldown _attackCd;
    ///   void Awake() => _attackCd = new Cooldown(attackDelay);
    ///   void Attack() { if (!_attackCd.TryTrigger()) return; ... }
    /// </summary>
    public class Cooldown
    {
        public float Duration { get; set; }
        private readonly bool _unscaled;
        private float _readyTime;

        /// <param name="unscaled">true면 timeScale(히트스톱, 일시정지)의 영향을 받지 않음</param>
        public Cooldown(float duration, bool unscaled = false)
        {
            Duration = duration;
            _unscaled = unscaled;
            _readyTime = 0f;
        }

        private float Now => _unscaled ? Time.unscaledTime : Time.time;

        public bool IsReady => Now >= _readyTime;
        public float Remaining => Mathf.Max(0f, _readyTime - Now);

        /// <summary>0(방금 사용) ~ 1(사용 가능) 쿨다운 UI 게이지용</summary>
        public float Progress => Duration <= 0f ? 1f : 1f - Mathf.Clamp01(Remaining / Duration);

        /// <summary>쿨다운 시작</summary>
        public void Trigger() => _readyTime = Now + Duration;

        /// <summary>준비됐으면 쿨다운을 시작하고 true, 아니면 false</summary>
        public bool TryTrigger()
        {
            if (!IsReady) return false;
            Trigger();
            return true;
        }

        /// <summary>즉시 사용 가능 상태로</summary>
        public void Reset() => _readyTime = 0f;
    }
}
