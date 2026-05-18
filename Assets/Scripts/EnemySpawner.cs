using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject slimePrefab;

    [Header("Spawning")]
    public float spawnInterval = 5f;
    public int maxEnemies = 10;
    public float spawnDistMin = 3f;
    public float spawnDistMax = 20f;

    [Header("Surface Scan")]
    public int scanStartY = 200;
    public int scanMinY = 0;

    private float spawnTimer;

    private void Awake()
    {
        if (slimePrefab == null)
            slimePrefab = Resources.Load<GameObject>("Slime");
    }

    private void Start()
    {
        spawnTimer = spawnInterval;
    }

    private void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f) return;

        spawnTimer = spawnInterval;

        int currentCount = Object.FindObjectsOfType<EnemyBase>().Length;
        if (currentCount >= maxEnemies) return;

        Vector2? spawnPos = FindSpawnPosition();
        if (spawnPos.HasValue)
        {
            Instantiate(slimePrefab, spawnPos.Value, Quaternion.identity);
        }
    }

    private Vector2? FindSpawnPosition()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return null;

        var cam = Camera.main;
        if (cam == null) return null;

        float camHalfWidth = cam.orthographicSize * cam.aspect;
        float camCenterX = cam.transform.position.x;
        float camLeft = camCenterX - camHalfWidth;
        float camRight = camCenterX + camHalfWidth;

        // Try a few times to find valid ground
        for (int attempt = 0; attempt < 15; attempt++)
        {
            bool spawnLeft = Random.value < 0.5f;
            float dist = Random.Range(spawnDistMin, spawnDistMax);
            float spawnX = spawnLeft ? (camLeft - dist) : (camRight + dist);

            int surfaceY = FindSurfaceY(spawnX);
            if (surfaceY == int.MinValue) continue;

            float spawnY = surfaceY + 1.5f;

            // Check not inside terrain
            Vector3Int checkCell = new Vector3Int(
                Mathf.FloorToInt(spawnX),
                Mathf.FloorToInt(spawnY),
                0);
            var tilemap = ChunkManager.Instance?.GroundTilemap;
            if (tilemap != null && tilemap.GetTile(checkCell) != null) continue;

            return new Vector2(spawnX, spawnY);
        }

        return null;
    }

    private int FindSurfaceY(float worldX)
    {
        var tilemap = ChunkManager.Instance?.GroundTilemap;
        if (tilemap == null) return int.MinValue;

        int cellX = Mathf.FloorToInt(worldX);

        for (int y = scanStartY; y >= scanMinY; y--)
        {
            Vector3Int cellPos = new Vector3Int(cellX, y, 0);
            if (tilemap.GetTile(cellPos) != null)
                return y;
        }

        return int.MinValue;
    }
}
