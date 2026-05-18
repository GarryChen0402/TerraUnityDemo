using UnityEngine;

public class ChunkData
{
    public Vector2Int chunkCoord;
    public int[,] tileIDs;       // 16×16, -1 = air
    public bool isActive;
    public bool isGenerated;

    public ChunkData(Vector2Int coord)
    {
        chunkCoord = coord;
        tileIDs = new int[16, 16];
        for (int x = 0; x < 16; x++)
            for (int y = 0; y < 16; y++)
                tileIDs[x, y] = -1;
    }
}
