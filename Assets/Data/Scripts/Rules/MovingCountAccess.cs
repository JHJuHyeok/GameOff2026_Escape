using Ju.Player;

namespace Ju.RuleSystem
{
    /// PlayerController.movingCount 접근을 한 곳에 모은 어댑터.
    /// 변수 이름이나 접근 수준(프로퍼티, private set 등)이 다르면 이 파일만 고치면 된다.
    public static class MovingCountAccess
    {
        public static int Get(PlayerController p) => p.MovingCount;
        public static void Set(PlayerController p, int value) => p.MovingCount = System.Math.Max(0, value);
    }
}
