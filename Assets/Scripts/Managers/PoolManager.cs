using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainSystem
{
    /// <summary>풀에서 꺼내질 때/반환될 때 상태를 초기화하고 싶은 오브젝트가 구현 (선택 사항)</summary>
    public interface IPoolable
    {
        void OnSpawn();     // 풀에서 꺼내져 활성화된 직후
        void OnDespawn();   // 풀로 반환되어 비활성화되기 직전
    }

    /// <summary>
    /// 프리팹 기반 범용 오브젝트 풀. 투사체, 이펙트, 적, 데미지 텍스트 등에 사용.
    ///
    /// 사용 예:
    ///   var bullet = PoolManager.Instance.Spawn(bulletPrefab, firePoint.position, firePoint.rotation);
    ///   PoolManager.Instance.Despawn(bullet);
    ///   PoolManager.Instance.DespawnAfter(effect, 1.5f);
    ///   PoolManager.Instance.Prewarm(bulletPrefab, 30);   // 첫 사용 시 끊김 방지
    ///
    /// 주의:
    ///  - 풀 관리 대상이 아닌 오브젝트를 Despawn하면 그냥 Destroy 된다.
    ///  - DespawnAfter 예약 후 그 오브젝트가 먼저 반환되어 재사용되면 예약이 새 사용분을 반환시킬 수 있다.
    ///    (짧은 수명의 이펙트/투사체 용도로 쓰는 것을 권장)
    /// </summary>
    public class PoolManager : Singleton<PoolManager>
    {
        private readonly Dictionary<GameObject, Stack<GameObject>> _pools = new Dictionary<GameObject, Stack<GameObject>>();
        private readonly Dictionary<int, GameObject> _prefabOf = new Dictionary<int, GameObject>(); // 인스턴스ID → 프리팹

        // ---------------- 꺼내기 ----------------

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null)
            {
                Debug.LogError("[PoolManager] Spawn할 프리팹이 null입니다.");
                return null;
            }

            Stack<GameObject> stack = GetStack(prefab);

            GameObject obj = null;
            while (stack.Count > 0 && obj == null)
                obj = stack.Pop(); // 씬 전환 등으로 파괴된 항목은 건너뜀

            if (obj == null)
                obj = CreateInstance(prefab);

            Transform t = obj.transform;
            t.SetParent(parent, false);
            t.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);

            if (obj.TryGetComponent(out IPoolable poolable))
                poolable.OnSpawn();

            return obj;
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Transform parent = null)
            => Spawn(prefab, position, Quaternion.identity, parent);

        /// <summary>컴포넌트 타입으로 바로 받기: var b = Spawn(bulletPrefab, pos, rot);</summary>
        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
        {
            GameObject obj = Spawn(prefab.gameObject, position, rotation, parent);
            return obj != null ? obj.GetComponent<T>() : null;
        }

        // ---------------- 반환 ----------------

        public void Despawn(GameObject obj)
        {
            if (obj == null) return;

            if (!_prefabOf.TryGetValue(obj.GetInstanceID(), out GameObject prefab))
            {
                Destroy(obj); // 풀 관리 대상이 아님
                return;
            }

            if (!obj.activeSelf) return; // 이미 반환된 오브젝트

            if (obj.TryGetComponent(out IPoolable poolable))
                poolable.OnDespawn();

            obj.SetActive(false);
            obj.transform.SetParent(transform, false);
            GetStack(prefab).Push(obj);
        }

        public void DespawnAfter(GameObject obj, float delay)
        {
            if (obj != null) StartCoroutine(DespawnRoutine(obj, delay));
        }

        private IEnumerator DespawnRoutine(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (obj != null && obj.activeSelf) Despawn(obj);
        }

        // ---------------- 관리 ----------------

        /// <summary>미리 count개를 만들어 풀에 넣어 둔다</summary>
        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null) return;

            Stack<GameObject> stack = GetStack(prefab);
            for (int i = 0; i < count; i++)
            {
                GameObject obj = CreateInstance(prefab);
                obj.SetActive(false);
                stack.Push(obj);
            }
        }

        /// <summary>풀에 보관 중인 모든 오브젝트 삭제 (스테이지 전환 시 등)</summary>
        public void ClearAll()
        {
            foreach (Stack<GameObject> stack in _pools.Values)
            {
                while (stack.Count > 0)
                {
                    GameObject obj = stack.Pop();
                    if (obj != null) Destroy(obj);
                }
            }
            _pools.Clear();
            _prefabOf.Clear();
        }

        private Stack<GameObject> GetStack(GameObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out Stack<GameObject> stack))
            {
                stack = new Stack<GameObject>();
                _pools[prefab] = stack;
            }
            return stack;
        }

        private GameObject CreateInstance(GameObject prefab)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.name = prefab.name;
            _prefabOf[obj.GetInstanceID()] = prefab;
            return obj;
        }
    }
}
