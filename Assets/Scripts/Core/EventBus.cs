using System;
using System.Collections.Generic;


namespace MainSystem
{
    /// 이벤트 분류용 인터페이스
    public interface IEvent { }

    public static class EventBus<T> where T : struct, IEvent
    {
        private static readonly List<Action<T>> handlers = new();

        public static void Subscribe(Action<T> h) => handlers.Add(h);
        public static void UnSubscribe(Action<T> h) => handlers.Remove(h);

        public static void Raise(T evt)
        {
            for (int i = handlers.Count - 1; i >= 0; i--)
                handlers[i]?.Invoke(evt);
        }

        public static void Clear() => handlers.Clear();
    }
}

