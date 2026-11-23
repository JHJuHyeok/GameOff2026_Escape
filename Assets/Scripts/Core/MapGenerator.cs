using System;
using System.Collections.Generic;
using UnityEngine;
using MainSystem;
using UtilityTool;
using Ju.Player;

namespace Ju.Environment
{
    /// <summary>
    /// 게임 시작 시 맵 생성
    ///
    /// 규칙
    ///  1. 모든 층에는 계단 방이 반드시 존재.
    ///  2. 일반 방은 그룹(기본 3층) 안에서 랜덤 층에 배치된다.
    ///  3. 일반 연결점(Door)으로 전체 맵이 하나로 이어지고, 모든 방은 1개 이상의 연결을 가진다.
    ///  4. 연결되지 않은 TeleportObject는 SetActive(false).
    ///  5. ContainerObject가 3개 이상인 방은 2개 이상을 랜덤 활성화.
    ///  6. 시작(튜토리얼) 방은 첫 층에 배치되고, 플레이어는 그 방의 spawnPoint로 이동한다.
    /// </summary>
    public class MapGenerator : MonoBehaviour
    {
        [Header("Room Pool")]
        [Tooltip("튜토리얼(시작) 방")]
        [SerializeField] private RoomSpace startRoomPrefab;
        [Tooltip("계단이 포함된 방 프리팹. 층마다 하나 이상 배치")]
        [SerializeField] private List<RoomSpace> stairRoomPrefabs = new();
        [Tooltip("일반 방 프리팹")]
        [SerializeField] private List<RoomSpace> roomPrefabs = new();
        [Tooltip("일반 방 중복 사용 여부")]
        [SerializeField] private bool allowDuplicates = false;

        [Header("Structure")]
        [SerializeField, Min(1)] private int groupCount = 2;
        [Tooltip("한 그룹이 가지는 층 수")]
        [SerializeField, Min(1)] private int floorsPerGroup = 3;
        [Tooltip("가장 낮은 층 번호")]
        [SerializeField] private int firstFloor = 1;
        [Tooltip("그룹 하나의 총 방 개수 최소, 최대값")]
        [SerializeField] private Vector2Int roomsPerGroup = new(5, 7);
        [Tooltip("같은 그룹 방끼리 추가로 문을 연결할 확률")]
        [SerializeField, Range(0f, 1f)] private float extraLinkChance = 0.15f;
        [Tooltip("연결 계획에 실패했을 때 재시도 횟수")]
        [SerializeField, Min(1)] private int maxAttempts = 30;

        [Header("Layout")]
        [Tooltip("방 사이 간격을 카메라 시야 크기에서 자동 확보. null일 시 Camera.main 사용")]
        [SerializeField] private Camera viewCamera;
        [Tooltip("카메라 시야 절반에 더해질 여유 간격")]
        [SerializeField, Min(0f)] private float viewMargin = 2f;
        [SerializeField, Min(0f)] private float horizontalGap = 4f;
        [Tooltip("층 사이 여백")]
        [SerializeField, Min(0f)] private float verticalGap = 4f;
        [Tooltip("0보다 크면 층 간격을 이 값으로 고정")]
        [SerializeField, Min(0f)] private float fixedFloorPitch = 0f;

        [Header("Seed")]
        [SerializeField] private bool useRandomSeed = true;
        [SerializeField] private int seed;

        [Header("Runtime")]
        [SerializeField] private bool generateOnStart = true;
        [Tooltip("지정하면 생성 후 시작 방의 spawnPoint로 이동")]
        [SerializeField] private PlayerController player;
        [Tooltip("켜면 방 생성/반환에 PoolManager를 사용한다.")]
        [SerializeField] private bool usePooling = false;

        public bool IsGenerated { get; private set; }
        public int CurrentSeed { get; private set; }
        public RoomSpace StartRoom { get; private set; }
        public IReadOnlyList<RoomSpace> Rooms => rooms;
        public event Action Generated;

        #region ---------------- 내부 데이터 ----------------
        private sealed class PortInfo
        {
            public readonly List<int> Walls = new();
            public readonly List<int> Ups = new();
            public readonly List<int> Downs = new();
            public readonly List<int> Lefts = new();
            public readonly List<int> Rights = new();
            public int DoorCount => Walls.Count + Lefts.Count + Rights.Count;
        }

        private sealed class Node
        {
            public int Index;
            public RoomSpace Prefab;
            public int Group;
            public int Floor;
            public bool IsStair;
            public bool IsStart;
            // 아직 연결되지 않은 TeleportObject 인덱스
            public readonly List<int> FreeWalls = new();
            public readonly List<int> FreeUps = new();
            public readonly List<int> FreeDowns = new();
            public readonly List<int> FreeLefts = new();
            public readonly List<int> FreeRights = new();
            public RoomSpace Instance;
        }

        private sealed class Edge
        {
            public Node A, B;
            public int PortA, PortB;
            public bool Stair;
        }

        private sealed class Plan
        {
            public List<Node> Nodes;
            public List<Edge> Edges;
        }
        #endregion

        private readonly Dictionary<RoomSpace, PortInfo> portCache = new();
        private readonly List<RoomSpace> rooms = new();
        private readonly List<(RoomSpace a, RoomSpace b, bool stair)> debugLinks = new();
        private RoomSpace startPrefab;
        private Transform root;
        private bool usedPool;

        private void Start()
        {
            if (generateOnStart) Generate();
        }

        // ---------------- 진입점 ----------------
        /// 맵을 생성한다. 게임 시작 시 한 번만 동작하며, 이미 생성되었다면 무시한다.
        public bool Generate()
        {
            if (IsGenerated)
            {
                Debug.LogWarning("[MapGenerator] 맵이 이미 생성되었습니다.", this);
                return false;
            }
            Clear();

            portCache.Clear();
            startPrefab = ValidateStart();
            var pool = BuildRoomPool();
            var stairPool = BuildStairPool();

            if (!ValidateConfig(pool, stairPool)) return false;

            CurrentSeed = useRandomSeed ? unchecked((int)DateTime.Now.Ticks) : seed;
            var rng = new System.Random(CurrentSeed);

            Plan plan = null;
            for (int attempt = 0; attempt < maxAttempts && plan == null; attempt++)
                plan = TryPlan(rng, pool, stairPool);

            if (plan == null)
            {
                Debug.LogError($"[MapGenerator] {maxAttempts}회 동안 모든 방을 연결하지 못했습니다. " +
                               "방 갯수를 줄이거나 텔레포터를 늘려야 합니다.", this);
                return false;
            }

            Build(rng, plan);
            IsGenerated = true;

            int stairEdges = plan.Edges.FindAll(e => e.Stair).Count;
            Debug.Log($"[MapGenerator] 생성 완료 seed={CurrentSeed}, 방 {plan.Nodes.Count}개, " +
                      $"계단 연결 {stairEdges}개, 문 연결 {plan.Edges.Count - stairEdges}개");

            if (player != null && StartRoom != null)
                player.TeleportTo(StartRoom.SpawnPosition, StartRoom.Floor);

            Generated?.Invoke();
            return true;
        }

        /// 생성된 맵 제거. 게임 종료/메뉴 복귀 등에서만 사용. (풀링 사용 시 PoolManager로 반환)
        public void Clear()
        {
            if (root == null)
            {
                var leftover = transform.Find("GeneratedMap");
                if (leftover != null) root = leftover;
            }
            else
            {
                if (usedPool && PoolManager.Instance != null)
                    foreach (var r in rooms)
                        if (r != null) PoolManager.Instance.Despawn(r.gameObject);

                if (Application.isPlaying) Destroy(root.gameObject);
                else DestroyImmediate(root.gameObject);
            }

            root = null;
            rooms.Clear();
            debugLinks.Clear();
            StartRoom = null;
            usedPool = false;
            IsGenerated = false;
        }

        [ContextMenu("재생성 (에디터 테스트용)")]
        private void RegenerateForTest()
        {
            Clear();
            Generate();
        }

        // ---------------- 검증 / 풀 구성 ----------------
        private PortInfo Inspect(RoomSpace prefab)
        {
            if (portCache.TryGetValue(prefab, out var info)) return info;

            info = new PortInfo();
            var ports = prefab.FindTeleporters();
            for (int i = 0; i < ports.Length; i++)
            {
                switch (ports[i].Role)
                {
                    case TeleportRole.StairUp:      info.Ups.Add(i); break;
                    case TeleportRole.StairDown:    info.Downs.Add(i); break;
                    case TeleportRole.LeftSide:     info.Lefts.Add(i); break;
                    case TeleportRole.RightSide:    info.Rights.Add(i); break;
                    default:                        info.Walls.Add(i); break;
                }
            }
            portCache[prefab] = info;
            return info;
        }

        private RoomSpace ValidateStart()
        {
            if (startRoomPrefab == null)
            {
                Debug.LogWarning("[MapGenerator] startRoomPrefab이 비어 있습니다.", this);
                return null;
            }
            if (Inspect(startRoomPrefab).DoorCount == 0)
            {
                Debug.LogWarning("[MapGenerator] 시작 방에 문이 없어 제외합니다.", startRoomPrefab);
                return null;
            }
            return startRoomPrefab;
        }

        private List<RoomSpace> BuildRoomPool()
        {
            var pool = new List<RoomSpace>();
            foreach (var prefab in roomPrefabs)
            {
                if (prefab == null || prefab == startPrefab) continue;
                if (Inspect(prefab).DoorCount == 0)
                {
                    Debug.LogWarning($"[MapGenerator] '{prefab.name}'에 문이 없어 제외합니다.", prefab);
                    continue;
                }
                pool.Add(prefab);
            }
            return pool;
        }

        private List<RoomSpace> BuildStairPool()
        {
            var pool = new List<RoomSpace>();
            foreach (var prefab in stairRoomPrefabs)
            {
                if (prefab == null) continue;
                if (prefab == startPrefab)
                {
                    Debug.LogWarning("[MapGenerator] 시작 방 프리팹이 stairRoomPrefabs에도 등록되어 있어 제외합니다.", prefab);
                    continue;
                }
                var info = Inspect(prefab);
                if (info.Ups.Count + info.Downs.Count == 0)
                {
                    Debug.LogWarning($"[MapGenerator] '{prefab.name}'에 StairUp/StairDown 텔레포터가 없어 제외합니다.", prefab);
                    continue;
                }
                pool.Add(prefab);
            }
            return pool;
        }

        private bool ValidateConfig(List<RoomSpace> pool, List<RoomSpace> stairPool)
        {
            if (groupCount * floorsPerGroup > 1)
            {
                bool anyUp = stairPool.Exists(p => Inspect(p).Ups.Count > 0);
                bool anyDown = stairPool.Exists(p => Inspect(p).Downs.Count > 0);
                if (!anyUp || !anyDown)
                {
                    Debug.LogError("[MapGenerator] StairUp 계단과 StairDown 계단이 모두 필요합니다. " +
                                   "stairRoomPrefabs와 TeleportObject의 Role 설정을 확인하세요.", this);
                    return false;
                }
            }
            if (pool.Count == 0 && stairPool.Count == 0 && startPrefab == null)
            {
                Debug.LogError("[MapGenerator] 사용 가능한 RoomSpace 프리팹이 없습니다.", this);
                return false;
            }
            return true;
        }

        // ---------------- 계획 (인스턴스 생성 전, 프리팹 정보만 사용) ----------------
        private Plan TryPlan(System.Random rng, List<RoomSpace> pool, List<RoomSpace> stairPool)
        {
            var nodes = new List<Node>();
            var edges = new List<Edge>();
            var linked = new HashSet<(int, int)>();
            int lastFloor = firstFloor + groupCount * floorsPerGroup - 1;

            var bag = new List<RoomSpace>(pool);
            ListExtensions.Shuffle(bag, rng);

            RoomSpace Draw()
            {
                if (bag.Count == 0 && allowDuplicates && pool.Count > 0)
                {
                    bag.AddRange(pool);
                    ListExtensions.Shuffle(bag, rng);
                }
                if (bag.Count == 0) return null;
                var r = bag[^1];
                bag.RemoveAt(bag.Count - 1);
                return r;
            }

            Node NewNode(RoomSpace prefab, int group, int floor, bool isStair)
            {
                var info = Inspect(prefab);
                var n = new Node { Index = nodes.Count, Prefab = prefab, Group = group, Floor = floor, IsStair = isStair };
                n.FreeWalls.AddRange(info.Walls);
                n.FreeUps.AddRange(info.Ups);
                n.FreeDowns.AddRange(info.Downs);
                n.FreeLefts.AddRange(info.Lefts);
                n.FreeRights.AddRange(info.Rights);
                nodes.Add(n);
                return n;
            }

            var groups = new List<List<Node>>();
            for (int g = 0; g < groupCount; g++) groups.Add(new List<Node>());
            var stairsByFloor = new Dictionary<int, List<Node>>();

            // 1. 모든 층에 계단 방 배치
            for (int f = firstFloor; f <= lastFloor; f++)
            {
                int g = (f - firstFloor) / floorsPerGroup;
                var picks = PickStairPrefabs(rng, stairPool, f < lastFloor, f > firstFloor);
                var list = new List<Node>();
                foreach (var prefab in picks)
                {
                    var n = NewNode(prefab, g, f, true);
                    groups[g].Add(n);
                    list.Add(n);
                }
                stairsByFloor[f] = list;
            }

            // 2. 시작 방 + 일반 방 배치
            int minRooms = Mathf.Max(1, roomsPerGroup.x);
            int maxRooms = Mathf.Max(minRooms, roomsPerGroup.y);

            for (int g = 0; g < groupCount; g++)
            {
                var list = groups[g];
                int want = rng.Next(minRooms, maxRooms + 1) - list.Count;

                if (g == 0 && startPrefab != null)
                {
                    var startNode = NewNode(startPrefab, g, firstFloor, false);
                    startNode.IsStart = true;
                    list.Add(startNode);
                    want--;
                }

                for (int i = 0; i < want; i++)
                {
                    var prefab = Draw();
                    if (prefab == null) break;
                    int floor = firstFloor + g * floorsPerGroup + rng.Next(floorsPerGroup);
                    list.Add(NewNode(prefab, g, floor, false));
                }
            }

            // 연결 상태 추적
            var uf = new int[nodes.Count];
            for (int i = 0; i < uf.Length; i++) uf[i] = i;
            int Find(int x)
            {
                while (uf[x] != x) { uf[x] = uf[uf[x]]; x = uf[x]; }
                return x;
            }
            void Union(Node a, Node b) => uf[Find(a.Index)] = Find(b.Index);

            // 3. 계단 연결
            for (int f = firstFloor; f < lastFloor; f++)
            {
                var ups = new List<(Node node, int port)>();
                var downs = new List<(Node node, int port)>();
                foreach (var n in stairsByFloor[f]) foreach (int p in n.FreeUps) ups.Add((n, p));
                foreach (var n in stairsByFloor[f + 1]) foreach (int p in n.FreeDowns) downs.Add((n, p));
                ListExtensions.Shuffle(ups, rng);
                ListExtensions.Shuffle(downs, rng);

                int count = Math.Min(ups.Count, downs.Count);
                for (int i = 0; i < count; i++)
                {
                    var (a, pa) = ups[i];
                    var (b, pb) = downs[i];
                    a.FreeUps.Remove(pa);
                    b.FreeDowns.Remove(pb);
                    AddEdge(a, pa, b, pb, true, edges, linked);
                }
            }

            // 4. 같은 층끼리 하나의 집합으로 연결
            var byFloor = new Dictionary<int, List<Node>>();
            foreach (var n in nodes)
            {
                if (!byFloor.TryGetValue(n.Floor, out var fl)) byFloor[n.Floor] = fl = new List<Node>();
                fl.Add(n);
            }

            foreach (var floorNodes in byFloor.Values)
            {
                int components = floorNodes.Count;
                while (components > 1)
                {
                    var candidates = CollectPairs(floorNodes, Find);
                    if (candidates.Count == 0) return null;

                    var (a, b) = candidates[rng.Next(candidates.Count)];
                    ConnectDoors(a, b, rng, edges, linked);
                    Union(a, b);
                    components--;
                }
            }

            // 5. 추가 연결
            if (extraLinkChance > 0f)
            {
                foreach (var floorNodes in byFloor.Values)
                {
                    for (int i = 0; i < floorNodes.Count; i++)
                        for (int j = i + 1; j < floorNodes.Count; j++)
                        {
                            var a = floorNodes[i];
                            var b = floorNodes[j];
                            if (!CanConnect(a, b)) continue;
                            if (linked.Contains(PairKey(a, b))) continue;
                            if (rng.NextDouble() >= extraLinkChance) continue;
                            ConnectDoors(a, b, rng, edges, linked);
                        }
                }
            }

            return nodes.Count > 0 ? new Plan { Nodes = nodes, Edges = edges } : null;
        }

        /// 한 층에 필요한 계단 방 선택
        private List<RoomSpace> PickStairPrefabs(System.Random rng, List<RoomSpace> stairPool, bool needUp, bool needDown)
        {
            var result = new List<RoomSpace>();
            if (!needUp && !needDown) return result;

            bool HasUp(RoomSpace p) => Inspect(p).Ups.Count > 0;
            bool HasDown(RoomSpace p) => Inspect(p).Downs.Count > 0;

            var exact = stairPool.FindAll(p => HasUp(p) == needUp && HasDown(p) == needDown);
            if (exact.Count > 0) { result.Add(exact[rng.Next(exact.Count)]); return result; }

            var full = stairPool.FindAll(p => (!needUp || HasUp(p)) && (!needDown || HasDown(p)));
            if (full.Count > 0) { result.Add(full[rng.Next(full.Count)]); return result; }

            if (needUp)
            {
                var ups = stairPool.FindAll(HasUp);
                result.Add(ups[rng.Next(ups.Count)]);
            }
            if (needDown)
            {
                var downs = stairPool.FindAll(HasDown);
                result.Add(downs[rng.Next(downs.Count)]);
            }
            return result;
        }

        private static bool CanConnect(Node a, Node b)
            => (a.FreeWalls.Count > 0 && b.FreeWalls.Count > 0)
            || (a.FreeLefts.Count > 0 && b.FreeRights.Count > 0)
            || (a.FreeRights.Count > 0 && b.FreeLefts.Count > 0);

        /// 같은 층에서 서로 다른 덩어리에 속하고 연결 가능한 방 쌍
        private static List<(Node, Node)> CollectPairs(List<Node> floorNodes, Func<int, int> find)
        {
            var result = new List<(Node, Node)>();
            for (int i = 0; i < floorNodes.Count; i++)
                for (int j = i + 1; j < floorNodes.Count; j++)
                {
                    var a = floorNodes[i];
                    var b = floorNodes[j];
                    if (find(a.Index) == find(b.Index)) continue;
                    if (!CanConnect(a, b)) continue;
                    result.Add((a, b));
                }
            return result;
        }

        private static (int, int) PairKey(Node a, Node b)
            => a.Index < b.Index ? (a.Index, b.Index) : (b.Index, a.Index);

        private static void AddEdge(Node a, int pa, Node b, int pb, bool stair, List<Edge> edges, HashSet<(int, int)> linked)
        {
            edges.Add(new Edge { A = a, B = b, PortA = pa, PortB = pb, Stair = stair });
            linked.Add(PairKey(a, b));
        }

        private static void ConnectDoors(Node a, Node b, System.Random rng, List<Edge> edges, HashSet<(int, int)> linked)
        {
            var options = new List<(List<int> fa, List<int> fb)>();
            if (a.FreeWalls.Count > 0 && b.FreeWalls.Count > 0) options.Add((a.FreeWalls, b.FreeWalls));
            if (a.FreeLefts.Count > 0 && b.FreeRights.Count > 0) options.Add((a.FreeLefts, b.FreeRights));
            if (a.FreeRights.Count > 0 && b.FreeLefts.Count > 0) options.Add((a.FreeRights, b.FreeLefts));

            var (fa, fb) = options[rng.Next(options.Count)];
            int ia = rng.Next(fa.Count);
            int ib = rng.Next(fb.Count);
            int pa = fa[ia];
            int pb = fb[ib];
            fa.RemoveAt(ia);
            fb.RemoveAt(ib);
            AddEdge(a, pa, b, pb, false, edges, linked);
        }

        // ---------------- 실제 생성 / 배치 / 연결 ----------------
        private void Build(System.Random rng, Plan plan)
        {
            root = new GameObject("GeneratedMap").transform;
            root.SetParent(transform, false);

            usedPool = usePooling && PoolManager.Instance != null;
            if (usePooling && !usedPool)
                Debug.LogWarning("[MapGenerator] PoolManager를 찾지 못해 일반 Instantiate로 생성합니다.", this);

            // 인스턴스 생성
            foreach (var n in plan.Nodes)
            {
                n.Instance = usedPool
                    ? PoolManager.Instance.Spawn(n.Prefab, Vector3.zero, Quaternion.identity, root)
                    : Instantiate(n.Prefab, root);

                n.Instance.name = $"{n.Prefab.name}_F{n.Floor}";
                n.Instance.SetFloor(n.Floor);
                rooms.Add(n.Instance);
                if (n.IsStart) StartRoom = n.Instance;
            }

            Layout(rng, plan.Nodes);

            // 텔레포터 초기화 (풀링 재사용 대비: 전부 켜고 연결 해제)
            var ports = new Dictionary<Node, TeleportObject[]>();
            foreach (var n in plan.Nodes)
            {
                var arr = n.Instance.FindTeleporters();
                foreach (var t in arr)
                {
                    t.gameObject.SetActive(true);
                    t.Unlink();
                }
                ports[n] = arr;
            }

            // 연결
            foreach (var e in plan.Edges)
            {
                var ta = ports[e.A][e.PortA];
                var tb = ports[e.B][e.PortB];
                ta.Link(tb.ArrivalPoint, e.B.Floor);
                tb.Link(ta.ArrivalPoint, e.A.Floor);
                debugLinks.Add((e.A.Instance, e.B.Instance, e.Stair));
            }

            // 연결되지 않은 텔레포터 비활성화
            int deadEndStairs = 0;
            foreach (var n in plan.Nodes)
            {
                foreach (var t in ports[n])
                {
                    if (t.IsLinked) continue;
                    if (t.Role == TeleportRole.StairUp || t.Role == TeleportRole.StairDown) deadEndStairs++;
                    t.gameObject.SetActive(false);
                }
            }
            if (deadEndStairs > 0)
                Debug.LogWarning($"[MapGenerator] 이어질 층이 없는 계단 {deadEndStairs}개 비활성화", this);

            // ContainerObject 랜덤 활성화
            foreach (var n in plan.Nodes)
                n.Instance.ApplyRandomContainers(rng);
        }

        /// 층마다 한 줄로 배치. 같은 층의 방들은 바닥 정렬, 가로 중앙 정렬.
        private void Layout(System.Random rng, List<Node> nodes)
        {
            float maxHeight = 0f;
            foreach (var n in nodes) maxHeight = Mathf.Max(maxHeight, n.Instance.Size.y);
            float gapX = horizontalGap;
            float gapY = verticalGap;
            var cam = viewCamera != null ? viewCamera : Camera.main;
            if (cam != null && cam.orthographic)
            {
                // 카메라 중심은 항상 현재 방 안에 있으므로, 시야 절반 + 여유만큼 떨어뜨려 생성
                float viewH = cam.orthographicSize * 2f;
                float viewW = viewH * cam.aspect;
                gapX = Mathf.Max(gapX, viewW * 0.5f + viewMargin);
                gapY = Mathf.Max(gapY, viewH * 0.5f + viewMargin);
            }
            float pitch = fixedFloorPitch > 0f ? fixedFloorPitch : maxHeight + gapY;

            var rows = new Dictionary<int, List<Node>>();
            foreach (var n in nodes)
            {
                if (!rows.TryGetValue(n.Floor, out var row)) rows[n.Floor] = row = new List<Node>();
                row.Add(n);
            }

            Vector3 origin = transform.position;
            foreach (var kv in rows)
            {
                var row = kv.Value;
                ListExtensions.Shuffle(row, rng);

                float totalWidth = gapX * (row.Count - 1);
                foreach (var n in row) totalWidth += n.Instance.Size.x;

                float x = -totalWidth * 0.5f;
                float floorY = (kv.Key - firstFloor) * pitch;

                foreach (var n in row)
                {
                    Vector2 size = n.Instance.Size;
                    Vector2 slotCenter = new(x + size.x * 0.5f, floorY + size.y * 0.5f);
                    Vector2 pos = slotCenter - n.Instance.LocalCenterOffset;
                    n.Instance.transform.position = origin + new Vector3(pos.x, pos.y, 0f);
                    x += size.x + gapX;
                }
            }
        }

        // ---------------- 유틸 ----------------
        private void OnValidate()
        {
            roomsPerGroup.x = Mathf.Max(1, roomsPerGroup.x);
            roomsPerGroup.y = Mathf.Max(roomsPerGroup.x, roomsPerGroup.y);
        }

        [ContextMenu("검증 (테스트용)")]
        private bool ValidateMap()
        {
            int errors = 0;
            void Fail(string msg, UnityEngine.Object ctx) { errors++; Debug.LogError("[Validate] " + msg, ctx); }
            bool IsStair(TeleportRole r) => r == TeleportRole.StairUp || r == TeleportRole.StairDown;

            int lastFloor = firstFloor + groupCount * floorsPerGroup - 1;
            var byFloor = new Dictionary<int, List<RoomSpace>>();

            foreach (var r in rooms)
            {
                if (!byFloor.TryGetValue(r.Floor, out var list)) byFloor[r.Floor] = list = new List<RoomSpace>();
                list.Add(r);

                int linkedCount = 0;
                foreach (var t in r.FindTeleporters())
                {
                    if (t.gameObject.activeSelf != t.IsLinked) Fail($"{r.name}/{t.name}: 활성 상태와 연결 상태 불일치", t);
                    if (!t.IsLinked) continue;
                    linkedCount++;

                    var other = t.Destination.GetComponentInParent<RoomSpace>();
                    var ot = t.Destination.GetComponentInParent<TeleportObject>();
                    if (other == null) { Fail($"{r.name}/{t.name}: 목적지 방을 찾을 수 없음", t); continue; }

                    switch (t.Role)
                    {
                        case TeleportRole.StairUp:
                            if (other.Floor != r.Floor + 1) Fail($"{r.name}: StairUp이 {other.Floor}층으로 연결됨", t); break;
                        case TeleportRole.StairDown:
                            if (other.Floor != r.Floor - 1) Fail($"{r.name}: StairDown이 {other.Floor}층으로 연결됨", t); break;
                        default:
                            if (other.Floor != r.Floor) Fail($"{r.name}: 문이 다른 층({other.Floor})으로 연결됨", t);
                            if (ot != null &&
                                ((t.Role == TeleportRole.LeftSide && ot.Role != TeleportRole.RightSide) ||
                                 (t.Role == TeleportRole.RightSide && ot.Role != TeleportRole.LeftSide) ||
                                 (t.Role == TeleportRole.WallDoor && ot.Role != TeleportRole.WallDoor)))
                                Fail($"{r.name}/{t.name}: 문 종류 규칙 위반 ({t.Role} → {ot.Role})", t);
                            break;
                    }
                }
                if (linkedCount == 0 && rooms.Count > 1) Fail($"{r.name}: 연결된 텔레포터가 없음", r);

                var containers = r.FindContainers();
                int active = 0;
                foreach (var c in containers) if (c.gameObject.activeSelf) active++;
                if (containers.Length >= 3 && active < 2) Fail($"{r.name}: ContainerObject {containers.Length}개 중 {active}개만 활성", r);
                if (containers.Length < 3 && active != containers.Length) Fail($"{r.name}: ContainerObject가 3개 미만인데 비활성화됨", r);
            }

            foreach (var kv in byFloor)
            {
                // 계단 존재
                bool up = false, down = false;
                foreach (var r in kv.Value)
                    foreach (var t in r.FindTeleporters())
                    {
                        if (!t.IsLinked) continue;
                        up |= t.Role == TeleportRole.StairUp;
                        down |= t.Role == TeleportRole.StairDown;
                    }
                if (kv.Key < lastFloor && !up) Fail($"{kv.Key}층: 위층 계단 없음", this);
                if (kv.Key > firstFloor && !down) Fail($"{kv.Key}층: 아래층 계단 없음", this);

                // 문만으로 같은 층이 하나의 집합인지 (BFS)
                var visited = new HashSet<RoomSpace> { kv.Value[0] };
                var queue = new Queue<RoomSpace>();
                queue.Enqueue(kv.Value[0]);
                while (queue.Count > 0)
                {
                    var cur = queue.Dequeue();
                    foreach (var t in cur.FindTeleporters())
                    {
                        if (!t.IsLinked || IsStair(t.Role)) continue;
                        var o = t.Destination.GetComponentInParent<RoomSpace>();
                        if (o != null && visited.Add(o)) queue.Enqueue(o);
                    }
                }
                if (visited.Count != kv.Value.Count) Fail($"{kv.Key}층: 문으로 연결되지 않은 방이 있음 ({visited.Count}/{kv.Value.Count})", this);
            }

            if (startRoomPrefab != null && StartRoom != null && StartRoom.Floor != firstFloor)
                Fail($"시작 방이 {StartRoom.Floor}층에 배치됨 (기대: {firstFloor}층)", this);

            Debug.Log($"[Validate] 완료: 방 {rooms.Count}개, 오류 {errors}건");
            return errors == 0;
        }

        [ContextMenu("반복 테스트 (시드 200회)")]
        private void StressTest()
        {
            bool prevRandom = useRandomSeed; int prevSeed = seed;
            useRandomSeed = false;
            int genFail = 0, invalid = 0;

            for (int i = 0; i < 200; i++)
            {
                seed = i;
                Clear();
                if (!Generate()) { genFail++; Debug.LogWarning($"[Stress] seed {i}: 생성 실패"); continue; }
                if (!ValidateMap()) { invalid++; Debug.LogWarning($"[Stress] seed {i}: 검증 실패"); }
            }

            Clear();
            useRandomSeed = prevRandom; seed = prevSeed;
            Debug.Log($"[Stress] 생성 실패 {genFail} / 검증 실패 {invalid} / 총 200");
        }

        private void OnDrawGizmosSelected()
        {
            foreach (var (a, b, stair) in debugLinks)
            {
                if (a == null || b == null) continue;
                Gizmos.color = stair ? Color.yellow : Color.cyan;
                Gizmos.DrawLine(a.WorldCenter, b.WorldCenter);
            }
        }
    }
}