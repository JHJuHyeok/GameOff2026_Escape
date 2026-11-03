namespace MyJam
{
    /// <summary>
    /// 태그 문자열 상수. 오타 방지용.
    /// 사용 예: if (other.CompareTag(Tags.Player)) ...
    /// ※ Unity의 Tags and Layers 설정에 같은 이름의 태그를 만들어 두어야 함 (Untagged, Player 등은 기본 제공).
    /// </summary>
    public static class Tags
    {
        public const string Untagged = "Untagged";
        public const string Player = "Player";
        public const string MainCamera = "MainCamera";
        public const string GameController = "GameController";

        // 필요한 태그는 아래에 추가
        public const string Enemy = "Enemy";
        public const string Projectile = "Projectile";
        public const string Pickup = "Pickup";
        public const string Interactable = "Interactable";
    }
}
