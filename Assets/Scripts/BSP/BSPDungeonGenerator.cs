using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public class BSPDungeonGenerator : MonoBehaviour
{
    [Header("Tilemap")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;

    public TileBase floorTile;
    public TileBase wallTile;

    public TileBase bossTile;
    public TileBase secretTile;
    public TileBase rewardTile;
    public TileBase explorationTile;
    public TileBase combatTile;

    [Header("Map Settings")]
    public int mapWidth = 80;
    public int mapHeight = 80;

    [Header("BSP Settings")]
    public int minSplitSize = 20;
    public int minRoomSize = 8;

    [Header("Seed Settings")]
    public int seed = 123456;
    public bool useRandomSeed = true;

    private BSPNode root;
    // private List<RectInt> rooms = new List<RectInt>();
    List<BSPNode> rooms = new List<BSPNode>();
    private List<Vector2Int> corridors = new List<Vector2Int>();

    public enum RoomType
    {
        None,
        Combat,
        Exploration,
        Secret,
        Boss,
        Reward
    }

    void Start()
    {
        GenerateDungeon();
    }

    public void GenerateDungeon()
    {
        ClearMap();
        
        // 1. 저장된 seed 로드 시도
        bool loaded = LoadSeed();

        // 2. seed 결정
        if (!loaded)
        {
            if (useRandomSeed)
            {
                seed = System.DateTime.Now.Millisecond;
            }
        }

        // 3. 랜덤 초기화 (핵심)
        Random.InitState(seed);

        Debug.Log("Current Seed: " + seed);

        // 4. 기존 BSP 로직 그대로 유지
        root = new BSPNode(new RectInt(0, 0, mapWidth, mapHeight));

        Split(root);
        CreateRooms(root);
        ConnectRooms(root);

        // ★ 추가
        rooms.Clear();
        CollectRooms(root);

        AssignRoomTypes();

        DrawRooms();
        DrawCorridors();
        DrawWalls();

        // 5. 생성 후 seed 저장
        SaveSeed();
    }

    public void GenerateNewMap()    // 강제로 새로운 맵 만들기
    {
        useRandomSeed = true;

        // 기존 파일 삭제
        string path = GetSeedPath();
        if (File.Exists(path))
            File.Delete(path);

        GenerateDungeon();
    }

    class BSPNode
    {
        public RectInt area;
        public BSPNode left;
        public BSPNode right;
        public RectInt room;

        // 추가
        public RoomType roomType = RoomType.None;
        public Vector2Int Center => new Vector2Int(
            room.x + room.width / 2,
            room.y + room.height / 2
        );

        public List<BSPNode> connectedNodes = new List<BSPNode>();

        public BSPNode(RectInt area)
        {
            this.area = area;
        }

        public bool IsLeaf()
        {
            return left == null && right == null;
        }
    }

    [System.Serializable]
    public class MapSeedData
    {
        public int seed;
        // public int difficulty;   // seed +  게임 상태 함께 관리
        // public int level;
    }

    string GetSeedPath()
    {
        string seedPath = Application.persistentDataPath + "/mapSeed.json";
        Debug.Log("mapSeed.json Path : " + seedPath);   // Users\ksseo\AppData\LocalLow\DaultCompany\TARZ
        return seedPath;
    }

    void SaveSeed()
    {
        MapSeedData data = new MapSeedData();
        data.seed = seed;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSeedPath(), json);

        Debug.Log("Seed Saved: " + seed);
    }

    bool LoadSeed()
    {
        string path = GetSeedPath();

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            MapSeedData data = JsonUtility.FromJson<MapSeedData>(json);

            seed = data.seed;
            Debug.Log("Seed Loaded: " + seed);

            return true;
        }

        return false;
    }

    void Split(BSPNode node)
    {
        // 더 이상 분할할 수 없는 경우
        if (node.area.width < minSplitSize * 2 &&
            node.area.height < minSplitSize * 2)
        {
            return;
        }

        bool splitHorizontal = Random.value > 0.5f;

        // 비율 기반 방향 보정 (한쪽이 너무 길면 그 방향으로 분할)
        if (node.area.width > node.area.height && node.area.width / (float)node.area.height >= 1.25f)
        {
            splitHorizontal = false;
        }
        else if (node.area.height > node.area.width && node.area.height / (float)node.area.width >= 1.25f)
        {
            splitHorizontal = true;
        }

        if (splitHorizontal)    // 수직으로 자른다
        {
            // 높이 기준 분할 가능 여부 확인
            if (node.area.height < minSplitSize * 2)
                return;

            int split = Random.Range(minSplitSize, node.area.height - minSplitSize);

            node.left = new BSPNode(new RectInt(
                node.area.x,
                node.area.y,
                node.area.width,
                split));

            node.right = new BSPNode(new RectInt(
                node.area.x,
                node.area.y + split,
                node.area.width,
                node.area.height - split));
        }
        else
        {                       // 수평으로 
            // 너비 기준 분할 가능 여부 확인
            if (node.area.width < minSplitSize * 2)
                return;

            int split = Random.Range(minSplitSize, node.area.width - minSplitSize);

            node.left = new BSPNode(new RectInt(
                node.area.x,
                node.area.y,
                split,
                node.area.height));

            node.right = new BSPNode(new RectInt(
                node.area.x + split,
                node.area.y,
                node.area.width - split,
                node.area.height));
        }

        Split(node.left);
        Split(node.right);
    }

    void CreateRooms(BSPNode node)
    {
        if (node.IsLeaf())
        {
            if (node.area.width < minRoomSize + 2 || 
            node.area.height < minRoomSize + 2)
            {
                return; // 방 생성 안 함
            }

            int roomWidth = Random.Range(minRoomSize, node.area.width - 2);
            int roomHeight = Random.Range(minRoomSize, node.area.height - 2);

            int x = Random.Range(node.area.x + 1, node.area.xMax - roomWidth - 1);
            int y = Random.Range(node.area.y + 1, node.area.yMax - roomHeight - 1);

            node.room = new RectInt(x, y, roomWidth, roomHeight);
            rooms.Add(node);
        }
        else
        {
            if (node.left != null) CreateRooms(node.left);
            if (node.right != null) CreateRooms(node.right);
        }
    }

    void ConnectRooms(BSPNode node)
    {
        if (node.left != null && node.right != null)
        {
            Vector2Int p1 = GetRoomCenter(node.left);
            Vector2Int p2 = GetRoomCenter(node.right);

            CreateCorridor(p1, p2);

            // ★ 추가: 연결 정보 저장
            node.left.connectedNodes.Add(node.right);
            node.right.connectedNodes.Add(node.left);

            ConnectRooms(node.left);
            ConnectRooms(node.right);
        }
    }

    Vector2Int GetRoomCenter(BSPNode node)
    {
        if (node.IsLeaf())
        {
            return new Vector2Int(
                node.room.x + node.room.width / 2,
                node.room.y + node.room.height / 2);
        }

        return Random.value > 0.5f ? GetRoomCenter(node.left) : GetRoomCenter(node.right);
    }

    void CreateCorridor(Vector2Int from, Vector2Int to)
    {
        Vector2Int pos = from;

        while (pos.x != to.x)
        {
            corridors.Add(pos);
            pos.x += (to.x > pos.x) ? 1 : -1;
        }

        while (pos.y != to.y)
        {
            corridors.Add(pos);
            pos.y += (to.y > pos.y) ? 1 : -1;
        }
    }

    void DrawRooms()
    {
        foreach (var node in rooms)
        {
            RectInt room = node.room;

            // 방 유효성 체크
            if (room.width <= 0 || room.height <= 0)
                continue;

            TileBase tile = floorTile;

            switch (node.roomType)
            {
                case RoomType.Boss:
                    tile = bossTile;
                    break;

                case RoomType.Secret:
                    tile = secretTile;
                    break;

                case RoomType.Reward:
                    tile = rewardTile;
                    break;

                case RoomType.Exploration:
                    tile = explorationTile;
                    break;

                case RoomType.Combat:
                    // 기본 floorTile 유지
                    tile = combatTile;
                    break;
            }

            // tile null 방지
            if (tile == null)
                tile = floorTile;

            for (int x = room.x; x < room.xMax; x++)
            {
                for (int y = room.y; y < room.yMax; y++)
                {
                    floorTilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }

    void DrawCorridors()
    {
        foreach (var pos in corridors)
        {
            floorTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), floorTile);
        }
    }

    void DrawWalls()
    {
        BoundsInt bounds = floorTilemap.cellBounds;

        foreach (var pos in bounds.allPositionsWithin)
        {
            if (floorTilemap.GetTile(pos) == null)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (floorTilemap.GetTile(pos + new Vector3Int(x, y, 0)) != null)
                        {
                            wallTilemap.SetTile(pos, wallTile);
                            break;
                        }
                    }
                }
            }
        }
    }

    void ClearMap()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        rooms.Clear();
        corridors.Clear();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateDungeon();
        }
        else if (Input.GetKeyDown(KeyCode.End))
        {
            GenerateNewMap();
        }
    }

    void CollectRooms(BSPNode node)
    {
        if (node.IsLeaf())
        {
            if (node.room.width > 0)
                rooms.Add(node);
        }
        else
        {
            if (node.left != null) CollectRooms(node.left);
            if (node.right != null) CollectRooms(node.right);
        }
    }

    void AssignRoomTypes()
    {
        if (rooms.Count == 0) return;

        // 1. Start Room
        BSPNode startRoom = rooms[0];

        // 2. Boss Room (가장 먼 방)
        BSPNode bossRoom = startRoom;
        float maxDist = 0;

        foreach (var room in rooms)
        {
            float dist = Vector2.Distance(startRoom.Center, room.Center);
            if (dist > maxDist)
            {
                maxDist = dist;
                bossRoom = room;
            }
        }

        bossRoom.roomType = RoomType.Boss;

        // 3. Secret Room (leaf + 확률)
        foreach (var room in rooms)
        {
            if (room.roomType == RoomType.None &&
                room.connectedNodes.Count == 1 &&
                Random.value < 0.2f)
            {
                room.roomType = RoomType.Secret;
            }
        }

        // 4. Reward Room
        foreach (var room in rooms)
        {
            if (room.roomType == RoomType.None &&
                room.connectedNodes.Count == 1 &&
                Random.value < 0.3f)
            {
                room.roomType = RoomType.Reward;
            }
        }

        // 5. Exploration Room (연결 많음)
        foreach (var room in rooms)
        {
            if (room.roomType == RoomType.None &&
                room.connectedNodes.Count >= 3)
            {
                room.roomType = RoomType.Exploration;
            }
        }

        // 6. 나머지 → Combat
        foreach (var room in rooms)
        {
            if (room.roomType == RoomType.None)
            {
                room.roomType = RoomType.Combat;
            }
        }
    }
}