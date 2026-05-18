using UnityEngine;

[System.Serializable]
public class ChunkData
{
    public Vector2Int chunkCoord;
    public int[,] tileIDs;       // 16×16, -1 = air
    public bool isActive;
    public bool isGenerated;
    public bool hasBeenModified;

    public ChunkData(Vector2Int coord)
    {
        chunkCoord = coord;
        tileIDs = new int[16, 16];
        for (int x = 0; x < 16; x++)
            for (int y = 0; y < 16; y++)
                tileIDs[x, y] = -1;
    }

    public int[] FlattenTileIDs()
    {
        int[] flat = new int[256];
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
                flat[y * 16 + x] = tileIDs[x, y];
        return flat;
    }

    public void UnflattenTileIDs(int[] flat)
    {
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
                if (y * 16 + x < flat.Length)
                    tileIDs[x, y] = flat[y * 16 + x];
    }
}
