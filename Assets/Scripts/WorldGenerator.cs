using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator Instance { get; private set; }

    [Header("Terrain Parameters")]
    [SerializeField] private float noiseScale = 0.05f;
    [SerializeField] private int surfaceBaseHeight = 100;
    [SerializeField] private int surfaceVariation = 20;
    [SerializeField] private int worldSeed;
    public int WorldSeed
    {
        get => worldSeed;
        set => worldSeed = value;
    }

    private bool seedInitialized;

    private void Awake() => Instance = this;

    private void Start()
    {
        if (!seedInitialized)
        {
            worldSeed = Random.Range(0, 999999);
            seedInitialized = true;
        }
    }

    public static void FillChunk(ChunkData chunk)
    {
        int gx0 = chunk.chunkCoord.x * 16;
        int gy0 = chunk.chunkCoord.y * 16;

        for (int lx = 0; lx < 16; lx++)
        {
            int worldX = gx0 + lx;
            for (int ly = 0; ly < 16; ly++)
            {
                int worldY = gy0 + ly;

                float noiseVal = Mathf.PerlinNoise(
                    (worldX + Instance.worldSeed) * Instance.noiseScale,
                    (worldY + Instance.worldSeed) * 0.02f);
                int surfaceY = Instance.surfaceBaseHeight +
                    Mathf.RoundToInt(noiseVal * Instance.surfaceVariation);

                int tileID;
                if (worldY > surfaceY)
                {
                    tileID = -1;  // air
                }
                else if (worldY == surfaceY)
                {
                    tileID = 0;  // grass
                }
                else if (worldY > surfaceY - 5)
                {
                    tileID = 1;  // dirt
                }
                else if (worldY > surfaceY - 30)
                {
                    tileID = IsCaveAt(worldX, worldY) ? -1 : 2;  // stone or cave
                }
                else
                {
                    tileID = IsCaveAt(worldX, worldY) ? -1 : 2;
                }

                chunk.tileIDs[lx, ly] = tileID;
            }
        }

        chunk.isGenerated = true;
    }

    private static bool IsCaveAt(int x, int y)
    {
        float n1 = Mathf.PerlinNoise((x + 500) * 0.08f, (y + 500) * 0.08f);
        float n2 = Mathf.PerlinNoise((x + 1000) * 0.04f, (y + 1000) * 0.04f);
        float threshold = y < 40 ? 0.6f : 0.5f;
        return n1 * n2 > threshold;
    }
}
