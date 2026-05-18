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
}
