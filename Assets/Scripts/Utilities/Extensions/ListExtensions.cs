using System.Collections.Generic;
using UnityEngine;

namespace UtilityTool
{
    public static class ListExtensions
    {
        /// <summary>Fisher-Yates 셔플 (리스트, 배열 모두 가능)</summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>무작위 요소 하나. 비어 있으면 default</summary>
        public static T GetRandom<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0) return default;
            return list[Random.Range(0, list.Count)];
        }

        public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
            => collection == null || collection.Count == 0;

        /// <summary>순서가 중요하지 않을 때 O(1) 삭제 (마지막 요소와 교체 후 제거)</summary>
        public static void RemoveSwapBack<T>(this List<T> list, int index)
        {
            int last = list.Count - 1;
            list[index] = list[last];
            list.RemoveAt(last);
        }
    }
}
