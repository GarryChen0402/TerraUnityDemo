using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "TerraUnity/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public int itemID;
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Stack")]
    public int maxStack = 999;

    [Header("Type")]
    public ItemType itemType;
    public ItemRarity rarity = ItemRarity.Common;

    [Header("Coin Value")]
    public int coinValue = 0;

    [Header("Tool")]
    public int miningPower = 0;
    public int miningLevel = 0;

    [Header("Weapon")]
    public int damage = 0;
    public float attackSpeed = 1f;
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

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public static class RarityColors
{
    public static Color GetColor(ItemRarity rarity) => rarity switch
    {
        ItemRarity.Common => Color.white,
        ItemRarity.Uncommon => Color.green,
        ItemRarity.Rare => new Color(0.3f, 0.5f, 1f),
        ItemRarity.Epic => new Color(0.7f, 0.2f, 1f),
        ItemRarity.Legendary => new Color(1f, 0.6f, 0f),
        _ => Color.white
    };
}
