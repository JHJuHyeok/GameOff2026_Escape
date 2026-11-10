using System;
using System.Collections.Generic;
using UnityEngine;
using RuleSystem;

namespace Environment
{
    public class Interactable : MonoBehaviour, ITargetable
    {
        [SerializeField] private int id;
        [SerializeField] private List<string> tags = new();
        //[SerializeField] private List<LootEntry> loot = new();
        [SerializeField] private bool singleUse = true;

        private bool used;
        public virtual bool CanInteract => !(singleUse && used);

        public int Id => id;
        public virtual TargetKind Kind => TargetKind.Object;
        public IReadOnlyCollection<string> Tags => tags;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
