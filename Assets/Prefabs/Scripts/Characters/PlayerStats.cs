using System;
using System.Collections.Generic;
using UnityEngine;
using RuleSystem;

namespace Player
{
    [Serializable]
    public class StatEntry
    {
        public StatType type;
        public float max = 100f;
        public float value = 100f;
        [Tooltip("초당 변화량")]
        public float changePerSecond;
    }

    /// 생존 스탯. 무게가 스태미나에 영향을 줌 (그 외는 추가 논의)
    public class PlayerStats : MonoBehaviour
    {
        [SerializeField] List<StatEntry> entries = new(5);
        [SerializeField] GridInventory inventory;
        private readonly Dictionary<StatType, StatEntry> map = new();

        public event Action<StatType, float> Changed;
        public event Action Died;
        public bool IsDead { get; private set; }

        private void Awake()
        {
            foreach (var e in entries) map[e.type] = e;
        }

        private void Update()
        {
            if (IsDead) return;
            foreach (var e in entries)
                if (e.changePerSecond != 0f) Modify(e.type, e.changePerSecond * Time.deltaTime);

            //if (inventory != null && inventory.WeightRatio > 1f)
            //    Modify(StatType.Stamina, (inventory.WeightRatio - 1f) * 5f * Time.deltaTime);

            // 스탯 관련 추가 규칙 논의 시 작성
        }

        private void Reset()
        {
            entries = new List<StatEntry>
            {
                new() { type = StatType.Hunger, value = 100, changePerSecond = -0.12f },
                new() { type = StatType.Thirst, value = 100, changePerSecond = -0.1f },
                new() { type = StatType.Stamina, value = 100 },
                new() { type = StatType.Weight, value = 0 },
                new() { type = StatType.Stress, value = 0, changePerSecond = 0.1f }
            };
        }

        /// 스탯 타입에 따른 값 구하기
        public float Get(StatType t) => map.TryGetValue(t, out var e) ? e.value : 0f;

        public void Modify(StatType t, float delta)
        {
            if (IsDead || !map.TryGetValue(t, out var e)) return;
            e.value = Mathf.Clamp(e.value + delta, 0f, e.max);
            Changed?.Invoke(t, e.value);
            // 사망 조건: 현재는 배고픔이 0보다 작으면 사망으로 설정 (변경 가능성 있음)
            if (t == StatType.Hunger && e.value <= 0f) { IsDead = true; Died?.Invoke(); }
        }

        //public float MoveSpeedMultiplier =>
        //    inventory == null ? 1f : Mathf.Lerp(1f, 0.4f, Mathf.InverseLerp(0.7f, 1.2f, inventory.WeightRatio));
    }
}
