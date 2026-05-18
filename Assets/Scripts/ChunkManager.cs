using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance { get; private set; }

    [Header("World Size")]
    public int worldWidth = 4200;
    public int worldHeight = 1200;

    [Header("Chunk Settings")]
    public int chunkSize = 16;
    public int activeChunkRadius = 2;

    [Header("Tile Assets (tileID → TileBase)")]
    public TileBase[] tileAssets;

    [Header("Target Tilemap")]
    [SerializeField] private Tilemap groundTilemap;

    private Dictionary<Vector2Int, ChunkData> chunks = new Dictionary<Vector2Int, ChunkData>();
    private Transform playerTransform;
    private Vector2Int lastCenterChunk;

    private void Awake() => Instance = this;

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        Vector2Int playerChunk = WorldToChunk(playerTransform.position);
        lastCenterChunk = playerChunk;
        UpdateActiveChunks(playerChunk);
    }

    private void Update()
    {
        if (playerTransform == null) return;

        Vector2Int playerChunk = WorldToChunk(playerTransform.position);
        if (playerChunk != lastCenterChunk)
        {
            lastCenterChunk = playerChunk;
            UpdateActiveChunks(playerChunk);
        }
    }

    public Vector2Int WorldToChunk(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / chunkSize),
            Mathf.FloorToInt(worldPos.y / chunkSize)
        );
    }

    private void UpdateActiveChunks(Vector2Int center)
    {
        HashSet<Vector2Int> neededChunks = new HashSet<Vector2Int>();
        for (int cx = -activeChunkRadius; cx <= activeChunkRadius; cx++)
            for (int cy = -activeChunkRadius; cy <= activeChunkRadius; cy++)
                neededChunks.Add(new Vector2Int(center.x + cx, center.y + cy));

        foreach (var kvp in chunks)
        {
            bool shouldBeActive = neededChunks.Contains(kvp.Key);
            if (kvp.Value.isActive && !shouldBeActive)
                DeactivateChunk(kvp.Key);
            else if (!kvp.Value.isActive && shouldBeActive)
                ActivateChunk(kvp.Key);
        }

        foreach (var coord in neededChunks)
        {
            if (!chunks.ContainsKey(coord))
            {
                chunks[coord] = new ChunkData(coord);
                GenerateChunkData(coord);
                ActivateChunk(coord);
            }
        }
    }

    private void GenerateChunkData(Vector2Int coord)
    {
        WorldGenerator.FillChunk(chunks[coord]);
    }

    private void ActivateChunk(Vector2Int coord)
    {
        var chunk = chunks[coord];
        chunk.isActive = true;
        ApplyChunkToTilemap(coord);
    }

    private void DeactivateChunk(Vector2Int coord)
    {
        var chunk = chunks[coord];
        chunk.isActive = false;

        for (int x = 0; x < chunkSize; x++)
            for (int y = 0; y < chunkSize; y++)
            {
                Vector3Int worldCell = new Vector3Int(
                    coord.x * chunkSize + x,
                    coord.y * chunkSize + y, 0);
                groundTilemap.SetTile(worldCell, null);
            }
    }

    public void MarkTileRemoved(Vector3Int worldCell)
    {
        Vector2Int chunkCoord = new Vector2Int(
            Mathf.FloorToInt((float)worldCell.x / chunkSize),
            Mathf.FloorToInt((float)worldCell.y / chunkSize));

        if (chunks.TryGetValue(chunkCoord, out var chunk))
        {
            int localX = worldCell.x - chunkCoord.x * chunkSize;
            int localY = worldCell.y - chunkCoord.y * chunkSize;
            if (localX >= 0 && localX < chunkSize && localY >= 0 && localY < chunkSize)
                chunk.tileIDs[localX, localY] = -1;
        }
    }

    private void ApplyChunkToTilemap(Vector2Int coord)
    {
        var chunk = chunks[coord];

        for (int x = 0; x < chunkSize; x++)
            for (int y = 0; y < chunkSize; y++)
            {
                int tileID = chunk.tileIDs[x, y];
                if (tileID < 0) continue;

                Vector3Int worldCell = new Vector3Int(
                    coord.x * chunkSize + x,
                    coord.y * chunkSize + y, 0);

                if (tileID < tileAssets.Length)
                    groundTilemap.SetTile(worldCell, tileAssets[tileID]);
            }
    }
}
