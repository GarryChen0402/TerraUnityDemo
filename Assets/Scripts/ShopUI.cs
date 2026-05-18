using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;

    public static bool IsOpen { get; private set; }

    private NPC activeNPC;
    private ShopData currentShop;
    private TMP_Text titleText;
    private TMP_Text coinText;
    private Transform gridContent;
    private GameObject selectedInfoPanel;
    private Image selectedInfoIcon;
    private TMP_Text selectedInfoText;
    private Button buyButton;
    private TMP_Text buyButtonLabel;

    private ShopItemEntry selectedEntry;
    private GameObject selectedEntryGo;

    private const float PanelWidth = 280f;
    private const float PanelHeight = 420f;
    private const float TitleHeight = 38f;
    private const float CoinHeight = 22f;
    private const float GridCellSize = 60f;
    private const float GridSpacing = 4f;
    private const float InfoAreaHeight = 48f;
    private const float BottomButtonHeight = 44f;

    private static readonly Color EntryNormal = new Color(0.15f, 0.15f, 0.18f);
    private static readonly Color EntrySelected = new Color(0.22f, 0.5f, 0.22f);

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        IsOpen = false;
    }

    private void Start()
    {
        if (shopPanel == null)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                var t = canvas.transform.Find("ShopPanel");
                if (t != null) shopPanel = t.gameObject;
            }
        }

        if (shopPanel == null)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                shopPanel = new GameObject("ShopPanel", typeof(RectTransform), typeof(Image));
                shopPanel.transform.SetParent(canvas.transform, false);

                var rt = shopPanel.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
            }
        }

        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
            BuildPanel();
        }
    }

    private void Update()
    {
        if (IsOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                CloseShop();

            if (activeNPC != null && !activeNPC.IsPlayerInRange())
                CloseShop();
        }
    }

    private void BuildPanel()
    {
        var panelRt = shopPanel.GetComponent<RectTransform>();
        panelRt.sizeDelta = new Vector2(PanelWidth, PanelHeight);

        var panelBg = shopPanel.GetComponent<Image>();
        if (panelBg == null)
            panelBg = shopPanel.AddComponent<Image>();
        panelBg.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);

        // ---- Top area ----

        // Title
        var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGo.transform.SetParent(shopPanel.transform, false);
        titleText = titleGo.GetComponent<TextMeshProUGUI>();
        titleText.fontSize = 18;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.text = "Shop";
        var titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0.5f, 1);
        titleRt.anchoredPosition = new Vector2(0, -6);
        titleRt.sizeDelta = new Vector2(0, TitleHeight);

        // Coin display
        var coinGo = new GameObject("CoinDisplay", typeof(RectTransform), typeof(TextMeshProUGUI));
        coinGo.transform.SetParent(shopPanel.transform, false);
        coinText = coinGo.GetComponent<TextMeshProUGUI>();
        coinText.fontSize = 13;
        coinText.color = new Color(1f, 0.85f, 0.3f);
        coinText.alignment = TextAlignmentOptions.Center;
        var coinRt = coinGo.GetComponent<RectTransform>();
        coinRt.anchorMin = new Vector2(0, 1);
        coinRt.anchorMax = new Vector2(1, 1);
        coinRt.pivot = new Vector2(0.5f, 1);
        coinRt.anchoredPosition = new Vector2(0, -46);
        coinRt.sizeDelta = new Vector2(0, CoinHeight);

        // Close button
        var closeGo = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        closeGo.transform.SetParent(shopPanel.transform, false);
        var closeRt = closeGo.GetComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(1, 1);
        closeRt.anchorMax = new Vector2(1, 1);
        closeRt.pivot = new Vector2(1, 1);
        closeRt.anchoredPosition = new Vector2(-4, -4);
        closeRt.sizeDelta = new Vector2(22, 22);
        closeGo.GetComponent<Image>().color = new Color(0.6f, 0.2f, 0.2f);
        closeGo.GetComponent<Button>().onClick.AddListener(CloseShop);
        var closeLabel = new GameObject("X", typeof(RectTransform), typeof(TextMeshProUGUI));
        closeLabel.transform.SetParent(closeGo.transform, false);
        var closeLblText = closeLabel.GetComponent<TextMeshProUGUI>();
        closeLblText.text = "X";
        closeLblText.fontSize = 14;
        closeLblText.color = Color.white;
        closeLblText.alignment = TextAlignmentOptions.Center;
        var closeLblRt = closeLabel.GetComponent<RectTransform>();
        closeLblRt.anchorMin = Vector2.zero;
        closeLblRt.anchorMax = Vector2.one;
        closeLblRt.sizeDelta = Vector2.zero;

        // ---- ScrollView (grid area) ----
        var scrollGo = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollGo.transform.SetParent(shopPanel.transform, false);
        var scrollRt = scrollGo.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 1);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.pivot = new Vector2(0.5f, 1);
        scrollRt.anchoredPosition = new Vector2(0, -74);
        scrollRt.sizeDelta = new Vector2(-8, 0);
        // Set height based on available space: panel - top - bottom
        float scrollHeight = PanelHeight - 74f - InfoAreaHeight - BottomButtonHeight - 8f;
        scrollRt.sizeDelta = new Vector2(-8, scrollHeight);
        scrollGo.GetComponent<Image>().color = new Color(0.06f, 0.06f, 0.1f, 0.5f);

        var scrollRect = scrollGo.GetComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        // Viewport
        var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewportGo.transform.SetParent(scrollGo.transform, false);
        var viewportRt = viewportGo.GetComponent<RectTransform>();
        viewportRt.anchorMin = Vector2.zero;
        viewportRt.anchorMax = Vector2.one;
        viewportRt.sizeDelta = Vector2.zero;
        viewportGo.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        viewportGo.GetComponent<Mask>().showMaskGraphic = false;

        // Content (grid)
        var contentGo = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup));
        contentGo.transform.SetParent(viewportGo.transform, false);
        var contentRt = contentGo.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0, 1);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = new Vector2(0, 0);

        var grid = contentGo.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(GridCellSize, GridCellSize);
        grid.spacing = new Vector2(GridSpacing, GridSpacing);
        grid.padding = new RectOffset(6, 6, 6, 6);
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;

        // Wire up ScrollRect
        scrollRect.viewport = viewportRt;
        scrollRect.content = contentRt;

        // Scrollbar
        var scrollbarGo = new GameObject("Scrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
        scrollbarGo.transform.SetParent(scrollGo.transform, false);
        var scrollbarRt = scrollbarGo.GetComponent<RectTransform>();
        scrollbarRt.anchorMin = new Vector2(1, 0);
        scrollbarRt.anchorMax = new Vector2(1, 1);
        scrollbarRt.pivot = new Vector2(1, 1);
        scrollbarRt.anchoredPosition = Vector2.zero;
        scrollbarRt.sizeDelta = new Vector2(8, 0);
        scrollbarGo.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f);

        var slidingArea = new GameObject("SlidingArea", typeof(RectTransform));
        slidingArea.transform.SetParent(scrollbarGo.transform, false);
        var slidingRt = slidingArea.GetComponent<RectTransform>();
        slidingRt.anchorMin = Vector2.zero;
        slidingRt.anchorMax = Vector2.one;
        slidingRt.sizeDelta = new Vector2(-2, -4);
        slidingRt.anchoredPosition = new Vector2(0, 0);

        var handleGo = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleGo.transform.SetParent(slidingArea.transform, false);
        var handleRt = handleGo.GetComponent<RectTransform>();
        handleRt.anchorMin = Vector2.zero;
        handleRt.anchorMax = Vector2.one;
        handleRt.sizeDelta = Vector2.zero;
        handleGo.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.5f);

        var scrollbar = scrollbarGo.GetComponent<Scrollbar>();
        scrollbar.handleRect = handleRt;
        scrollbar.targetGraphic = handleGo.GetComponent<Image>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollRect.verticalScrollbar = scrollbar;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

        gridContent = contentRt;

        // ---- Selected item info (below grid) ----
        var infoPanelGo = new GameObject("SelectedInfo", typeof(RectTransform), typeof(Image));
        infoPanelGo.transform.SetParent(shopPanel.transform, false);
        selectedInfoPanel = infoPanelGo;
        var infoPanelRt = infoPanelGo.GetComponent<RectTransform>();
        infoPanelRt.anchorMin = new Vector2(0, 1);
        infoPanelRt.anchorMax = new Vector2(1, 1);
        infoPanelRt.pivot = new Vector2(0.5f, 1);
        infoPanelRt.anchoredPosition = new Vector2(0, -74f - scrollHeight);
        infoPanelRt.sizeDelta = new Vector2(0, InfoAreaHeight);
        infoPanelGo.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.14f, 0.9f);

        // Info icon
        var infoIconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        infoIconGo.transform.SetParent(infoPanelGo.transform, false);
        selectedInfoIcon = infoIconGo.GetComponent<Image>();
        selectedInfoIcon.preserveAspect = true;
        var infoIconRt = infoIconGo.GetComponent<RectTransform>();
        infoIconRt.anchorMin = new Vector2(0, 0.5f);
        infoIconRt.anchorMax = new Vector2(0, 0.5f);
        infoIconRt.pivot = new Vector2(0, 0.5f);
        infoIconRt.anchoredPosition = new Vector2(8, 0);
        infoIconRt.sizeDelta = new Vector2(32, 32);

        // Info text
        var infoTextGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        infoTextGo.transform.SetParent(infoPanelGo.transform, false);
        selectedInfoText = infoTextGo.GetComponent<TextMeshProUGUI>();
        selectedInfoText.fontSize = 14;
        selectedInfoText.color = Color.white;
        selectedInfoText.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Midline;
        var infoTextRt = infoTextGo.GetComponent<RectTransform>();
        infoTextRt.anchorMin = Vector2.zero;
        infoTextRt.anchorMax = Vector2.one;
        infoTextRt.offsetMin = new Vector2(46, 0);
        infoTextRt.offsetMax = new Vector2(-8, 0);
        selectedInfoText.text = "";

        selectedInfoPanel.SetActive(false);

        // ---- Buy button ----
        var btnGo = new GameObject("BuyButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(shopPanel.transform, false);
        var btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0);
        btnRt.anchorMax = new Vector2(0.5f, 0);
        btnRt.pivot = new Vector2(0.5f, 0);
        btnRt.anchoredPosition = new Vector2(0, 6);
        btnRt.sizeDelta = new Vector2(PanelWidth - 16, BottomButtonHeight);

        buyButton = btnGo.GetComponent<Button>();
        buyButton.onClick.AddListener(OnBuyClicked);
        buyButton.interactable = false;
        buyButton.image.color = new Color(0.35f, 0.35f, 0.35f);

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(btnGo.transform, false);
        buyButtonLabel = labelGo.GetComponent<TextMeshProUGUI>();
        buyButtonLabel.text = "Select an item";
        buyButtonLabel.fontSize = 15;
        buyButtonLabel.color = Color.white;
        buyButtonLabel.alignment = TextAlignmentOptions.Center;
        var labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.sizeDelta = Vector2.zero;
    }

    public void OpenShop(NPC npc)
    {
        if (npc == null || npc.shopData == null) return;

        activeNPC = npc;
        currentShop = npc.shopData;
        IsOpen = true;
        selectedEntry = null;
        selectedEntryGo = null;

        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            if (coinText != null)
                coinText.text = PlayerCurrency.Instance.FormatCurrency();
            PopulateShop(currentShop);
        }
    }

    private void CloseShop()
    {
        IsOpen = false;
        activeNPC = null;
        currentShop = null;
        selectedEntry = null;
        selectedEntryGo = null;
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    private void PopulateShop(ShopData shop)
    {
        if (gridContent == null) return;

        foreach (Transform child in gridContent)
            Destroy(child.gameObject);

        if (titleText != null)
            titleText.text = shop.shopName;

        selectedEntry = null;
        selectedEntryGo = null;

        if (shop.itemsForSale == null) return;

        int itemCount = 0;
        foreach (var entry in shop.itemsForSale)
        {
            //Debug.Log(itemCount.ToString());
            if (entry.item == null) continue;
            CreateShopEntry(entry);
            itemCount++;
        }

        // Manually set content height for ScrollRect
        int columns = 4;
        int rows = Mathf.CeilToInt(itemCount / (float)columns);
        float contentHeight = rows * GridCellSize + Mathf.Max(0, rows - 1) * GridSpacing + 12f; // 12 = padding top+bottom
        gridContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, contentHeight);

        UpdateSelectionDisplay();
        UpdateBuyButton();
    }

    private void CreateShopEntry(ShopItemEntry entry)
    {
        var go = new GameObject("ShopEntry", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(gridContent, false);
        Image image = go.GetComponent<Image>();
        image.sprite = entry.item.icon;
        image.color = Color.white;
        image.maskable = false;
        //go.GetComponent<Image>().color = EntryNormal;
        //go.GetComponent<Image>().sprite = entry.item.icon;

        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(() => SelectEntry(entry, go));

        // Icon
        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(go.transform, false);
        var iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.5f, 0.5f);
        iconRt.anchorMax = new Vector2(0.5f, 0.5f);
        iconRt.pivot = new Vector2(0.5f, 0.5f);
        iconRt.anchoredPosition = Vector2.zero;
        iconRt.sizeDelta = new Vector2(GridCellSize - 10, GridCellSize - 10);

        var iconImg = iconGo.GetComponent<Image>();
        if (entry.item.icon != null)
        {
            Debug.Log("Item has sprite");
            iconImg.sprite = entry.item.icon;
            iconImg.preserveAspect = true;
            iconImg.color = Color.white;
        }
        else
        {
            iconImg.color = RarityColors.GetColor(entry.item.rarity);
        }
    }

    private void SelectEntry(ShopItemEntry entry, GameObject go)
    {
        if (selectedEntryGo != null)
            selectedEntryGo.GetComponent<Image>().color = EntryNormal;

        selectedEntry = entry;
        selectedEntryGo = go;
        go.GetComponent<Image>().color = EntrySelected;

        UpdateSelectionDisplay();
        UpdateBuyButton();
    }

    private void UpdateSelectionDisplay()
    {
        if (selectedInfoPanel == null) return;

        if (selectedEntry == null || selectedEntry.item == null)
        {
            selectedInfoPanel.SetActive(false);
            return;
        }

        selectedInfoPanel.SetActive(true);

        if (selectedInfoIcon != null)
        {
            if (selectedEntry.item.icon != null)
            {
                selectedInfoIcon.sprite = selectedEntry.item.icon;
                selectedInfoIcon.enabled = true;
            }
            else
            {
                selectedInfoIcon.enabled = false;
            }
        }

        if (selectedInfoText != null)
        {
            long price = currentShop != null ? currentShop.GetBuyPrice(selectedEntry.item) : selectedEntry.item.coinValue;
            selectedInfoText.text = $"<b>{selectedEntry.item.itemName}</b>\n<color=#ffcc00>{price}c</color>";
        }
    }

    private void UpdateBuyButton()
    {
        if (buyButton == null) return;

        bool canBuy = selectedEntry != null
            && selectedEntry.item != null
            && PlayerCurrency.Instance != null
            && PlayerCurrency.Instance.CanAfford(currentShop.GetBuyPrice(selectedEntry.item));

        buyButton.interactable = canBuy;
        buyButton.image.color = canBuy
            ? new Color(0.3f, 0.55f, 0.3f)
            : new Color(0.35f, 0.35f, 0.35f);

        if (buyButtonLabel != null)
        {
            if (selectedEntry == null)
            {
                buyButtonLabel.text = "Select an item";
            }
            else if (canBuy)
            {
                buyButtonLabel.text = $"Buy {selectedEntry.item.itemName}";
            }
            else
            {
                buyButtonLabel.text = $"Need {currentShop.GetBuyPrice(selectedEntry.item)}c";
            }
        }
    }

    private void OnBuyClicked()
    {
        if (selectedEntry == null || selectedEntry.item == null || currentShop == null) return;

        long price = currentShop.GetBuyPrice(selectedEntry.item);

        if (!PlayerCurrency.Instance.CanAfford(price))
        {
            Debug.Log($"Not enough coins. Need {price}c.");
            return;
        }

        int added = PlayerInventory.Instance.AddItem(selectedEntry.item, 1);
        if (added <= 0)
        {
            Debug.Log("Inventory full!");
            return;
        }

        PlayerCurrency.Instance.Spend(price);
        Debug.Log($"Bought {selectedEntry.item.itemName} for {price}c.");

        if (coinText != null)
            coinText.text = PlayerCurrency.Instance.FormatCurrency();
        InventoryUI.Instance?.ForceRefresh();

        UpdateBuyButton();
    }
}
