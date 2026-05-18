using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "TerraUnity/Item Data")]
public class ItemData : ScriptableObject
{
    public int itemID;
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    public int maxStack = 999;

    public ItemType itemType;

    [Header("Tool")]
    public int miningPower = 0;
    public int miningLevel = 0;

    [Header("Weapon")]
    public int damage = 0;
}

public enum ItemType
{
    Material,
    Tool,
    Weapon,
    Armor,
    Consumable,
    Misc
}
