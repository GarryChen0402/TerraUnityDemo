using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName ="TileData", menuName ="TerraUnity/Tile Data")]
public class TileData : ScriptableObject
{
    public TileBase tile;
    public string tileName;
    public float hardness = 1f;
    public int requiredMiningLevel = 0;
    public ItemData dropItem;
    public int dropAmount = 1;
}
