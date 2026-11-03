using UnityEngine;

namespace UtilityTool
{
    public static class VectorExtensions
    {
        // ---------- 일부 축만 교체 ----------
        public static Vector3 WithX(this Vector3 v, float x) => new Vector3(x, v.y, v.z);
        public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);
        public static Vector3 WithZ(this Vector3 v, float z) => new Vector3(v.x, v.y, z);
        public static Vector2 WithX(this Vector2 v, float x) => new Vector2(x, v.y);
        public static Vector2 WithY(this Vector2 v, float y) => new Vector2(v.x, y);

        // ---------- 변환 ----------
        public static Vector2 ToVector2(this Vector3 v) => new Vector2(v.x, v.y);
        public static Vector3 ToVector3(this Vector2 v, float z = 0f) => new Vector3(v.x, v.y, z);

        /// <summary>y를 0으로 만든 벡터 (3D 탑뷰 이동 방향 계산용)</summary>
        public static Vector3 Flat(this Vector3 v) => new Vector3(v.x, 0f, v.z);

        // ---------- 방향 / 각도 ----------
        public static Vector2 DirectionTo(this Vector2 from, Vector2 to) => (to - from).normalized;
        public static Vector3 DirectionTo(this Vector3 from, Vector3 to) => (to - from).normalized;

        /// <summary>오른쪽(1,0) 기준 각도(도). 반시계 방향이 +</summary>
        public static float AngleDeg(this Vector2 v) => Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;

        /// <summary>각도(도)로 단위 벡터 생성</summary>
        public static Vector2 FromAngleDeg(float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }

        /// <summary>벡터를 degrees만큼 회전 (반시계 +)</summary>
        public static Vector2 Rotate(this Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        // ---------- 기타 ----------
        /// <summary>그리드 크기에 맞춰 좌표를 스냅</summary>
        public static Vector2 Snap(this Vector2 v, float gridSize)
        {
            return new Vector2(Mathf.Round(v.x / gridSize) * gridSize,
                               Mathf.Round(v.y / gridSize) * gridSize);
        }

        public static Vector3 Snap(this Vector3 v, float gridSize)
        {
            return new Vector3(Mathf.Round(v.x / gridSize) * gridSize,
                               Mathf.Round(v.y / gridSize) * gridSize,
                               Mathf.Round(v.z / gridSize) * gridSize);
        }

        public static bool IsNear(this Vector3 a, Vector3 b, float distance)
            => (a - b).sqrMagnitude <= distance * distance;

        public static bool IsNear(this Vector2 a, Vector2 b, float distance)
            => (a - b).sqrMagnitude <= distance * distance;
    }
}
