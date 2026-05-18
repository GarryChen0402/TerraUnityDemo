using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [SerializeField] private int inventorySize = 40;
    public List<InventorySlot> slots = new List<InventorySlot>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        for (int i = 0; i < inventorySize; i++)
            slots.Add(new InventorySlot());
    }

    public int AddItem(ItemData item, int amount)
    {
        int remaining = amount;

        foreach (var slot in slots)
        {
            if (remaining <= 0) break;
            if (slot.itemData == item && slot.amount < item.maxStack)
            {
                int canAdd = item.maxStack - slot.amount;
                int actualAdd = Mathf.Min(canAdd, remaining);
                slot.amount += actualAdd;
                remaining -= actualAdd;
            }
        }

        foreach (var slot in slots)
        {
            if (remaining <= 0) break;
            if (slot.itemData == null)
            {
                int actualAdd = Mathf.Min(item.maxStack, remaining);
                slot.itemData = item;
                slot.amount = actualAdd;
                remaining -= actualAdd;
            }
        }

        int addedAmount = amount - remaining;
        if (addedAmount > 0)
            Debug.Log($"Picked up {item.itemName} x{addedAmount}");

        return addedAmount;
    }

    public bool ConsumeItem(ItemData item, int amount)
    {
        if (GetItemCount(item) < amount) return false;

        int remaining = amount;
        foreach (var slot in slots)
        {
            if (remaining <= 0) break;
            if (slot.itemData == item)
            {
                int consume = Mathf.Min(slot.amount, remaining);
                slot.amount -= consume;
                remaining -= consume;
                if (slot.amount <= 0)
                    slot.itemData = null;
            }
        }
        return true;
    }

    public int GetItemCount(ItemData item)
    {
        int count = 0;
        foreach (var slot in slots)
            if (slot.itemData == item)
                count += slot.amount;
        return count;
    }

    public void ClearAll()
    {
        foreach (var slot in slots)
        {
            slot.itemData = null;
            slot.amount = 0;
        }
    }

    public void LoadSlot(int index, ItemData item, int amount)
    {
        if (index < 0 || index >= slots.Count) return;
        slots[index].itemData = item;
        slots[index].amount = amount;
    }
}

[System.Serializable]
public class InventorySlot
{
    public ItemData itemData;
    public int amount;
}
