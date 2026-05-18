using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform slotGrid;
    [SerializeField] private KeyCode toggleKey = KeyCode.B;

    private bool isOpen;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        ConfigureSlots();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isOpen = !isOpen;
            if (inventoryPanel != null)
                inventoryPanel.SetActive(isOpen);
        }

        if (isOpen)
            RefreshSlots();
    }

    private void ConfigureSlots()
    {
        for (int i = 0; i < slotGrid.childCount; i++)
        {
            Transform slot = slotGrid.GetChild(i);

            // Icon: centered, smaller than slot with padding
            RectTransform iconRt = slot.Find("Icon")?.GetComponent<RectTransform>();
            if (iconRt != null)
            {
                iconRt.anchorMin = new Vector2(0.5f, 0.5f);
                iconRt.anchorMax = new Vector2(0.5f, 0.5f);
                iconRt.pivot = new Vector2(0.5f, 0.5f);
                iconRt.anchoredPosition = Vector2.zero;
                iconRt.sizeDelta = new Vector2(40, 40);
            }

            // Count: bottom-right corner
            RectTransform countRt = slot.Find("Count")?.GetComponent<RectTransform>();
            if (countRt != null)
            {
                countRt.anchorMin = new Vector2(1, 0);
                countRt.anchorMax = new Vector2(1, 0);
                countRt.pivot = new Vector2(1, 0);
                countRt.anchoredPosition = new Vector2(2, 2);
                countRt.sizeDelta = new Vector2(50, 20);

                TMP_Text countText = countRt.GetComponent<TMP_Text>();
                if (countText != null)
                {
                    countText.alignment = TextAlignmentOptions.BottomRight;
                    countText.fontSize = 14;
                }
            }
        }
    }

    private void RefreshSlots()
    {
        if (PlayerInventory.Instance == null) return;

        for (int i = 0; i < slotGrid.childCount; i++)
        {
            Transform slot = slotGrid.GetChild(i);
            Image icon = slot.Find("Icon")?.GetComponent<Image>();
            TMP_Text countText = slot.Find("Count")?.GetComponent<TMP_Text>();

            if (i < PlayerInventory.Instance.slots.Count)
            {
                InventorySlot data = PlayerInventory.Instance.slots[i];
                if (data.itemData != null)
                {
                    if (icon != null)
                    {
                        icon.sprite = data.itemData.icon;
                        icon.enabled = true;
                    }
                    if (countText != null)
                        countText.text = data.amount > 1 ? data.amount.ToString() : "";
                }
                else
                {
                    if (icon != null) icon.enabled = false;
                    if (countText != null) countText.text = "";
                }
            }
        }
    }

    public void ForceRefresh()
    {
        RefreshSlots();
    }
}
