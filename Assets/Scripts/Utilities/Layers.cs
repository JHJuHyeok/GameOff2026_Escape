using UnityEngine;

namespace UtilityTool
{
    /// <summary>
    /// 레이어 인덱스/마스크 상수. 오타 방지 + 매번 NameToLayer를 호출하지 않아도 됨.
    /// ※ 템플릿 프로젝트의 Project Settings > Tags and Layers 에서
    ///   아래 번호(8~)에 맞춰 같은 이름으로 레이어를 만들어 두어야 함.
    /// 사용 예:
    ///   gameObject.layer = Layers.Player;
    ///   Physics2D.OverlapCircle(pos, r, Layers.EnemyMask | Layers.GroundMask);
    /// </summary>
    public static class Layers
    {
        // ---- 유니티 기본 레이어 ----
        public const int Default = 0;
        public const int TransparentFX = 1;
        public const int IgnoreRaycast = 2;
        public const int Water = 4;
        public const int UI = 5;

        // ---- 프로젝트 레이어 (8번부터 직접 지정) ----
        public const int Player = 8;
        public const int Enemy = 9;
        public const int Ground = 10;
        public const int Projectile = 11;
        public const int Interactable = 12;
        public const int Pickup = 13;

        // ---- 마스크 (비트 연산 결과) ----
        public const int DefaultMask = 1 << Default;
        public const int PlayerMask = 1 << Player;
        public const int EnemyMask = 1 << Enemy;
        public const int GroundMask = 1 << Ground;
        public const int ProjectileMask = 1 << Projectile;
        public const int InteractableMask = 1 << Interactable;
        public const int PickupMask = 1 << Pickup;

        /// <summary>레이어 번호 → 마스크</summary>
        public static int ToMask(int layer) => 1 << layer;

        /// <summary>오브젝트의 레이어가 마스크에 포함되는지</summary>
        public static bool InMask(GameObject go, int mask) => (mask & (1 << go.layer)) != 0;
    }
}
