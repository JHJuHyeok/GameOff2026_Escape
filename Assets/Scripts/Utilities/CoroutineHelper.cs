using System;
using System.Collections;
using UnityEngine;

namespace UtilityTool
{
    /// <summary>
    /// 어디서든 코루틴 기반 지연/반복 실행을 호출할 수 있는 도우미.
    /// 씬에 배치할 필요 없이 첫 호출 시 숨겨진 오브젝트가 자동 생성됨.
    ///
    /// 사용 예:
    ///   CoroutineHelper.Delay(1.5f, () => Debug.Log("1.5초 뒤"));
    ///   var c = CoroutineHelper.Repeat(0.5f, Spawn);
    ///   CoroutineHelper.Stop(c);
    ///
    /// 주의: 실행 주체가 이 도우미이므로, 호출한 오브젝트가 파괴돼도 코루틴은 계속 실행됨.
    ///       콜백에서 null 체크를 하거나 반환된 Coroutine을 Stop 하세요.
    /// </summary>
    public class CoroutineHelper : Singleton<CoroutineHelper>
    {
        // Domain Reload 비활성화 옵션 대비
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
            _isQuitting = false;
        }

        // ---------------- Public API ----------------

        /// <summary>seconds 후 action 실행</summary>
        public static Coroutine Delay(float seconds, Action action, bool unscaled = false)
        {
            var inst = Instance;
            return inst == null ? null : inst.StartCoroutine(DelayRoutine(seconds, action, unscaled));
        }

        /// <summary>다음 프레임에 action 실행</summary>
        public static Coroutine NextFrame(Action action)
        {
            var inst = Instance;
            return inst == null ? null : inst.StartCoroutine(NextFrameRoutine(action));
        }

        /// <summary>interval 간격으로 action 반복. count가 음수면 무한 반복</summary>
        public static Coroutine Repeat(float interval, Action action, int count = -1, bool unscaled = false)
        {
            var inst = Instance;
            return inst == null ? null : inst.StartCoroutine(RepeatRoutine(interval, action, count, unscaled));
        }

        public static void Stop(Coroutine coroutine)
        {
            if (coroutine != null && _instance != null)
                _instance.StopCoroutine(coroutine);
        }

        public static void StopAll()
        {
            if (_instance != null) _instance.StopAllCoroutines();
        }

        // ---------------- Routines ----------------

        private static IEnumerator DelayRoutine(float seconds, Action action, bool unscaled)
        {
            if (unscaled) yield return new WaitForSecondsRealtime(seconds);
            else yield return new WaitForSeconds(seconds);
            action?.Invoke();
        }

        private static IEnumerator NextFrameRoutine(Action action)
        {
            yield return null;
            action?.Invoke();
        }

        private static IEnumerator RepeatRoutine(float interval, Action action, int count, bool unscaled)
        {
            for (int i = 0; count < 0 || i < count; i++)
            {
                if (unscaled) yield return new WaitForSecondsRealtime(interval);
                else yield return new WaitForSeconds(interval);
                action?.Invoke();
            }
        }
    }
}
