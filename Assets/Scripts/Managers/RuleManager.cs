using System;
using System.Collections.Generic;
using UnityEngine;
using MainSystem;

namespace RuleSystem
{
    public class ActiveRule
    {
        public RuleDefinition Def;
        public bool Pending;
        public float Remaining;
        public RuleEvent Activator;
    }

    public class RuleManager : MonoBehaviour
    {
        [SerializeField] RuleDatabase database;
        [SerializeField] int ruleCount = 3;
        [SerializeField] PlayerStats stats;
        [SerializeField] GridInventory inventory;
        [SerializeField] PlayerController player;

        public RuleContext Context { get; private set; }
        public IReadOnlyList<ActiveRule> ActiveRules => active;

        // UI 연결용
        public event Action<IReadOnlyList<ActiveRule>> RulesDrawn;
        public event Action<ActiveRule> RuleViolated, RuleFulfilled, ObligationStarted;

        private readonly List<ActiveRule> active = new();
        private readonly Queue<RuleEvent> queue = new();        // 이벤트 재진입 방지용

        private void Awake()
        {
            Context = new RuleContext
            {
                Stats = stats,
                Inventory = inventory,
                Player = player,
                World = new WorldState(),
                Runner = this
            };
        }
        private void OnEnable()     => EventBus<RuleEvent>.Subscribe(queue.Enqueue);
        private void OnDisable()    => EventBus<RuleEvent>.UnSubscribe(queue.Enqueue);
        private void Start()        => StartNewRun(0);

        private void Update()
        {
            while (queue.Count > 0) Process(queue.Dequeue());

            // Obligation 타이머
            foreach (var r in active)
            {
                if (!r.Pending) continue;
                r.Remaining -= Time.deltaTime;
                if (r.Remaining > 0) continue;
                r.Pending = false;
                if (r.Def.veracity == RuleVeracity.Truthful) Violate(r, r.Activator);
            }
        }

        /// 새 게임 시작: 규칙 n개 추첨, 상태 초기화. seed 0 = 랜덤
        public void StartNewRun(int seed)
        {
            active.Clear(); queue.Clear(); Context.World.Clear();
            var rng = seed == 0 ? new System.Random() : new System.Random(seed);
            foreach (var def in Draw(rng, ruleCount)) active.Add(new ActiveRule { Def = def });
            RulesDrawn?.Invoke(active);
        }

        /// 가중치로 규칙 추첨 + exclusiveGroup 충돌 방지
        private List<RuleDefinition> Draw(System.Random rng, int count)
        {
            var pool = new List<RuleDefinition>(database.rules.FindAll(r => r.weight > 0));
            var result = new List<RuleDefinition>();
            var usedGroups = new HashSet<char?>();
            while (result.Count < count && pool.Count > 0)
            {
                float total = 0;
                foreach (var r in pool) total += r.weight;
                float roll = (float)rng.NextDouble() * total;
                int idx = 0;
                for (; idx < pool.Count - 1; idx++) { roll -= pool[idx].weight; if (roll <= 0) break; }
                var pick = pool[idx]; pool.RemoveAt(idx);
                if (pick.exclusiveGroup == null && !usedGroups.Add(pick.exclusiveGroup)) continue;
                result.Add(pick);
            }
            return result;
        }

        private void Process(RuleEvent e)
        {
            foreach (var r in active)
            {
                var d = r.Def;
                if (d.mode == RuleMode.Prohibition)
                {
                    if (d.trigger.Matches(e, Context) && d.veracity == RuleVeracity.Truthful) Violate(r, e);
                }
                else
                {
                    if (r.Pending && d.response.Matches(e, Context))
                    {
                        r.Pending = false;
                        if (d.veracity == RuleVeracity.Deceptive) Violate(r, e);
                        //else RuleFulfilled(r, e);
                    }
                    else if (!r.Pending && d.trigger.Matches(e, Context))
                    {
                        r.Pending = true; r.Remaining = d.deadline; r.Activator = e;
                        ObligationStarted?.Invoke(r);
                    }
                }
            }
        }

        void Violate(ActiveRule r, RuleEvent e) { foreach (var fx in r.Def.onViolation) fx.Apply(Context, e); RuleViolated?.Invoke(r); }
        //void Fulfill(ActiveRule r, RuleEvent e) { foreach (var fx in r.Def.onFulfilled) fx.Apply(Context, e); RuleFulfilled?.Invoke(r); }
    }

}
