using System.Collections.Generic;
using UnityEngine;

namespace RuleSystem
{
    public enum RuleMode 
    { 
        Prohibition,        // "~하지 마시오": 트리거가 발생하면 위반
        Obligation          // "~하면 N초 안에 ~하시오": 트리거 발생 후 N초 내에 수행 필요
    }

    public enum RuleVeracity 
    { 
        Truthful,           // 규칙대로 적용
        Deceptive           // 가짜 규칙 -> 어떻게 할지 의논 필요
    }

    [CreateAssetMenu(fileName = "Rule", menuName = "RuleSystem/Rule")]
    public class RuleDefinition : ScriptableObject
    {
        [Header("식별")]
        public string ruleId;
        [TextArea] public string displayText;

        [Header("논리")]
        public RuleMode mode;
        public RuleVeracity veracity;
        [Tooltip("Prohibition: 금지된 행동 / Obligation: 의무가 발생하는 계기")]
        public RuleTrigger trigger = new();
        [Tooltip("Obligation 전용: 트리거 이후 수행해야 하는 행동")]
        public RuleTrigger response = new();
        public float deadline = 5f;

        [Header("효과")]
        public List<RuleEffect> onViolation = new();
        // public List<RuleEffect> onFulfilled = new();     // 보상 (논의 필요)

        [Header("추첨")]
        [Min(0)] public float weight = 1f;
        [Tooltip("같은 그룹의 규칙은 한 판에 하나로 제한(모순/중복 방지)")]
        public char? exclusiveGroup;
    }
}
