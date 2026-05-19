using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

/// <summary>
/// Auto-configures Week 10 systems before the first scene loads.
/// Fills in any missing scene references at runtime.
/// </summary>
public static class RuntimeWeek10Setup
{
    private static bool configured;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoad()
    {
        // Hook into the first scene load
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene,
        UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (configured) return;
        configured = true;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;

        // Defer to first frame so other Awake methods have run
        var go = new GameObject("__Week10RuntimeSetup");
        go.AddComponent<Week10SetupBehaviour>();
        Object.DontDestroyOnLoad(go);
    }

    private class Week10SetupBehaviour : MonoBehaviour
    {
        private void Start()
        {
            EnsureDayNightManager();
            EnsureNightOverlay();
            EnsureTilemapFurniture();
            EnsureRoomDetectorExists();
            EnsureShopUIExists();
            EnsureSaveManagerExists();
            EnsureEnemySpawnerExists();
            EnsureMinimapExists();
            LinkRoomDetectorReferences();
            Destroy(gameObject);
        }

        private void EnsureDayNightManager()
        {
            if (Object.FindObjectOfType<DayNightCycle>() != null) return;
            var go = new GameObject("DayNightManager");
            go.AddComponent<DayNightCycle>();
        }

        private void EnsureNightOverlay()
        {
            var dn = Object.FindObjectOfType<DayNightCycle>();
            if (dn == null) return;

            var field = typeof(DayNightCycle).GetField("nightOverlay",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field == null) return;
            if (field.GetValue(dn) is Image existing && existing != null) return;

            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null) return;

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

            field.SetValue(dn, img);
        }

        private void EnsureTilemapFurniture()
        {
            var grid = Object.FindObjectOfType<Grid>();
            if (grid == null) return;

            var existing = grid.transform.Find("Tilemap_Furniture");
            if (existing != null) return;

            var go = new GameObject("Tilemap_Furniture", typeof(Tilemap), typeof(TilemapRenderer));
            go.transform.SetParent(grid.transform, false);

            var renderer = go.GetComponent<TilemapRenderer>();
            renderer.sortingOrder = 1;
        }

        private void EnsureRoomDetectorExists()
        {
            if (Object.FindObjectOfType<RoomDetector>() != null) return;

            var worldManager = GameObject.Find("WorldManager");
            if (worldManager == null)
            {
                Debug.LogWarning("[Week10Setup] WorldManager not found — creating standalone RoomDetector.");
                var go = new GameObject("RoomDetector");
                go.AddComponent<RoomDetector>();
            }
            else
            {
                worldManager.AddComponent<RoomDetector>();
            }
        }

        private void EnsureShopUIExists()
        {
            if (Object.FindObjectOfType<ShopUI>() != null) return;
            var go = new GameObject("ShopUI");
            go.AddComponent<ShopUI>();
        }

        private void EnsureSaveManagerExists()
        {
            if (Object.FindObjectOfType<SaveManager>() != null) return;
            var go = new GameObject("SaveManager");
            go.AddComponent<SaveManager>();
        }

        private void EnsureEnemySpawnerExists()
        {
            if (Object.FindObjectOfType<EnemySpawner>() != null) return;
            var go = new GameObject("EnemySpawner");
            go.AddComponent<EnemySpawner>();
        }

        private void EnsureMinimapExists()
        {
            if (Object.FindObjectOfType<Minimap>() != null) return;
            var go = new GameObject("Minimap");
            go.AddComponent<Minimap>();
        }

        private void LinkRoomDetectorReferences()
        {
            var rd = Object.FindObjectOfType<RoomDetector>();
            if (rd == null) return;

            // Link groundTilemap
            var gtField = typeof(RoomDetector).GetField("groundTilemap",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (gtField != null && gtField.GetValue(rd) == null)
            {
                var grid = Object.FindObjectOfType<Grid>();
                if (grid != null)
                {
                    var gt = grid.transform.Find("Tilemap_Ground");
                    if (gt != null)
                        gtField.SetValue(rd, gt.GetComponent<Tilemap>());
                }
            }

            // Link furnitureTilemap
            var ftField = typeof(RoomDetector).GetField("furnitureTilemap",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (ftField != null && ftField.GetValue(rd) == null)
            {
                var grid = Object.FindObjectOfType<Grid>();
                if (grid != null)
                {
                    var ft = grid.transform.Find("Tilemap_Furniture");
                    if (ft != null)
                        ftField.SetValue(rd, ft.GetComponent<Tilemap>());
                }
            }
        }
    }
}
