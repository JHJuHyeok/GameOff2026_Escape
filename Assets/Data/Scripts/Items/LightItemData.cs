using UnityEngine;
using Ju.Visual;

namespace Ju.Player
{
    [CreateAssetMenu(menuName = "Data/Light Item Data", fileName = "NewLightItem")]
    public class LightItemData : ItemData, IExpandableItem
    {
        [Header("Light")]
        [Min(0.1f)] public float radius = 4f;
        [Range(0f, 1f)] public float softness = 0.35f;
        [Tooltip("0이면 장착 해제 전까지 유지(내구도 소모 없음). 0보다 크면 효과 종료 시 내구도 1 감소")]
        [Min(0f)] public float duration = 30f;
        public float fadeIn = 0.3f;
        public float fadeOut = 1.5f;

        public ExpandableUseTiming UseTiming => ExpandableUseTiming.OnEquip;

        private void Reset() => type = ItemType.Expendable;

        public void OnEquip(ItemUseContext ctx)
        {
            var overlay = BlackoutOverlay.Instance;
            if (overlay == null) return;

            if (ctx.User.TryGetActiveLight(this, out int activeId) && overlay.HasLight(activeId)) return;

            int id = overlay.AddLight(ctx.Player.transform, radius, softness,
                                      duration > 0f ? duration : -1f, fadeIn, fadeOut);
            if (id < 0) return;

            ctx.User.SetActiveLight(this, id);

            if (duration > 0f) WatchLightEnd(overlay, id, ctx);
        }

        public void OnUnequip(ItemUseContext ctx)
        {
            var overlay = BlackoutOverlay.Instance;
            if (ctx.User.TryGetActiveLight(this, out int activeId) && overlay != null)
                overlay.ReleaseLight(activeId);     // 이미 만료된 id면 무시됨

            ctx.User.ClearActiveLight(this);
        }

        // 해당 광원이 사라지는 순간 한 번만 알린다.
        private static void WatchLightEnd(BlackoutOverlay overlay, int id, ItemUseContext ctx)
        {
            void Handler(int endedId)
            {
                if (endedId != id) return;
                overlay.LightEnded -= Handler;
                ctx.OnEffectEnded?.Invoke();
            }
            overlay.LightEnded += Handler;
        }
    }
}