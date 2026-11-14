using System;
using System.Collections.Generic;
using UnityEngine;
using RuleSystem;
using Player;

namespace Environment
{
    [Serializable] public struct LootEntry { public ItemData item; [Range(0, 1)] public float chance; }

    [RequireComponent(typeof(Collider2D))]
    public class Interactable : MonoBehaviour, ITargetable
    {
        [SerializeField] private int id;
        [SerializeField] private List<string> tags = new();
        [SerializeField] private List<LootEntry> loot = new();
        [SerializeField] private bool singleUse = true;

        private bool used;
        public virtual bool CanInteract => !(singleUse && used);

        public int Id => id;
        public virtual TargetKind Kind => TargetKind.Object;

        public virtual void Interact(PlayerController player)
        {
            used = true;
            //PlayerController.Emit(ActionType.Interact, this);

            //foreach (var entry in loot)
            //    if (UnityEngine.Random.value <= entry.chance)
            //        player.Inventory.TryAdd(entry.item, out _);
        }
    }
}
