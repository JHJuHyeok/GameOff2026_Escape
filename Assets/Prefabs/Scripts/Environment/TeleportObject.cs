using UnityEngine;
using Ju.Player;

namespace Ju.Environment
{
    public class TeleportObject : Interactable
    {
        [SerializeField] private Transform destination;
        [SerializeField] private int destinationFloor;

        [Tooltip("이 텔레포터로 도착할 때 플레이어가 놓일 위치. null일 경우 자기 자신의 위치.")]
        [SerializeField] private Transform arrivalPoint;

        public Transform ArrivalPoint => arrivalPoint != null ? arrivalPoint : transform;
        public bool IsLinked => destination != null;

        public void Link(Transform dest, int floor)
        {
            destination = dest;
            destinationFloor = floor;
        }

        public void UnLink()
        {
            destination = null;
            destinationFloor = 0;
        }

        protected override void OnInteract(PlayerController player)
        {
            if (destination == null)
            {
                Debug.LogWarning("[TeleportObject] destination 미지정.");
                return;
            }

            RaiseInteracted();
            player.TeleportTo(destination.position, destinationFloor);
            //MarkUsed();
        }
    }
}