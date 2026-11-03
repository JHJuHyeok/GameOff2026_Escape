using UnityEngine;

namespace UtilityTool
{
    public static class LayerMaskExtensions
    {
        /// <summary>충돌한 오브젝트의 레이어가 마스크에 포함되는지: if (hitMask.Contains(other.gameObject.layer))</summary>
        public static bool Contains(this LayerMask mask, int layer) => (mask.value & (1 << layer)) != 0;
    }
}
