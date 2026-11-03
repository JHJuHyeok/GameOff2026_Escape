using System;
using UnityEngine;

namespace UtilityTool
{
    /// <summary>
    /// 순수 C# 타이머. MonoBehaviour의 Update에서 Tick(Time.deltaTime)을 호출해서 사용.
    ///
    /// 사용 예:
    ///   _spawnTimer = new Timer(2f, loop: true, onComplete: Spawn);
    ///   _spawnTimer.Start();
    ///   void Update() => _spawnTimer.Tick(Time.deltaTime);
    /// </summary>
    public class Timer
    {
        public float Duration { get; private set; }     // 전체 지속 시간
        public float Elapsed { get; private set; }      // 경과 시간
        public bool IsRunning { get; private set; }
        public bool IsFinished { get; private set; }
        public bool Loop { get; set; }

        public float Remaining => Mathf.Max(0f, Duration - Elapsed);
        /// <summary>0 ~ 1 진행률</summary>
        public float Progress => Duration <= 0f ? 1f : Mathf.Clamp01(Elapsed / Duration);

        public event Action OnComplete;

        public Timer(float duration, bool loop = false, Action onComplete = null)
        {
            Duration = duration;
            Loop = loop;
            if (onComplete != null) OnComplete += onComplete;
        }

        /// <summary>처음부터 시작</summary>
        public void Start()
        {
            Elapsed = 0f;
            IsFinished = false;
            IsRunning = true;
        }

        public void Start(float newDuration)
        {
            Duration = newDuration;
            Start();
        }

        public void Pause() => IsRunning = false;
        public void Resume() { if (!IsFinished) IsRunning = true; }

        public void Stop()
        {
            IsRunning = false;
            IsFinished = false;
            Elapsed = 0f;
        }

        /// <summary>시간을 진행시킴. 이번 호출에서 완료되면 true 반환</summary>
        public bool Tick(float deltaTime)
        {
            if (!IsRunning) return false;

            Elapsed += deltaTime;
            if (Elapsed < Duration) return false;

            if (Loop && Duration > 0f)
            {
                Elapsed -= Duration;
                if (Elapsed >= Duration) Elapsed = 0f; // 프레임이 매우 긴 경우 방어
            }
            else
            {
                Elapsed = Duration;
                IsRunning = false;
                IsFinished = true;
            }

            OnComplete?.Invoke();
            return true;
        }
    }
}
