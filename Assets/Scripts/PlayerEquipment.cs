using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    public static PlayerEquipment Instance { get; private set; }

    public ItemData helmet;
    public ItemData chestplate;
    public ItemData leggings;
    public ItemData boots;
    public ItemData offhand;
    public ItemData accessory1;
    public ItemData accessory2;
    public ItemData weapon;
    public ItemData tool;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool Equip(ItemData item)
    {
        if (item.itemType != ItemType.Tool && item.itemType != ItemType.Weapon
            && item.itemType != ItemType.Armor) return false;

        switch (item.itemType)
        {
            case ItemType.Weapon: weapon = item; break;
            case ItemType.Tool: tool = item; break;
            case ItemType.Armor: chestplate = item; break;
            default: return false;
        }
        return true;
    }

    public int GetAttackBonus()
    {
        int bonus = 0;
        if (weapon != null) bonus += weapon.damage;
        return bonus;
    }

    public int GetDefense()
    {
        int defense = 0;
        if (helmet != null && helmet.itemType == ItemType.Armor) defense += helmet.damage;
        if (chestplate != null && chestplate.itemType == ItemType.Armor) defense += chestplate.damage;
        if (leggings != null && leggings.itemType == ItemType.Armor) defense += leggings.damage;
        if (boots != null && boots.itemType == ItemType.Armor) defense += boots.damage;
        return defense;
    }

    public int GetMiningPower()
    {
        if (tool != null) return tool.miningPower;
        return 1; // bare hands
    }

    public int GetMiningLevel()
    {
        if (tool != null) return tool.miningLevel;
        return 0;
    }

    public void LoadFromItemIDs(int[] ids, System.Collections.Generic.Dictionary<int, ItemData> itemDB)
    {
        ItemData Resolve(int idx) => idx >= 0 && idx < ids.Length && ids[idx] >= 0 && itemDB.TryGetValue(ids[idx], out var item) ? item : null;

        helmet = Resolve(0);
        chestplate = Resolve(1);
        leggings = Resolve(2);
        boots = Resolve(3);
        offhand = Resolve(4);
        accessory1 = Resolve(5);
        accessory2 = Resolve(6);
        weapon = Resolve(7);
        tool = Resolve(8);
    }

    public int[] GetEquipmentItemIDs()
    {
        return new int[]
        {
            helmet != null ? helmet.itemID : -1,
            chestplate != null ? chestplate.itemID : -1,
            leggings != null ? leggings.itemID : -1,
            boots != null ? boots.itemID : -1,
            offhand != null ? offhand.itemID : -1,
            accessory1 != null ? accessory1.itemID : -1,
            accessory2 != null ? accessory2.itemID : -1,
            weapon != null ? weapon.itemID : -1,
            tool != null ? tool.itemID : -1,
        };
    }
}
