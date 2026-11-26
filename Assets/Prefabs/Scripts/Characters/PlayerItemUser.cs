using System.Collections.Generic;
using UnityEngine;
using Ju.Player;

namespace Ju.Player
{
    /// 플레이어에 붙이는 '아이템 사용' 진입점. 인벤토리 UI의 사용 입력(우클릭/버튼 등)에서 TryUse를 호출한다.
    public class PlayerItemUser : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        private readonly Dictionary<ItemData, int> activeLights = new();

        private void Awake()
        {
            if (player == null) player = GetComponent<PlayerController>();
        }

        public bool TryUse(ItemInstance item)
        {
            if (item == null || player == null || player.IsDead) return false;
            if (item.Data is not IUsableItem usable) return false;

            bool ok = usable.Use(new ItemUseContext { Item = item, Player = player, User = this });

            // 규칙 시스템 연동: ItemInstance가 ITargetable을 구현하면 아래 주석을 해제
            // if (ok) PlayerController.Emit(ActionType.Use, item);
            return ok;
        }

        // ---- 활성 광원 추적 (LightItemData가 사용) ----
        public bool TryGetActiveLight(ItemData data, out int id) => activeLights.TryGetValue(data, out id);
        public void SetActiveLight(ItemData data, int id) => activeLights[data] = id;
        public void ClearActiveLight(ItemData data) => activeLights.Remove(data);
    }
}
