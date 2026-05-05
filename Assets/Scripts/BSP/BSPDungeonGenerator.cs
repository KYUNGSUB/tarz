using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BSPDungeonGenerator : MonoBehaviour
{
    [Header("Tilemap")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;

    public TileBase floorTile;
    public TileBase wallTile;

    [Header("Map Settings")]
    public int mapWidth = 80;
    public int mapHeight = 80;

    [Header("BSP Settings")]
    public int minSplitSize = 20;
    public int minRoomSize = 8;

    private BSPNode root;
    private List<RectInt> rooms = new List<RectInt>();
    private List<Vector2Int> corridors = new List<Vector2Int>();

    void Start()
    {
        GenerateDungeon();
    }

    public void GenerateDungeon()
    {
        ClearMap();

        root = new BSPNode(new RectInt(0, 0, mapWidth, mapHeight));

        Split(root);
        CreateRooms(root);
        ConnectRooms(root);

        DrawRooms();
        DrawCorridors();
        DrawWalls();
    }

    class BSPNode
    {
        public RectInt area;
        public BSPNode left;
        public BSPNode right;
        public RectInt room;

        public BSPNode(RectInt area)
        {
            this.area = area;
        }

        public bool IsLeaf()
        {
            return left == null && right == null;
        }
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
            rooms.Add(node.room);
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
        foreach (var room in rooms)
        {
            for (int x = room.x; x < room.xMax; x++)
            {
                for (int y = room.y; y < room.yMax; y++)
                {
                    floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
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
    }
}