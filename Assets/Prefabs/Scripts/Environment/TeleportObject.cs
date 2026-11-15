using UnityEngine;
using Ju.Player;

namespace Ju.Environment
{
    public class TeleportObject : Interactable
    {
        [SerializeField] private Transform destination;
        [SerializeField] private int destinationFloor;

        protected override void OnInteract(PlayerController player)
        {
            if (destination == null)
            {
                Debug.LogWarning("[TeleportObject] destination πÃ¡ˆ¡§.");
                return;
            }

            RaiseInteracted();
            player.TeleportTo(destination.position, destinationFloor);
            //MarkUsed();
        }
    }
}