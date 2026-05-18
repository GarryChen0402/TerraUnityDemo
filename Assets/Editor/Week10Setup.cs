using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;

public static class Week10Setup
{
    [MenuItem("Tools/Setup Week 10 - Complete")]
    public static void SetupAll()
    {
        SetupDayNightManager();
        SetupNightOverlay();
        SetupFurnitureTilemap();
        SetupRoomDetector();
        SetupShopUI();
        PopulateGeneralStore();
        CreateSummonToken();
        CreateFurnitureTiles();
        CreateNPCPrefab();
        Debug.Log("Week 10 setup complete! All systems configured.");
    }

    // 1. DayNightManager
    private static void SetupDayNightManager()
    {
        var existing = Object.FindObjectOfType<DayNightCycle>();
        if (existing != null)
        {
            Debug.Log("DayNightManager already exists.");
            return;
        }

        var go = new GameObject("DayNightManager");
        go.AddComponent<DayNightCycle>();
        Debug.Log("Created DayNightManager.");
    }

    // 2. NightOverlay on Canvas
    private static void SetupNightOverlay()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in scene!");
            return;
        }

        var existing = canvas.transform.Find("NightOverlay");
        if (existing != null)
        {
            var dn = Object.FindObjectOfType<DayNightCycle>();
            if (dn != null) dn.GetType().GetField("nightOverlay",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(dn, existing.GetComponent<Image>());
            Debug.Log("NightOverlay already exists, re-linked.");
            return;
        }

        var overlay = new GameObject("NightOverlay", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(canvas.transform, false);

        var rt = overlay.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = overlay.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0);
        img.raycastTarget = false;

        var dnCycle = Object.FindObjectOfType<DayNightCycle>();
        if (dnCycle != null)
        {
            var field = typeof(DayNightCycle).GetField("nightOverlay",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(dnCycle, img);
        }

        EditorUtility.SetDirty(overlay);
        Debug.Log("Created NightOverlay on Canvas.");
    }

    // 3. Tilemap_Furniture
    private static void SetupFurnitureTilemap()
    {
        var grid = Object.FindObjectOfType<Grid>();
        if (grid == null)
        {
            Debug.LogError("No Grid found in scene!");
            return;
        }

        var existing = grid.transform.Find("Tilemap_Furniture");
        if (existing != null)
        {
            Debug.Log("Tilemap_Furniture already exists.");
            return;
        }

        var go = new GameObject("Tilemap_Furniture", typeof(Tilemap), typeof(TilemapRenderer));
        go.transform.SetParent(grid.transform, false);

        var renderer = go.GetComponent<TilemapRenderer>();
        renderer.sortingOrder = 1;

        EditorUtility.SetDirty(go);
        Debug.Log("Created Tilemap_Furniture.");
    }

    // 4. RoomDetector on WorldManager
    private static void SetupRoomDetector()
    {
        var existing = Object.FindObjectOfType<RoomDetector>();
        if (existing != null)
        {
            Debug.Log("RoomDetector already exists.");
            return;
        }

        var worldManager = GameObject.Find("WorldManager");
        if (worldManager == null)
        {
            Debug.LogError("No WorldManager found in scene!");
            return;
        }

        var rd = worldManager.AddComponent<RoomDetector>();

        var grid = Object.FindObjectOfType<Grid>();
        if (grid != null)
        {
            var groundTm = grid.transform.Find("Tilemap_Ground");
            if (groundTm != null)
                SetPrivateField(rd, "groundTilemap", groundTm.GetComponent<Tilemap>());

            var furnTm = grid.transform.Find("Tilemap_Furniture");
            if (furnTm != null)
                SetPrivateField(rd, "furnitureTilemap", furnTm.GetComponent<Tilemap>());
        }

        EditorUtility.SetDirty(worldManager);
        Debug.Log("Added RoomDetector to WorldManager.");
    }

    // 5. ShopUI
    private static void SetupShopUI()
    {
        var existing = Object.FindObjectOfType<ShopUI>();
        if (existing != null)
        {
            Debug.Log("ShopUI already exists.");
            return;
        }

        var go = new GameObject("ShopUI");
        go.AddComponent<ShopUI>();
        Debug.Log("Created ShopUI.");
    }

    // 6. Populate GeneralStore with items
    private static void PopulateGeneralStore()
    {
        var shopData = AssetDatabase.LoadAssetAtPath<ShopData>(
            "Assets/Sources/ScriptObjects/GeneralStore.asset");

        if (shopData == null)
        {
            shopData = ScriptableObject.CreateInstance<ShopData>();
            shopData.shopName = "General Store";
            AssetDatabase.CreateAsset(shopData, "Assets/Sources/ScriptObjects/GeneralStore.asset");
        }

        var items = new System.Collections.Generic.List<ShopItemEntry>();

        AddItemIfExists(items, "Assets/Sources/ScriptObjects/DirtItem.asset");
        AddItemIfExists(items, "Assets/Sources/ScriptObjects/FiberItem.asset");
        AddItemIfExists(items, "Assets/Sources/ScriptObjects/WoodPickaxeItem.asset");
        AddItemIfExists(items, "Assets/Sources/ScriptObjects/WoodSwordItem.asset");

        shopData.itemsForSale = items.ToArray();
        shopData.sellPriceMultiplier = 0.5f;

        EditorUtility.SetDirty(shopData);
        AssetDatabase.SaveAssets();
        Debug.Log($"GeneralStore populated with {items.Count} items.");
    }

    private static void AddItemIfExists(System.Collections.Generic.List<ShopItemEntry> list, string path)
    {
        var item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
        if (item != null)
            list.Add(new ShopItemEntry { item = item, amount = -1 });
    }

    // 7. NPC Summon Token
    private static void CreateSummonToken()
    {
        const string path = "Assets/Sources/ScriptObjects/NPCSummonToken.asset";
        var existing = AssetDatabase.LoadAssetAtPath<ItemData>(path);
        if (existing != null)
        {
            Debug.Log("NPC Summon Token already exists.");
            return;
        }

        var token = ScriptableObject.CreateInstance<ItemData>();
        token.itemID = 50;
        token.itemName = "NPC Summon Token";
        token.description = "Summons an NPC into a valid room.";
        token.maxStack = 1;
        token.itemType = ItemType.Misc;
        token.rarity = ItemRarity.Uncommon;
        token.coinValue = 0;

        AssetDatabase.CreateAsset(token, path);
        AssetDatabase.SaveAssets();
        Debug.Log("Created NPC Summon Token.");

        // Link to RoomDetector
        var rd = Object.FindObjectOfType<RoomDetector>();
        if (rd != null)
        {
            SetPrivateField(rd, "npcSummonItem", token);
            EditorUtility.SetDirty(rd);
        }
    }

    // 8. Furniture Tile assets
    private static void CreateFurnitureTiles()
    {
        CreateColoredTile("DoorTile", new Color(0.55f, 0.35f, 0.15f), "doorTile");
        CreateColoredTile("TableTile", new Color(0.7f, 0.55f, 0.3f), "tableTile");
        CreateColoredTile("ChairTile", new Color(0.6f, 0.45f, 0.25f), "chairTile");
        CreateColoredTile("LightTile", new Color(1f, 0.9f, 0.3f), "lightTile");
    }

    private static void CreateColoredTile(string name, Color color, string fieldName)
    {
        const string dir = "Assets/Sources/Tiles/";

        // Ensure directory exists
        if (!AssetDatabase.IsValidFolder(dir.TrimEnd('/')))
        {
            var parent = "Assets/Sources";
            if (!AssetDatabase.IsValidFolder(parent))
                AssetDatabase.CreateFolder("Assets", "Sources");
            AssetDatabase.CreateFolder(parent, "Tiles");
        }

        string path = dir + name + ".asset";
        var existing = AssetDatabase.LoadAssetAtPath<Tile>(path);
        if (existing != null)
        {
            LinkTileToRoomDetector(existing, fieldName);
            return;
        }

        // Create a simple sprite for the tile
        var tex = new Texture2D(16, 16);
        var pixels = new Color[16 * 16];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();

        string texPath = dir + name + "_tex.png";
        byte[] pngData = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(texPath, pngData);
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(texPath);
        var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);

        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        AssetDatabase.CreateAsset(tile, path);

        LinkTileToRoomDetector(tile, fieldName);
        Debug.Log($"Created {name} at {path}");
    }

    private static void LinkTileToRoomDetector(Tile tile, string fieldName)
    {
        var rd = Object.FindObjectOfType<RoomDetector>();
        if (rd != null)
            SetPrivateField(rd, fieldName, tile);
    }

    // 9. NPC Prefab
    private static void CreateNPCPrefab()
    {
        const string path = "Assets/Sources/NPC.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null)
        {
            // Link to RoomDetector
            var roomDetector = Object.FindObjectOfType<RoomDetector>();
            if (roomDetector != null)
            {
                SetPrivateField(roomDetector, "npcPrefab", existing);
                EditorUtility.SetDirty(roomDetector);
            }
            Debug.Log("NPC Prefab already exists, re-linked.");
            return;
        }

        var go = new GameObject("NPC");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.color = Color.green;
        sr.sortingOrder = 1;

        var tex = new Texture2D(16, 16);
        var pixels = new Color[16 * 16];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.green;
        tex.SetPixels(pixels);
        tex.Apply();

        const string sprDir = "Assets/Sources/";
        string texPath = sprDir + "NPC_Sprite.png";
        System.IO.File.WriteAllBytes(texPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(texPath);
        var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);
        sr.sprite = sprite;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        var boxCol = go.AddComponent<BoxCollider2D>();
        boxCol.size = new Vector2(1f, 2f);
        boxCol.offset = new Vector2(0f, 1f);

        var npc = go.AddComponent<NPC>();
        npc.npcName = "Merchant";
        npc.interactionRange = 3f;

        var shopData = AssetDatabase.LoadAssetAtPath<ShopData>(
            "Assets/Sources/ScriptObjects/GeneralStore.asset");
        npc.shopData = shopData;

        // Ensure Sources directory exists
        if (!AssetDatabase.IsValidFolder("Assets/Sources"))
            AssetDatabase.CreateFolder("Assets", "Sources");

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        // Link to RoomDetector
        var rd = Object.FindObjectOfType<RoomDetector>();
        if (rd != null)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            SetPrivateField(rd, "npcPrefab", prefab);
            EditorUtility.SetDirty(rd);
        }

        Debug.Log("Created NPC prefab.");
    }

    // Helper: set private serialized field via reflection
    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
            EditorUtility.SetDirty(target as Object);
        }
        else
        {
            Debug.LogWarning($"Field '{fieldName}' not found on {target.GetType().Name}");
        }
    }
}
