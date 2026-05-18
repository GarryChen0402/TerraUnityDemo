using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class TileDataManager : MonoBehaviour
{
    public static TileDataManager Instance { get; private set; }

    [SerializeField] private List<TileData> tileDataList;
    private Dictionary<TileBase, TileData> tileDataMap;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        tileDataMap = new Dictionary<TileBase, TileData>();
        foreach (var data in tileDataList)
        {
            if (data.tile != null)
                tileDataMap[data.tile] = data;
        }
    }

    public TileData GetTileData(TileBase tile)
    {
        tileDataMap.TryGetValue(tile, out var data);
        return data;
    }

    public IReadOnlyList<TileData> AllTileData => tileDataList;
}
