using UnityEngine;

public class CraftingStation : MonoBehaviour
{
    public CraftingStationType stationType;
    public float interactionRange = 2f;
    public SpriteRenderer spriteRenderer;

    private bool playerInRange;

    private void Update()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.transform.position);
        playerInRange = dist <= interactionRange;

        if (spriteRenderer != null)
            spriteRenderer.color = playerInRange ? Color.yellow : Color.white;
    }

    public bool IsPlayerInRange() => playerInRange;
}
