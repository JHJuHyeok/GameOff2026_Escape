using UnityEngine;

namespace UtilityTool
{
    public static class GameObjectExtensions
    {
        /// <summary>컴포넌트가 있으면 반환, 없으면 추가 후 반환</summary>
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            if (!go.TryGetComponent(out T component))
                component = go.AddComponent<T>();
            return component;
        }

        public static T GetOrAddComponent<T>(this Component c) where T : Component
            => c.gameObject.GetOrAddComponent<T>();

        public static bool HasComponent<T>(this GameObject go) where T : Component
            => go.TryGetComponent<T>(out _);

        /// <summary>자식까지 모두 같은 레이어로 설정</summary>
        public static void SetLayerRecursive(this GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
                child.gameObject.SetLayerRecursive(layer);
        }
    }
}
