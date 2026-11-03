using System.Collections.Generic;
using UnityEngine;

namespace UtilityTool
{
    public static class TransformExtensions
    {
        /// <summary>모든 자식 오브젝트 삭제 (플레이 중엔 Destroy, 에디터에선 DestroyImmediate)</summary>
        public static void DestroyChildren(this Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                GameObject child = t.GetChild(i).gameObject;
                if (Application.isPlaying) Object.Destroy(child);
                else Object.DestroyImmediate(child);
            }
        }

        /// <summary>로컬 위치/회전/스케일 초기화</summary>
        public static void ResetLocal(this Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        /// <summary>2D에서 target을 바라보게 회전. 스프라이트가 위쪽을 보고 있으면 offsetDeg = -90</summary>
        public static void LookAt2D(this Transform t, Vector2 target, float offsetDeg = 0f)
        {
            Vector2 dir = target - (Vector2)t.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            t.rotation = Quaternion.Euler(0f, 0f, angle + offsetDeg);
        }

        /// <summary>직계 자식 목록</summary>
        public static List<Transform> GetChildren(this Transform t)
        {
            var list = new List<Transform>(t.childCount);
            for (int i = 0; i < t.childCount; i++) list.Add(t.GetChild(i));
            return list;
        }

        public static void SetPositionX(this Transform t, float x) => t.position = t.position.WithX(x);
        public static void SetPositionY(this Transform t, float y) => t.position = t.position.WithY(y);
        public static void SetPositionZ(this Transform t, float z) => t.position = t.position.WithZ(z);
    }
}
