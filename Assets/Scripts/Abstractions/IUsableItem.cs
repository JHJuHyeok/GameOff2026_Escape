using System;

namespace Ju.Player
{
    /// 사용 가능한 아이템 데이터가 구현. (ItemData 서브클래스에 붙임)
    public interface IUsableItem
    {
        /// <returns>사용 성공 여부</returns>
        bool Use(ItemUseContext ctx);
    }

    public class ItemUseContext
    {
        public ItemInstance Item;
        public PlayerController Player;
        public PlayerItemUser User;

        public Action OnEffectEnded;
    }
}
