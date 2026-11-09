using UnityEngine;

namespace RuleSystem
{
    /// 규칙의 효과 정의. 새 효과는 이 클래스를 상속해 SO로 만들기
    public abstract class RuleEffect : ScriptableObject
    {
        [TextArea] public string designerNote;
        /// <param name="trigger">효과를 유발한 사건</param>
        public abstract void Apply(RuleContext ctx, RuleEvent trigger);
    }
}
