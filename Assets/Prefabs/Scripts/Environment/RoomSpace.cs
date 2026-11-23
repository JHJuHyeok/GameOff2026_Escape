using System.Collections.Generic;
using UnityEngine;
using Ju.RuleSystem;
using Ju.Player;

namespace Ju.Environment
{
    public class RoomSpace : MonoBehaviour, ITargetable
    {
        /// 최소로 켜져 있어야 하는 ContainerObject 수
        private const int MinActiveContainers = 2;

        [SerializeField] private int id;
        [SerializeField] private int floor;

        [Header("레이아웃")]
        [Tooltip("BoxCollider2D가 없을 때 사용할 방 크기.")]
        [SerializeField] private Vector2 fallbackSize = new(16f, 9f);
        [Tooltip("플레이어 시작 위치. 튜토리얼(시작) 방에서만 사용.")]
        [SerializeField] private Transform spawnPoint;

        public int Id => id;
        public TargetKind Kind => TargetKind.Space;
        public int Floor => floor;

        /// 방 크기. BoxCollider2D 기준, 없으면 fallbackSize 사용
        public Vector2 Size
        {
            get
            {
                if (TryGetComponent(out BoxCollider2D box))
                {
                    Vector3 s = transform.lossyScale;
                    return new Vector2(box.size.x * Mathf.Abs(s.x), box.size.y * Mathf.Abs(s.y));
                }
                return fallbackSize;
            }
        }

        public Vector2 LocalCenterOffset
        {
            get
            {
                if (TryGetComponent(out BoxCollider2D box))
                    return Vector2.Scale(box.offset, (Vector2)transform.lossyScale);
                return Vector2.zero;
            }
        }

        public Vector3 WorldCenter => transform.position + (Vector3)LocalCenterOffset;
        public Vector3 SpawnPosition => spawnPoint != null ? spawnPoint.position : WorldCenter;

        public void SetFloor(int value) => floor = value;

        public TeleportObject[] FindTeleporters() => GetComponentsInChildren<TeleportObject>(true);
        public ContainerObject[] FindContainers() => GetComponentsInChildren<ContainerObject>(true);

        /// ContainerObject가 3개 이상이면 랜덤하게 2개 이상(최대 전부) 활성화
        /// 3개 미만이면 전부 활성 상태 유지
        public void ApplyRandomContainers(System.Random rng)
        {
            var containers = FindContainers();
            foreach (var c in containers) c.enabled = true;
            if (containers.Length <= MinActiveContainers) return;

            int activeCount = rng.Next(MinActiveContainers, containers.Length + 1);

            // 셔플 후 activeCount만큼 유지
            for (int i = containers.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (containers[i], containers[j]) = (containers[j], containers[i]);
            }
            for (int i = activeCount; i < containers.Length; i++)
                containers[i].enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.TryGetComponent(out PlayerController p)) return;
            p.CurrentSpace = this;
            PlayerController.Emit(ActionType.Enter, this, floor);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.TryGetComponent(out PlayerController p)) return;
            if (p.CurrentSpace == this) p.CurrentSpace = null;
            PlayerController.Emit(ActionType.Exit, this, floor);
        }
    }
}
