using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    public ItemData itemData;
    public int amount = 1;

    [SerializeField] private float pickupRange = 1f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float pickupDelay = 0.5f;

    private Transform playerTransform;
    private bool canPickup;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        Invoke(nameof(EnablePickup), pickupDelay);

        if (itemData != null && itemData.icon != null)
            spriteRenderer.sprite = itemData.icon;
    }

    private void EnablePickup() => canPickup = true;

    private void Update()
    {
        if (!canPickup || playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer < pickupRange * 3f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                playerTransform.position,
                moveSpeed * Time.deltaTime
            );
        }

        if (distanceToPlayer < 0.3f)
        {
            PickupItem();
        }
    }

    private void PickupItem()
    {
        if (itemData.coinValue > 0 && PlayerCurrency.Instance != null)
            PlayerCurrency.Instance.Earn(itemData.coinValue * amount);

        if (PlayerInventory.Instance != null)
        {
            int added = PlayerInventory.Instance.AddItem(itemData, amount);
            if (added >= amount)
            {
                Debug.Log($"Picked up {itemData.itemName} x{amount}");
                Destroy(gameObject);
                return;
            }

            amount -= added;
            Debug.Log($"Picked up {itemData.itemName} x{added}, inventory full — {amount} left on ground");
        }
        else
        {
            Debug.Log($"Picked up {itemData.itemName} x{amount}");
            Destroy(gameObject);
        }
    }
}
