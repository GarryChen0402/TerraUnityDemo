using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "TerraUnity/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("Output")]
    public ItemData outputItem;
    public int outputAmount = 1;

    [Header("Ingredients")]
    public CraftingIngredient[] ingredients;

    [Header("Required Station")]
    public CraftingStationType requiredStation = CraftingStationType.Hand;

    public bool CanCraft(PlayerInventory inventory)
    {
        foreach (var ing in ingredients)
        {
            if (inventory.GetItemCount(ing.item) < ing.amount)
                return false;
        }
        return true;
    }

    public bool Craft(PlayerInventory inventory)
    {
        if (!CanCraft(inventory)) return false;

        foreach (var ing in ingredients)
            inventory.ConsumeItem(ing.item, ing.amount);

        inventory.AddItem(outputItem, outputAmount);
        Debug.Log($"Crafted {outputItem.itemName} x{outputAmount}");
        return true;
    }
}

[System.Serializable]
public class CraftingIngredient
{
    public ItemData item;
    public int amount;
}

public enum CraftingStationType { Hand, Workbench, Furnace, Anvil }
