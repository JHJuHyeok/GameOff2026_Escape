using System.Collections.Generic;

namespace RuleSystem
{
    // ----------- 기본 열거형 -----------   : 추가 및 제거 여부는 차후 결정
    /// 규칙의 '행동'. 게임 시스템이 발행하는 모든 사건의 종류
    public enum ActionType { PickUp, Drop, Use, Interact, Enter, Exit, Appear, Attack, ChangeFloor }

    /// 규칙의 '대상'. 적대 존재 / 아이템 / 배치 오브젝트 / 공간 등
    public enum TargetKind { Any, Player, Entity, Item, Object, Space }

    public enum StatType { Hunger, Thirst, Stamina, Stress, Weight }
    public enum Comparison { Less, LessOrEqual, Greater, GreaterOrEqual }

    /// 규칙의 대상이 될 수 있는 모든 것에 상속시킬 것
    public interface ITargetable
    {
        string Id { get; }
        TargetKind Kind { get; }
        // 특정한 조건 명시 (예: 붉은 것, 무언가를 포함하는, 표식)
        IReadOnlyCollection<string> Tags { get; } 
    }
}
