namespace MainSystem
{
    public enum GameState
    {
        Boot,       // 초기화 중 (Boot 씬)
        Title,      // 타이틀 / 메인 메뉴
        Playing,    // 플레이 중
        Paused,     // 일시정지
        GameOver,   // 게임 오버
        Clear       // 클리어
    }
}
