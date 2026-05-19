using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    public static Minimap Instance { get; private set; }

    [Header("Key Bindings")]
    [SerializeField] private KeyCode fullMapKey = KeyCode.M;

    [Header("Minimap Display")]
    [SerializeField] private float minimapPanelSize = 155f;
    [SerializeField] private float minimapViewRadius = 100f;

    [Header("Full Map")]
    [SerializeField] private float fullMapWidth = 600f;
    [SerializeField] private float fullMapHeight = 380f;

    [Header("Update")]
    [SerializeField] private float updateInterval = 0.3f;

    // Fog texture — 1 pixel = 10 world units
    private const int FogWidth = 420;
    private const int FogHeight = 120;
    private const float WorldUnitsPerPixel = 10f;

    // Minimap render texture
    private const int MinimapTexSize = 150;

    // ── Colors ─────────────────────────────────────────

    // Fog / unexplored
    private static readonly Color32 FogUnexplored = new Color32(8, 8, 14, 255);

    // Terrain tile ID → minimap color
    // -1 air, 0 grass, 1 dirt, 2 stone
    private static readonly Color32[] TileColors = new Color32[]
    {
        new Color32(60, 120, 40, 255),   // 0 = grass
        new Color32(100, 75, 40, 255),   // 1 = dirt
        new Color32(90, 88, 95, 255),    // 2 = stone
    };
    private static readonly Color32 AirColor = new Color32(30, 35, 50, 255);
    private static readonly Color32 UnknownTerrain = new Color32(35, 35, 42, 255);

    // Entity dots
    private static readonly Color32 PlayerDot = new Color32(0, 230, 230, 255);
    private static readonly Color32 NpcDot = new Color32(0, 220, 0, 255);
    private static readonly Color32 EnemyDot = new Color32(230, 40, 40, 255);

    // ── UI references ──────────────────────────────────

    private GameObject minimapPanel;
    private RawImage minimapRawImage;
    private Texture2D minimapTex;
    private Color32[] minimapPixels;

    private GameObject fullMapPanel;
    private RawImage fullMapRawImage;
    private Text fullMapLabel;

    // ── Fog state ──────────────────────────────────────

    private Texture2D fogTexture;
    private Texture2D fogTexNoEnemies;
    private Color32[] fogPixels;
    private Color32[] fogPixelsNoEnemies;
    private Color32[] fogClean;
    private bool isFullMapOpen;
    private bool showEnemiesOnFullMap = false;
    private float updateTimer;

    // ── Cache ──────────────────────────────────────────

    private Transform playerTransform;
    private ChunkManager chunkManager;
    private int chunkSize;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[Minimap] No Canvas found in scene.");
            return;
        }

        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        chunkManager = ChunkManager.Instance;
        if (chunkManager != null)
            chunkSize = chunkManager.chunkSize;

        fogTexture = new Texture2D(FogWidth, FogHeight, TextureFormat.RGBA32, false);
        fogTexture.filterMode = FilterMode.Point;
        fogPixels = new Color32[FogWidth * FogHeight];
        fogPixelsNoEnemies = new Color32[FogWidth * FogHeight];
        fogClean = new Color32[FogWidth * FogHeight];
        ClearFog();

        fogTexNoEnemies = new Texture2D(FogWidth, FogHeight, TextureFormat.RGBA32, false);
        fogTexNoEnemies.filterMode = FilterMode.Point;

        minimapTex = new Texture2D(MinimapTexSize, MinimapTexSize, TextureFormat.RGBA32, false);
        minimapTex.filterMode = FilterMode.Point;
        minimapPixels = new Color32[MinimapTexSize * MinimapTexSize];

        CreateMinimapUI(canvas);
        CreateFullMapUI(canvas);

        updateTimer = 0f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(fullMapKey))
            ToggleFullMap();

        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0f)
        {
            updateTimer = updateInterval;
            RefreshFogAndMinimap();
        }
    }

    // ── Fog of War ──────────────────────────────────────

    private void ClearFog()
    {
        for (int i = 0; i < fogPixels.Length; i++)
            fogPixels[i] = FogUnexplored;
        System.Array.Copy(fogPixels, fogClean, fogPixels.Length);
        fogTexture.SetPixels32(fogPixels);
        fogTexture.Apply();
    }

    private void RefreshFogAndMinimap()
    {
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null) return;
        }

        Vector3 playerPos = playerTransform.position;
        int playerTexX = WorldToTexX(playerPos.x);
        int playerTexY = WorldToTexY(playerPos.y);

        // Reveal fog — stores terrain colors sampled from ChunkManager
        RevealCircle(playerTexX, playerTexY, 4);

        // Copy clean (terrain-only) to working buffer, then overlay entity dots
        System.Array.Copy(fogClean, fogPixels, fogPixels.Length);

        DrawDotOnFog(playerTexX, playerTexY, PlayerDot);

        var npcs = FindObjectsOfType<NPC>();
        foreach (var npc in npcs)
        {
            int nx = WorldToTexX(npc.transform.position.x);
            int ny = WorldToTexY(npc.transform.position.y);
            DrawDotOnFog(nx, ny, NpcDot);
        }

        var enemies = FindObjectsOfType<EnemyBase>();
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            int ex = WorldToTexX(enemy.transform.position.x);
            int ey = WorldToTexY(enemy.transform.position.y);
            DrawDotOnFog(ex, ey, EnemyDot);
        }

        fogTexture.SetPixels32(fogPixels);
        fogTexture.Apply();

        // Build a no-enemies version for the full map toggle
        System.Array.Copy(fogClean, fogPixelsNoEnemies, fogPixelsNoEnemies.Length);
        DrawDotOnBuf(fogPixelsNoEnemies, playerTexX, playerTexY, PlayerDot);
        var npcs2 = FindObjectsOfType<NPC>();
        foreach (var n in npcs2)
            DrawDotOnBuf(fogPixelsNoEnemies, WorldToTexX(n.transform.position.x), WorldToTexY(n.transform.position.y), NpcDot);
        fogTexNoEnemies.SetPixels32(fogPixelsNoEnemies);
        fogTexNoEnemies.Apply();

        if (fullMapRawImage != null)
            fullMapRawImage.texture = showEnemiesOnFullMap ? fogTexture : fogTexNoEnemies;

        RenderMinimapView();
    }

    private void RevealCircle(int cx, int cy, int radius)
    {
        int r2 = radius * radius;
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx * dx + dy * dy > r2) continue;
                int px = cx + dx;
                int py = cy + dy;
                if (px < 0 || px >= FogWidth || py < 0 || py >= FogHeight) continue;

                int worldX = px * (int)WorldUnitsPerPixel;
                int worldY = py * (int)WorldUnitsPerPixel;
                fogClean[py * FogWidth + px] = SampleTerrainColor(worldX, worldY);
            }
        }
    }

    private void DrawDotOnFog(int cx, int cy, Color32 color)
    {
        DrawDotOnBuf(fogPixels, cx, cy, color);
    }

    private void DrawDotOnBuf(Color32[] buf, int cx, int cy, Color32 color)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int px = cx + dx;
                int py = cy + dy;
                if (px < 0 || px >= FogWidth || py < 0 || py >= FogHeight) continue;
                buf[py * FogWidth + px] = color;
            }
        }
    }

    // ── Terrain Sampling ───────────────────────────────

    /// <summary>
    /// Read the tile at (worldX, worldY) from the ChunkManager and return its minimap color.
    /// If the chunk doesn't exist yet, returns FogUnexplored.
    /// </summary>
    private Color32 SampleTerrainColor(int worldX, int worldY)
    {
        if (chunkManager == null)
        {
            chunkManager = ChunkManager.Instance;
            if (chunkManager == null) return UnknownTerrain;
        }

        var chunks = chunkManager.GetAllChunks();
        if (chunks == null) return UnknownTerrain;

        int cx = Mathf.FloorToInt((float)worldX / chunkSize);
        int cy = Mathf.FloorToInt((float)worldY / chunkSize);
        var coord = new Vector2Int(cx, cy);

        if (!chunks.TryGetValue(coord, out ChunkData chunk))
            return FogUnexplored;

        int localX = worldX - cx * chunkSize;
        int localY = worldY - cy * chunkSize;
        if (localX < 0 || localX >= chunkSize || localY < 0 || localY >= chunkSize)
            return FogUnexplored;

        int tileID = chunk.tileIDs[localX, localY];
        return TileIDToColor(tileID);
    }

    private static Color32 TileIDToColor(int tileID)
    {
        if (tileID < 0) return AirColor;
        if (tileID < TileColors.Length) return TileColors[tileID];
        return UnknownTerrain;
    }

    // ── Minimap View ────────────────────────────────────

    private void RenderMinimapView()
    {
        if (playerTransform == null) return;

        float worldPerMinimapPixel = (minimapViewRadius * 2f) / MinimapTexSize;
        int halfSize = MinimapTexSize / 2;

        for (int my = 0; my < MinimapTexSize; my++)
        {
            for (int mx = 0; mx < MinimapTexSize; mx++)
            {
                float worldX = playerTransform.position.x + (mx - halfSize) * worldPerMinimapPixel;
                float worldY = playerTransform.position.y + (my - halfSize) * worldPerMinimapPixel;

                int texX = WorldToTexX(worldX);
                int texY = WorldToTexY(worldY);

                Color32 c;
                if (texX < 0 || texX >= FogWidth || texY < 0 || texY >= FogHeight)
                {
                    c = FogUnexplored;
                }
                else
                {
                    Color32 fogSample = fogPixels[texY * FogWidth + texX];

                    // If the fog pixel at this world position is still unexplored,
                    // try a direct terrain sample (handles newly-generated chunks)
                    if (ColorEquals(fogSample, FogUnexplored))
                    {
                        int sampleWorldX = Mathf.FloorToInt(worldX);
                        int sampleWorldY = Mathf.FloorToInt(worldY);
                        Color32 directSample = SampleTerrainColor(sampleWorldX, sampleWorldY);
                        c = ColorEquals(directSample, FogUnexplored) ? fogSample : directSample;
                    }
                    else
                    {
                        c = fogSample;
                    }
                }

                minimapPixels[my * MinimapTexSize + mx] = c;
            }
        }

        // Player dot at center
        DrawMinimapDot(halfSize, halfSize, 2, PlayerDot, true);

        // NPC dots
        var npcs = FindObjectsOfType<NPC>();
        foreach (var npc in npcs)
        {
            float relX = npc.transform.position.x - playerTransform.position.x;
            float relY = npc.transform.position.y - playerTransform.position.y;
            int mx = halfSize + Mathf.RoundToInt(relX / worldPerMinimapPixel);
            int my = halfSize + Mathf.RoundToInt(relY / worldPerMinimapPixel);
            DrawMinimapDot(mx, my, 1, NpcDot, false);
        }

        // Enemy dots — single pixel
        var enemies = FindObjectsOfType<EnemyBase>();
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            float relX = enemy.transform.position.x - playerTransform.position.x;
            float relY = enemy.transform.position.y - playerTransform.position.y;
            int mx = halfSize + Mathf.RoundToInt(relX / worldPerMinimapPixel);
            int my = halfSize + Mathf.RoundToInt(relY / worldPerMinimapPixel);
            if (mx >= 0 && mx < MinimapTexSize && my >= 0 && my < MinimapTexSize)
                minimapPixels[my * MinimapTexSize + mx] = EnemyDot;
        }

        minimapTex.SetPixels32(minimapPixels);
        minimapTex.Apply();

        if (minimapRawImage != null)
            minimapRawImage.texture = minimapTex;
    }

    private void DrawMinimapDot(int cx, int cy, int radius, Color32 color, bool crosshair)
    {
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (crosshair && (Mathf.Abs(dx) < 1 && Mathf.Abs(dy) < 1))
                    continue; // crosshair skips center
                int px = cx + dx;
                int py = cy + dy;
                if (px < 0 || px >= MinimapTexSize || py < 0 || py >= MinimapTexSize) continue;
                minimapPixels[py * MinimapTexSize + px] = color;
            }
        }
    }

    // ── UI Construction ────────────────────────────────

    private void CreateMinimapUI(Canvas canvas)
    {
        var existing = canvas.transform.Find("MinimapPanel");
        if (existing != null)
        {
            minimapPanel = existing.gameObject;
            minimapRawImage = minimapPanel.GetComponentInChildren<RawImage>();
            return;
        }

        minimapPanel = new GameObject("MinimapPanel", typeof(RectTransform), typeof(Image));
        minimapPanel.transform.SetParent(canvas.transform, false);
        var rt = minimapPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-16, -16);
        rt.sizeDelta = new Vector2(minimapPanelSize, minimapPanelSize);
        minimapPanel.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.08f, 0.85f);

        // Border
        var border = new GameObject("Border", typeof(RectTransform), typeof(Image));
        border.transform.SetParent(minimapPanel.transform, false);
        var bRt = border.GetComponent<RectTransform>();
        bRt.anchorMin = Vector2.zero; bRt.anchorMax = Vector2.one;
        bRt.offsetMin = new Vector2(-2, -2); bRt.offsetMax = new Vector2(2, 2);
        border.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.35f, 0.8f);
        border.transform.SetAsFirstSibling();

        // RawImage
        var rawGo = new GameObject("MinimapImage", typeof(RectTransform), typeof(RawImage));
        rawGo.transform.SetParent(minimapPanel.transform, false);
        var rawRt = rawGo.GetComponent<RectTransform>();
        rawRt.anchorMin = Vector2.zero; rawRt.anchorMax = Vector2.one;
        rawRt.offsetMin = new Vector2(3, 3); rawRt.offsetMax = new Vector2(-3, -3);
        minimapRawImage = rawGo.GetComponent<RawImage>();
        minimapRawImage.texture = minimapTex;

        // Label
        CreateLabel(minimapPanel.transform, "MAP", new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 3), new Vector2(60, 16), 10, TextAnchor.LowerCenter);
    }

    private void CreateFullMapUI(Canvas canvas)
    {
        var existing = canvas.transform.Find("FullMapPanel");
        if (existing != null)
        {
            fullMapPanel = existing.gameObject;
            fullMapRawImage = fullMapPanel.transform.Find("MapImage")?.GetComponent<RawImage>();
            fullMapLabel = fullMapPanel.transform.Find("MapLabel")?.GetComponent<Text>();
            fullMapPanel.SetActive(false);
            return;
        }

        fullMapPanel = new GameObject("FullMapPanel", typeof(RectTransform), typeof(Image));
        fullMapPanel.transform.SetParent(canvas.transform, false);
        var rt = fullMapPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(fullMapWidth + 16, fullMapHeight + 40);
        fullMapPanel.GetComponent<Image>().color = new Color(0.02f, 0.02f, 0.05f, 0.95f);

        var border = new GameObject("Border", typeof(RectTransform), typeof(Image));
        border.transform.SetParent(fullMapPanel.transform, false);
        var bRt = border.GetComponent<RectTransform>();
        bRt.anchorMin = Vector2.zero; bRt.anchorMax = Vector2.one;
        bRt.offsetMin = new Vector2(-3, -3); bRt.offsetMax = new Vector2(3, 3);
        border.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.35f, 0.9f);
        border.transform.SetAsFirstSibling();

        var rawGo = new GameObject("MapImage", typeof(RectTransform), typeof(RawImage));
        rawGo.transform.SetParent(fullMapPanel.transform, false);
        var rawRt = rawGo.GetComponent<RectTransform>();
        rawRt.anchorMin = new Vector2(0.5f, 0.5f); rawRt.anchorMax = new Vector2(0.5f, 0.5f);
        rawRt.pivot = new Vector2(0.5f, 0.5f);
        rawRt.anchoredPosition = new Vector2(0, 10);
        rawRt.sizeDelta = new Vector2(fullMapWidth, fullMapHeight);
        fullMapRawImage = rawGo.GetComponent<RawImage>();
        fullMapRawImage.texture = fogTexture;

        fullMapLabel = CreateLabel(fullMapPanel.transform, "World Map — Press M to close",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 6), new Vector2(fullMapWidth, 22), 13, TextAnchor.LowerCenter);

        // Legend
        var legend = new GameObject("Legend", typeof(RectTransform));
        legend.transform.SetParent(fullMapPanel.transform, false);
        var lRt = legend.GetComponent<RectTransform>();
        lRt.anchorMin = new Vector2(1, 0); lRt.anchorMax = new Vector2(1, 0);
        lRt.pivot = new Vector2(1, 0);
        lRt.anchoredPosition = new Vector2(-8, 2);
        lRt.sizeDelta = new Vector2(280, 40);

        float yRow0 = 16f;
        float yRow1 = 0f;
        CreateLegendDot(legend.transform, PlayerDot, "Player", 0, yRow0);
        CreateLegendDot(legend.transform, NpcDot, "NPC", 65, yRow0);
        CreateLegendDot(legend.transform, EnemyDot, "Enemy", 120, yRow0);
        CreateLegendDot(legend.transform, TileColors[0], "Grass", 180, yRow0);

        CreateLegendDot(legend.transform, TileColors[1], "Dirt", 0, yRow1);
        CreateLegendDot(legend.transform, TileColors[2], "Stone", 65, yRow1);
        CreateLegendDot(legend.transform, AirColor, "Air", 120, yRow1);

        // Enemy toggle button (bottom-left)
        CreateEnemyToggleButton(fullMapPanel.transform);

        fullMapPanel.SetActive(false);
    }

    private Text CreateLabel(Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPos, Vector2 sizeDelta, int fontSize, TextAnchor alignment)
    {
        var go = new GameObject("Label", typeof(RectTransform));
        go.AddComponent<Text>();
        go.transform.SetParent(parent, false);

        var txt = go.GetComponent<Text>();
        txt.text = text;
        txt.fontSize = fontSize;
        txt.color = new Color(0.7f, 0.7f, 0.8f);
        txt.alignment = alignment;
        txt.raycastTarget = false;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.pivot = anchorMin;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        return txt;
    }

    private void CreateLegendDot(Transform parent, Color32 color, string label, float xOffset, float yOffset)
    {
        var dot = new GameObject("Dot_" + label, typeof(RectTransform), typeof(Image));
        dot.transform.SetParent(parent, false);
        var drt = dot.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0); drt.anchorMax = new Vector2(0, 0);
        drt.pivot = new Vector2(0, 0);
        drt.anchoredPosition = new Vector2(xOffset, yOffset);
        drt.sizeDelta = new Vector2(8, 8);
        dot.GetComponent<Image>().color = color;
        dot.GetComponent<Image>().raycastTarget = false;

        var lbl = new GameObject("Text", typeof(RectTransform));
        lbl.AddComponent<Text>();
        lbl.transform.SetParent(dot.transform, false);
        var ltxt = lbl.GetComponent<Text>();
        ltxt.text = " " + label;
        ltxt.fontSize = 10;
        ltxt.color = new Color(0.6f, 0.6f, 0.7f);
        ltxt.alignment = TextAnchor.MiddleLeft;
        ltxt.raycastTarget = false;
        var lrt = lbl.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0, 0); lrt.anchorMax = new Vector2(0, 0);
        lrt.pivot = new Vector2(0, 0);
        lrt.anchoredPosition = new Vector2(10, 0);
        lrt.sizeDelta = new Vector2(60, 14);
    }

    private void CreateEnemyToggleButton(Transform parent)
    {
        var btnGo = new GameObject("EnemyToggleBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(parent, false);
        var bRt = btnGo.GetComponent<RectTransform>();
        bRt.anchorMin = new Vector2(0, 0); bRt.anchorMax = new Vector2(0, 0);
        bRt.pivot = new Vector2(0, 0);
        bRt.anchoredPosition = new Vector2(8, 2);
        bRt.sizeDelta = new Vector2(130, 22);

        var btnImg = btnGo.GetComponent<Image>();
        btnImg.color = new Color(0.25f, 0.25f, 0.3f);

        var btn = btnGo.GetComponent<Button>();
        btn.onClick.AddListener(ToggleEnemyDisplay);

        var labelGo = new GameObject("Label", typeof(RectTransform));
        labelGo.AddComponent<Text>();
        labelGo.transform.SetParent(btnGo.transform, false);
        var lTxt = labelGo.GetComponent<Text>();
        lTxt.text = "Show Enemies: OFF";
        lTxt.fontSize = 11;
        lTxt.color = new Color(0.7f, 0.7f, 0.7f);
        lTxt.alignment = TextAnchor.MiddleCenter;
        lTxt.raycastTarget = false;
        var lRt = labelGo.GetComponent<RectTransform>();
        lRt.anchorMin = Vector2.zero; lRt.anchorMax = Vector2.one;
        lRt.sizeDelta = Vector2.zero;
    }

    private void ToggleEnemyDisplay()
    {
        showEnemiesOnFullMap = !showEnemiesOnFullMap;

        if (fullMapRawImage != null)
            fullMapRawImage.texture = showEnemiesOnFullMap ? fogTexture : fogTexNoEnemies;

        // Update button label
        if (fullMapPanel != null)
        {
            var btn = fullMapPanel.transform.Find("EnemyToggleBtn");
            if (btn != null)
            {
                var label = btn.Find("Label")?.GetComponent<Text>();
                if (label != null)
                    label.text = showEnemiesOnFullMap ? "Show Enemies: ON" : "Show Enemies: OFF";
            }
        }
    }

    // ── Full Map Toggle ────────────────────────────────

    private void ToggleFullMap()
    {
        isFullMapOpen = !isFullMapOpen;
        if (fullMapPanel != null)
            fullMapPanel.SetActive(isFullMapOpen);
        if (minimapPanel != null)
            minimapPanel.SetActive(!isFullMapOpen);
    }

    public bool IsFullMapOpen => isFullMapOpen;

    // ── Helpers ────────────────────────────────────────

    private int WorldToTexX(float worldX)
    {
        return Mathf.Clamp(Mathf.RoundToInt(worldX / WorldUnitsPerPixel), 0, FogWidth - 1);
    }

    private int WorldToTexY(float worldY)
    {
        return Mathf.Clamp(Mathf.RoundToInt(worldY / WorldUnitsPerPixel), 0, FogHeight - 1);
    }

    private static bool ColorEquals(Color32 a, Color32 b)
    {
        return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
    }
}
