using System;
using System.Collections.Generic;
using UnityEngine;
using MainSystem;
using Player;

namespace RuleSystem
{
    /// "어디서(Floor) 무엇을(Target) 어떻게(Action) 했는가" - 'Floor'는 이후 수정 가능성 있음
    public readonly struct RuleEvent : IEvent
    {
        public readonly ActionType Action;
        public readonly ITargetable Target;
        public readonly int Floor;
        public RuleEvent(ActionType action, ITargetable target, int floor)
        { Action = action; Target = target; Floor = floor; }
    }

    // ----------- 트리거 = 행동 + 대상 + 조건 -----------
    [Serializable]
    public class RuleTrigger
    {
        public ActionType action;
        public TargetFilter target = new();
        public List<RuleCondition> conditions = new();

        public bool Matches(RuleEvent e, RuleContext ctx)
        {
            if (e.Action != action) return false;
            if (!target.Matches(e.Target, e.Floor)) return false;
            foreach (var c in conditions) if (!c.Evaluate(ctx)) return false;
            return true;
        }
    }

    /// ----------- 대상 필터 -----------
    [Serializable]
    public class TargetFilter
    {
        public TargetKind kind = TargetKind.Any;
        public int specificId;                       // 비었으면 무시
        public List<string> requiredTags = new();       // 요구 태그(전부 포함되야 일치)
        public List<string> excludeTags = new();        // 제외 태그(하나라도 있으면 불일치)
        public bool restrictFloor;
        public int floor;

        public bool Matches(ITargetable t, int eventFloor)
        {
            if (restrictFloor && eventFloor != floor) return false;
            if (t == null) return kind == TargetKind.Any && specificId != 0 && requiredTags.Count == 0;
            if (kind != TargetKind.Any && t.Kind != kind) return false;
            if (specificId != 0 && t.Id != specificId) return false;
            foreach (var tag in requiredTags) if (!Has(t, tag)) return false;
            foreach (var tag in excludeTags) if (Has(t, tag)) return false;
            return true;
        }
        private static bool Has(ITargetable t, string tag) 
        { foreach (var x in t.Tags) if (x == tag) return true; return false; }
    }

    // ----------- 조건 -----------
    [Serializable]
    public class RuleCondition
    {
        public enum Kind { Stat, WeightRatio, FlagOn, FlagOff }
        public Kind kind;
        public StatType stat;
        public Comparison comparison;
        public float value;
        public string flag;

        public bool Evaluate(RuleContext c)
        {
            switch (kind)
            {
                //case kind.stat:         return compare(c.stats.get(stat), comparison, value);
                //case kind.weightratio:  return compare(c.inventory.weightratio, comparison, value);
                //case kind.flagon:       return c.world.has(flag);
                //case kind.flagoff:      return !c.world.has(flag);
            }
            return true;
        }
        private static bool Compare(float a, Comparison op, float b) => op switch
        {
            Comparison.Less => a < b,
            Comparison.LessOrEqual => a <= b,
            Comparison.Greater => a > b,
            _ => a >= b
        };
    }

    // ----------- 월드 상태 & 컨텍스트 -----------
    /// 공간/환경 상태 플래그
    public class WorldState
    {
        private readonly HashSet<string> flags = new();
        public bool Has(string f) => flags.Contains(f);
        public void Set(string f, bool on) { if (on) flags.Add(f); else flags.Remove(f); }
        public void Clear() => flags.Clear();
    }

    /// 효과/조건이 접근할 게임 상태 집합
    public class RuleContext
    {
        public PlayerStats Stats;
        public GridInventory Inventory;
        public PlayerController Player;
        public WorldState World;
        public MonoBehaviour Runner;            // 코루틴 실행용
        //public int Floor => Player.CurrentFloor;
    }
}
