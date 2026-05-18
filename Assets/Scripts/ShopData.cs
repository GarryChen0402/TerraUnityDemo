using UnityEngine;

[System.Serializable]
public class ShopItemEntry
{
    public ItemData item;
    public int amount = -1; // -1 = unlimited stock
}

[CreateAssetMenu(fileName = "NewShop", menuName = "TerraUnity/Shop Data")]
public class ShopData : ScriptableObject
{
    public string shopName = "General Store";
    public ShopItemEntry[] itemsForSale;
    [Range(0.1f, 1f)] public float sellPriceMultiplier = 0.5f;

    public long GetBuyPrice(ItemData item)
    {
        if (item == null) return 0;
        return item.coinValue;
    }

    public long GetSellPrice(ItemData item)
    {
        if (item == null || item.coinValue <= 0) return 0;
        return (long)Mathf.Max(1, (long)(item.coinValue * sellPriceMultiplier));
    }
}
