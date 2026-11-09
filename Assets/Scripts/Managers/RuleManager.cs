using System;
using System.Collections.Generic;
using UnityEngine;

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

    }

}
