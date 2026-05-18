using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class RoomDetector : MonoBehaviour
{
    public static RoomDetector Instance { get; private set; }

    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap furnitureTilemap;

    [Header("Room Requirements")]
    [SerializeField] private int minRoomWidth = 6;
    [SerializeField] private int minRoomHeight = 10;

    [Header("Furniture Tiles")]
    [SerializeField] private TileBase doorTile;
    [SerializeField] private TileBase tableTile;
    [SerializeField] private TileBase chairTile;
    [SerializeField] private TileBase lightTile;

    [Header("NPC Summon")]
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private ItemData npcSummonItem;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (groundTilemap == null)
        {
            var cm = ChunkManager.Instance;
            if (cm != null)
                groundTilemap = cm.GroundTilemap;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            TrySummonNPC();
        }
    }

    private void TrySummonNPC()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        if (npcSummonItem != null && PlayerInventory.Instance != null)
        {
            if (PlayerInventory.Instance.GetItemCount(npcSummonItem) <= 0)
            {
                Debug.Log("You need an NPC Summon Token to summon an NPC.");
                return;
            }
        }

        if (TrySpawnNPC(player.transform.position, out string message))
        {
            Debug.Log(message);
            if (npcSummonItem != null && PlayerInventory.Instance != null)
                PlayerInventory.Instance.ConsumeItem(npcSummonItem, 1);
        }
        else
        {
            Debug.Log($"Room invalid:\n{message}");
        }
    }

    [System.Serializable]
    public class RoomValidationResult
    {
        public bool hasWalls;
        public bool hasDoor;
        public bool hasTable;
        public bool hasChair;
        public bool hasLightSource;
        public bool meetsMinSize;
        public int areaWidth;
        public int areaHeight;
        public RectInt bounds;
        public Vector3 spawnPoint;

        public bool IsValid =>
            hasWalls && hasDoor && hasTable && hasChair && hasLightSource && meetsMinSize;

        public List<string> GetMissingRequirements(int minW, int minH)
        {
            var missing = new List<string>();
            if (!meetsMinSize)
                missing.Add($"Size too small: {areaWidth}x{areaHeight} (need {minW}x{minH})");
            if (!hasWalls)
                missing.Add("Missing walls (not fully enclosed)");
            if (!hasDoor)
                missing.Add("Missing door");
            if (!hasTable)
                missing.Add("Missing table");
            if (!hasChair)
                missing.Add("Missing chair");
            if (!hasLightSource)
                missing.Add("Missing light source");
            return missing;
        }
    }

    public RoomValidationResult DetectRoom(Vector3 worldPosition)
    {
        if (groundTilemap == null)
            return new RoomValidationResult();

        Vector3Int seedCell = groundTilemap.WorldToCell(worldPosition);

        var interiorCells = FloodFillEnclosedArea(seedCell);
        if (interiorCells == null || interiorCells.Count == 0)
            return new RoomValidationResult();

        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        foreach (var cell in interiorCells)
        {
            if (cell.x < minX) minX = cell.x;
            if (cell.y < minY) minY = cell.y;
            if (cell.x > maxX) maxX = cell.x;
            if (cell.y > maxY) maxY = cell.y;
        }

        var result = new RoomValidationResult
        {
            bounds = new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1),
            areaWidth = maxX - minX + 1,
            areaHeight = maxY - minY + 1
        };

        result.meetsMinSize = result.areaWidth >= minRoomWidth && result.areaHeight >= minRoomHeight;
        result.hasWalls = true;
        result.hasDoor = HasFurnitureInArea(result.bounds, doorTile);
        result.hasTable = HasFurnitureInArea(result.bounds, tableTile);
        result.hasChair = HasFurnitureInArea(result.bounds, chairTile);
        result.hasLightSource = HasFurnitureInArea(result.bounds, lightTile);

        Vector3 worldCenter = groundTilemap.CellToWorld(
            new Vector3Int(minX + result.areaWidth / 2, minY + result.areaHeight / 2, 0));
        result.spawnPoint = worldCenter + new Vector3(0.5f, 0.5f, 0);

        return result;
    }

    public bool TrySpawnNPC(Vector3 playerPosition, out string message)
    {
        var result = DetectRoom(playerPosition);

        if (!result.IsValid)
        {
            var missing = result.GetMissingRequirements(minRoomWidth, minRoomHeight);
            message = string.Join("\n  ", missing);
            if (string.IsNullOrEmpty(message))
                message = "No enclosed room found at this position.";
            return false;
        }

        if (npcPrefab == null)
        {
            message = "NPC prefab not assigned to RoomDetector.";
            return false;
        }

        Instantiate(npcPrefab, result.spawnPoint, Quaternion.identity);
        message = $"NPC spawned at {result.spawnPoint}";
        return true;
    }

    private HashSet<Vector3Int> FloodFillEnclosedArea(Vector3Int seed)
    {
        if (groundTilemap.GetTile(seed) != null)
            return null;

        var visited = new HashSet<Vector3Int>();
        var queue = new Queue<Vector3Int>();
        queue.Enqueue(seed);
        visited.Add(seed);

        int maxArea = 5000;
        bool isOpenToSky = false;

        Vector3Int[] dirs = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        while (queue.Count > 0 && visited.Count < maxArea)
        {
            var current = queue.Dequeue();

            foreach (var dir in dirs)
            {
                var neighbor = current + dir;
                if (visited.Contains(neighbor)) continue;

                TileBase tile = groundTilemap.GetTile(neighbor);

                if (tile == null)
                {
                    if (IsOpenToSurface(neighbor))
                    {
                        isOpenToSky = true;
                        break;
                    }

                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            if (isOpenToSky) break;
        }

        if (isOpenToSky) return null;
        if (visited.Count >= maxArea) return null;
        return visited;
    }

    private bool IsOpenToSurface(Vector3Int cell)
    {
        int maxCheck = 50;
        for (int y = cell.y; y < cell.y + maxCheck; y++)
        {
            if (groundTilemap.GetTile(new Vector3Int(cell.x, y, 0)) != null)
                return false;
        }
        return true;
    }

    private bool HasFurnitureInArea(RectInt bounds, TileBase targetTile)
    {
        if (targetTile == null || furnitureTilemap == null) return true;

        for (int x = bounds.x; x < bounds.x + bounds.width; x++)
        {
            for (int y = bounds.y; y < bounds.y + bounds.height; y++)
            {
                if (furnitureTilemap.GetTile(new Vector3Int(x, y, 0)) == targetTile)
                    return true;
            }
        }
        return false;
    }
}
