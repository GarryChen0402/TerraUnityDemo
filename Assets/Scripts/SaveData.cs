using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int saveVersion = 1;
    public string saveTime;
    public WorldData worldData;
    public PlayerData playerData;
    public NpcData[] npcs;
    public DeadEnemyData[] deadEnemies;
}

[System.Serializable]
public class WorldData
{
    public int worldSeed;
    public float timeOfDay;
    public ChunkSaveEntry[] modifiedChunks;
}

[System.Serializable]
public class ChunkSaveEntry
{
    public int cx;
    public int cy;
    public int[] flatTiles; // length 256, row-major: flatTiles[y*16+x]
}

[System.Serializable]
public class PlayerData
{
    public float posX;
    public float posY;
    public int currentHP;
    public int currentMP;
    public int maxHP;
    public int maxMP;
    public int baseAttack;
    public int defense;
    public long totalCopper;
    public InvSlotSave[] inventory;   // only non-empty slots
    public int[] equipment;           // length 9, itemID or -1
}

[System.Serializable]
public class InvSlotSave
{
    public int slotIndex;
    public int itemID;   // -1 = empty
    public int amount;
}

[System.Serializable]
public class NpcData
{
    public string npcName;
    public float posX;
    public float posY;
    public float wanderDirection;
    public float wanderTimer;
    public bool hasShownNightMessage;
    public bool flipX;
}

[System.Serializable]
public class DeadEnemyData
{
    public string enemyTypeName;
    public float posX;
    public float posY;
}
