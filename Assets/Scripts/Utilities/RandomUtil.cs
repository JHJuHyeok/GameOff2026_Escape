using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UtilityTool
{
    public static class RandomUtil
    {
        /// <summary>확률 판정. Chance(0.3f) → 30% 확률로 true</summary>
        public static bool Chance(float probability) => Random.value < probability;

        /// <summary>+1 또는 -1</summary>
        public static int Sign() => Random.value < 0.5f ? -1 : 1;

        // ---------------- 가중치 랜덤 ----------------

        /// <summary>가중치 배열에서 인덱스를 뽑음. 총합이 0 이하면 -1</summary>
        public static int WeightedIndex(IList<float> weights)
        {
            float total = 0f;
            for (int i = 0; i < weights.Count; i++) total += Mathf.Max(0f, weights[i]);
            if (total <= 0f) return -1;

            float roll = Random.value * total;
            for (int i = 0; i < weights.Count; i++)
            {
                roll -= Mathf.Max(0f, weights[i]);
                if (roll <= 0f) return i;
            }
            return weights.Count - 1;
        }

        /// <summary>항목 리스트에서 가중치 함수 기준으로 하나 선택. 예: items.WeightedRandom(i => i.dropRate)</summary>
        public static T WeightedRandom<T>(this IList<T> items, Func<T, float> weightSelector)
        {
            if (items == null || items.Count == 0) return default;

            var weights = new float[items.Count];
            for (int i = 0; i < items.Count; i++) weights[i] = weightSelector(items[i]);

            int index = WeightedIndex(weights);
            return index < 0 ? default : items[index];
        }

        // ---------------- 위치 / 방향 ----------------

        /// <summary>랜덤 2D 방향 (단위 벡터)</summary>
        public static Vector2 Direction2D() => Random.insideUnitCircle.normalized;

        /// <summary>원 내부의 랜덤 위치</summary>
        public static Vector2 InsideCircle(Vector2 center, float radius)
            => center + Random.insideUnitCircle * radius;

        /// <summary>원 둘레 위의 랜덤 위치</summary>
        public static Vector2 OnCircle(Vector2 center, float radius)
            => center + Direction2D() * radius;

        /// <summary>안쪽 반지름 ~ 바깥 반지름 사이 고리 영역의 랜덤 위치 (플레이어 주변 스폰에 유용)</summary>
        public static Vector2 InsideRing(Vector2 center, float minRadius, float maxRadius)
        {
            float r = Mathf.Sqrt(Random.Range(minRadius * minRadius, maxRadius * maxRadius));
            return center + Direction2D() * r;
        }

        /// <summary>구 내부의 랜덤 위치 (3D)</summary>
        public static Vector3 InsideSphere(Vector3 center, float radius)
            => center + Random.insideUnitSphere * radius;

        /// <summary>Bounds 내부의 랜덤 위치</summary>
        public static Vector3 InsideBounds(Bounds b) => new Vector3(
            Random.Range(b.min.x, b.max.x),
            Random.Range(b.min.y, b.max.y),
            Random.Range(b.min.z, b.max.z));

        /// <summary>Rect 내부의 랜덤 위치</summary>
        public static Vector2 InsideRect(Rect r) => new Vector2(
            Random.Range(r.xMin, r.xMax),
            Random.Range(r.yMin, r.yMax));

        // ---------------- 기타 ----------------

        /// <summary>랜덤 색상 (HSV 기반으로 선명한 색)</summary>
        public static Color Color(float saturation = 0.8f, float value = 1f)
            => UnityEngine.Color.HSVToRGB(Random.value, saturation, value);
    }
}
