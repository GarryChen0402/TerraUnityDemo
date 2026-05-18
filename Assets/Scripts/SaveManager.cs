using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string SaveSubDir = "saves";
    private const string FilePrefix = "save_slot_";
    private const string FileExtension = ".json";
    private const float AutoSaveInterval = 300f;

    private float autoSaveTimer;
    private Dictionary<int, ItemData> itemDB;
    private HashSet<string> deadEnemyKeys = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        BuildItemDatabase();
    }

    private void Start()
    {
        autoSaveTimer = AutoSaveInterval;
    }

    private void Update()
    {
        autoSaveTimer -= Time.deltaTime;
        if (autoSaveTimer <= 0f)
        {
            Save(0);
            autoSaveTimer = AutoSaveInterval;
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            Save(1);
            Debug.Log("Game saved (slot 1)");
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            if (SlotExists(1))
                Load(1);
            else if (SlotExists(2))
                Load(2);
            else if (SlotExists(3))
                Load(3);
            else if (SlotExists(0))
                Load(0);
            else
                Debug.LogWarning("No save file found!");
        }
    }

    private string SaveDirectory => Path.Combine(Application.persistentDataPath, SaveSubDir);
    private string GetFilePath(int slot) => Path.Combine(SaveDirectory, $"{FilePrefix}{slot}{FileExtension}");

    public bool SlotExists(int slot) => File.Exists(GetFilePath(slot));

    private void BuildItemDatabase()
    {
        itemDB = new Dictionary<int, ItemData>();
        var items = Resources.LoadAll<ItemData>("");
        foreach (var item in items)
        {
            if (!itemDB.ContainsKey(item.itemID))
                itemDB[item.itemID] = item;
        }
        Debug.Log($"[SaveManager] Loaded {itemDB.Count} ItemData assets into database.");
    }

    public void RegisterDeadEnemy(EnemyBase enemy)
    {
        if (enemy != null)
            deadEnemyKeys.Add(enemy.DeathKey);
    }

    public void Save(int slot)
    {
        var data = BuildSaveData();
        data.saveTime = System.DateTime.Now.ToString("O");

        string json = JsonUtility.ToJson(data, prettyPrint: true);
        Directory.CreateDirectory(SaveDirectory);
        File.WriteAllText(GetFilePath(slot), json);
        Debug.Log($"[SaveManager] Saved to slot {slot}: {GetFilePath(slot)}");
    }

    public void Load(int slot)
    {
        string path = GetFilePath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveManager] No save file at slot {slot}");
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data;
        try
        {
            data = JsonUtility.FromJson<SaveData>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to parse save file: {e.Message}");
            return;
        }

        if (data == null)
        {
            Debug.LogError("[SaveManager] Save data is null after deserialization.");
            return;
        }

        ApplySaveData(data);
        Debug.Log($"[SaveManager] Loaded from slot {slot}");
    }

    private SaveData BuildSaveData()
    {
        var data = new SaveData();

        // World
        data.worldData = new WorldData();
        if (WorldGenerator.Instance != null)
            data.worldData.worldSeed = WorldGenerator.Instance.WorldSeed;
        if (DayNightCycle.Instance != null)
            data.worldData.timeOfDay = DayNightCycle.Instance.TimeOfDay;

        // Modified chunks
        var cm = ChunkManager.Instance;
        if (cm != null)
        {
            var dirtyList = new List<ChunkSaveEntry>();
            foreach (var kvp in cm.GetAllChunks())
            {
                if (kvp.Value.hasBeenModified)
                {
                    dirtyList.Add(new ChunkSaveEntry
                    {
                        cx = kvp.Key.x,
                        cy = kvp.Key.y,
                        flatTiles = kvp.Value.FlattenTileIDs()
                    });
                }
            }
            data.worldData.modifiedChunks = dirtyList.ToArray();
        }

        // Player
        var player = GameObject.FindGameObjectWithTag("Player");
        data.playerData = new PlayerData();

        if (player != null)
        {
            data.playerData.posX = player.transform.position.x;
            data.playerData.posY = player.transform.position.y;
        }

        if (PlayerStats.Instance != null)
        {
            var s = PlayerStats.Instance;
            data.playerData.currentHP = s.currentHP;
            data.playerData.currentMP = s.currentMP;
            data.playerData.maxHP = s.maxHP;
            data.playerData.maxMP = s.maxMP;
            data.playerData.baseAttack = s.baseAttack;
            data.playerData.defense = s.defense;
        }

        if (PlayerCurrency.Instance != null)
            data.playerData.totalCopper = PlayerCurrency.Instance.TotalCopper;

        if (PlayerInventory.Instance != null)
        {
            var inv = PlayerInventory.Instance;
            var slotList = new List<InvSlotSave>();
            for (int i = 0; i < inv.slots.Count; i++)
            {
                if (inv.slots[i].itemData != null)
                {
                    slotList.Add(new InvSlotSave
                    {
                        slotIndex = i,
                        itemID = inv.slots[i].itemData.itemID,
                        amount = inv.slots[i].amount
                    });
                }
            }
            data.playerData.inventory = slotList.ToArray();
        }

        if (PlayerEquipment.Instance != null)
            data.playerData.equipment = PlayerEquipment.Instance.GetEquipmentItemIDs();

        // NPCs
        var npcs = Object.FindObjectsOfType<NPC>();
        data.npcs = new NpcData[npcs.Length];
        for (int i = 0; i < npcs.Length; i++)
        {
            data.npcs[i] = new NpcData
            {
                npcName = npcs[i].npcName,
                posX = npcs[i].transform.position.x,
                posY = npcs[i].transform.position.y,
                wanderDirection = npcs[i].WanderDirection,
                wanderTimer = npcs[i].WanderTimer,
                hasShownNightMessage = npcs[i].HasShownNightMessage,
                flipX = npcs[i].FlipX
            };
        }

        // Dead enemies
        data.deadEnemies = deadEnemyKeys.Select(k =>
        {
            var parts = k.Split('|');
            return new DeadEnemyData
            {
                enemyTypeName = parts.Length > 0 ? parts[0] : "",
                posX = parts.Length > 1 && float.TryParse(parts[1], out var x) ? x : 0,
                posY = parts.Length > 2 && float.TryParse(parts[2], out var y) ? y : 0
            };
        }).ToArray();

        return data;
    }

    private void ApplySaveData(SaveData data)
    {
        // World seed
        if (WorldGenerator.Instance != null && data.worldData != null)
            WorldGenerator.Instance.WorldSeed = data.worldData.worldSeed;

        // Time of day
        if (DayNightCycle.Instance != null && data.worldData != null)
            DayNightCycle.Instance.TimeOfDay = data.worldData.timeOfDay;

        // Clear and rebuild world
        var cm = ChunkManager.Instance;
        var player = GameObject.FindGameObjectWithTag("Player");

        // Set player position first
        if (player != null && data.playerData != null)
            player.transform.position = new Vector3(data.playerData.posX, data.playerData.posY, 0);

        if (cm != null)
        {
            cm.ClearAllChunks();

            if (player != null)
            {
                Vector2Int playerChunk = cm.WorldToChunk(player.transform.position);
                cm.ForceRegenerateAround(playerChunk);
            }

            // Apply saved chunk modifications
            if (data.worldData != null && data.worldData.modifiedChunks != null)
            {
                foreach (var entry in data.worldData.modifiedChunks)
                {
                    if (entry.flatTiles != null && entry.flatTiles.Length == 256)
                        cm.ApplySavedChunk(new Vector2Int(entry.cx, entry.cy), entry.flatTiles);
                }
            }
        }

        // Player stats
        if (PlayerStats.Instance != null && data.playerData != null)
        {
            var pd = data.playerData;
            PlayerStats.Instance.LoadState(pd.currentHP, pd.currentMP, pd.maxHP, pd.maxMP, pd.baseAttack, pd.defense);
        }

        // Currency
        if (PlayerCurrency.Instance != null && data.playerData != null)
            PlayerCurrency.Instance.SetTotalCopper(data.playerData.totalCopper);

        // Inventory
        if (PlayerInventory.Instance != null && data.playerData != null)
        {
            PlayerInventory.Instance.ClearAll();
            if (data.playerData.inventory != null)
            {
                foreach (var slot in data.playerData.inventory)
                {
                    ItemData item = null;
                    if (slot.itemID >= 0 && itemDB != null)
                        itemDB.TryGetValue(slot.itemID, out item);

                    if (item != null || slot.itemID < 0)
                        PlayerInventory.Instance.LoadSlot(slot.slotIndex, item, slot.amount);
                    else
                        Debug.LogWarning($"[SaveManager] Item ID {slot.itemID} not found in database.");
                }
            }
        }

        // Equipment
        if (PlayerEquipment.Instance != null && data.playerData != null && data.playerData.equipment != null)
            PlayerEquipment.Instance.LoadFromItemIDs(data.playerData.equipment, itemDB);

        // NPCs
        var existingNpcs = Object.FindObjectsOfType<NPC>();
        if (data.npcs != null)
        {
            foreach (var npcData in data.npcs)
            {
                var match = System.Array.Find(existingNpcs, n => n.npcName == npcData.npcName);
                if (match != null)
                {
                    match.LoadState(npcData.posX, npcData.posY, npcData.wanderDirection,
                        npcData.wanderTimer, npcData.hasShownNightMessage, npcData.flipX);
                }
                else
                {
                    Debug.LogWarning($"[SaveManager] NPC '{npcData.npcName}' not found in scene.");
                }
            }
        }

        // Destroy dead enemies
        if (data.deadEnemies != null)
        {
            deadEnemyKeys.Clear();
            var allEnemies = Object.FindObjectsOfType<EnemyBase>();
            foreach (var entry in data.deadEnemies)
            {
                string key = $"{entry.enemyTypeName}|{entry.posX:F1}|{entry.posY:F1}";
                deadEnemyKeys.Add(key);

                foreach (var enemy in allEnemies)
                {
                    if (enemy != null && enemy.DeathKey == key)
                        Destroy(enemy.gameObject);
                }
            }
        }
    }
}
