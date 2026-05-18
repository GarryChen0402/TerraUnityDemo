using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingUI : MonoBehaviour
{
    public static CraftingUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject craftingPanel;
    [SerializeField] private Transform recipeContainer;
    [SerializeField] private KeyCode toggleKey = KeyCode.C;

    [Header("Recipes")]
    [SerializeField] private CraftingRecipe[] allRecipes;

    private bool isOpen;
    private CraftingStation activeStation;
    private ScrollRect scrollRect;
    private CraftingRecipe selectedRecipe;
    private GameObject selectedEntry;
    private Button craftButton;
    private TMP_Text craftButtonLabel;

    private static readonly Color SelectedColor = new Color(0.25f, 0.45f, 0.25f);
    private static readonly Color NormalColor = new Color(0.15f, 0.15f, 0.15f);
    private static readonly Color UnavailableColor = new Color(0.12f, 0.12f, 0.12f);

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (craftingPanel == null)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                var t = canvas.transform.Find("CraftingPanel");
                if (t != null) craftingPanel = t.gameObject;
            }
        }

        if (craftingPanel != null)
        {
            craftingPanel.SetActive(false);

            var panelRt = craftingPanel.GetComponent<RectTransform>();
            if (panelRt != null && panelRt.sizeDelta.magnitude < 10f)
                panelRt.sizeDelta = new Vector2(320, 420);

            if (recipeContainer == null)
            {
                var existingScroll = craftingPanel.transform.Find("ScrollView");
                if (existingScroll != null)
                {
                    var content = existingScroll.Find("Viewport/Content");
                    if (content != null) recipeContainer = content;
                }
            }

            if (recipeContainer == null)
                CreateScrollView();

            // Create the global Craft button at the bottom
            if (craftButton == null)
                CreateCraftButton();
        }

        if (allRecipes == null || allRecipes.Length == 0)
            allRecipes = Resources.LoadAll<CraftingRecipe>("");
    }

    private void CreateScrollView()
    {
        var svGo = new GameObject("ScrollView", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        svGo.transform.SetParent(craftingPanel.transform, false);
        var svRt = svGo.GetComponent<RectTransform>();
        svRt.anchorMin = new Vector2(0, 0);
        svRt.anchorMax = new Vector2(1, 1);
        svRt.offsetMin = new Vector2(8, 44);
        svRt.offsetMax = new Vector2(-8, -8);

        var vpGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        vpGo.transform.SetParent(svGo.transform, false);
        var vpRt = vpGo.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.sizeDelta = Vector2.zero;
        vpGo.GetComponent<Image>().color = new Color(0, 0, 0, 0.05f);
        vpGo.GetComponent<Mask>().showMaskGraphic = false;

        var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentGo.transform.SetParent(vpGo.transform, false);
        var contentRt = contentGo.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.sizeDelta = Vector2.zero;
        var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 2;
        vlg.padding = new RectOffset(2, 2, 2, 2);
        contentGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect = svGo.GetComponent<ScrollRect>();
        scrollRect.viewport = vpRt;
        scrollRect.content = contentRt;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        recipeContainer = contentRt;
    }

    private void CreateCraftButton()
    {
        var btnGo = new GameObject("CraftButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(craftingPanel.transform, false);
        var btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0);
        btnRt.anchorMax = new Vector2(0.5f, 0);
        btnRt.pivot = new Vector2(0.5f, 0);
        btnRt.anchoredPosition = new Vector2(0, 4);
        btnRt.sizeDelta = new Vector2(200, 36);

        var btnImg = btnGo.GetComponent<Image>();
        btnImg.color = new Color(0.3f, 0.55f, 0.3f);

        craftButton = btnGo.GetComponent<Button>();
        craftButton.onClick.AddListener(OnCraftClicked);

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(btnGo.transform, false);
        craftButtonLabel = labelGo.GetComponent<TextMeshProUGUI>();
        craftButtonLabel.text = "Craft";
        craftButtonLabel.fontSize = 16;
        craftButtonLabel.color = Color.white;
        craftButtonLabel.alignment = TextAlignmentOptions.Center;
        var labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.sizeDelta = Vector2.zero;

        UpdateCraftButtonState();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isOpen = !isOpen;
            if (craftingPanel != null)
            {
                craftingPanel.SetActive(isOpen);
                if (isOpen) RefreshRecipes();
                else { selectedRecipe = null; selectedEntry = null; }
            }
        }

        if (isOpen)
            DetectNearbyStation();
    }

    private void DetectNearbyStation()
    {
        activeStation = null;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        foreach (var station in FindObjectsOfType<CraftingStation>())
        {
            if (station.IsPlayerInRange())
            {
                activeStation = station;
                break;
            }
        }
    }

    public void RefreshRecipes()
    {
        if (recipeContainer == null) return;

        foreach (Transform child in recipeContainer)
            Destroy(child.gameObject);

        selectedRecipe = null;
        selectedEntry = null;

        CraftingStationType currentStation = activeStation != null
            ? activeStation.stationType : CraftingStationType.Hand;

        foreach (var recipe in allRecipes)
        {
            if (recipe.requiredStation != currentStation) continue;

            var entry = CreateEntry(recipe);
            SetupEntry(entry, recipe);
        }

        UpdateCraftButtonState();
    }

    private GameObject CreateEntry(CraftingRecipe recipe)
    {
        var go = new GameObject("RecipeEntry", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(recipeContainer, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 48);

        var bg = go.GetComponent<Image>();
        bg.color = NormalColor;

        var layout = go.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 6;
        layout.padding = new RectOffset(6, 6, 4, 4);

        // Icon
        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(go.transform, false);
        iconGo.GetComponent<RectTransform>().sizeDelta = new Vector2(32, 32);

        // Info
        var infoGo = new GameObject("Info", typeof(RectTransform), typeof(TextMeshProUGUI));
        infoGo.transform.SetParent(go.transform, false);
        infoGo.GetComponent<RectTransform>().sizeDelta = new Vector2(240, 40);
        var tmp = infoGo.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = 12;
        tmp.overflowMode = TextOverflowModes.Truncate;

        return go;
    }

    private void SetupEntry(GameObject entry, CraftingRecipe recipe)
    {
        var iconImg = entry.transform.Find("Icon")?.GetComponent<Image>();
        var infoText = entry.transform.Find("Info")?.GetComponent<TextMeshProUGUI>();
        var btn = entry.GetComponent<Button>();

        if (iconImg != null && recipe.outputItem != null)
            iconImg.sprite = recipe.outputItem.icon;

        if (infoText != null)
        {
            string info = $"<b>{recipe.outputItem.itemName}</b> x{recipe.outputAmount}\n";

            foreach (var ing in recipe.ingredients)
            {
                int owned = PlayerInventory.Instance != null
                    ? PlayerInventory.Instance.GetItemCount(ing.item) : 0;
                string color = owned >= ing.amount ? "#8f8" : "#f66";
                info += $"  <color={color}>{ing.item.itemName} {owned}/{ing.amount}</color>";
                if (ing != recipe.ingredients[recipe.ingredients.Length - 1])
                    info += "\n";
            }

            infoText.text = info;
        }

        // Entry click selects the recipe
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => SelectRecipe(recipe, entry));
    }

    private void SelectRecipe(CraftingRecipe recipe, GameObject entry)
    {
        // Deselect previous
        if (selectedEntry != null)
            selectedEntry.GetComponent<Image>().color = NormalColor;

        selectedRecipe = recipe;
        selectedEntry = entry;
        entry.GetComponent<Image>().color = SelectedColor;

        UpdateCraftButtonState();
    }

    private void UpdateCraftButtonState()
    {
        if (craftButton == null) return;

        bool canCraft = selectedRecipe != null
            && PlayerInventory.Instance != null
            && selectedRecipe.CanCraft(PlayerInventory.Instance);

        craftButton.interactable = canCraft;
        craftButton.image.color = canCraft
            ? new Color(0.35f, 0.6f, 0.35f)
            : new Color(0.4f, 0.4f, 0.4f);

        if (craftButtonLabel != null)
        {
            craftButtonLabel.color = canCraft ? Color.white : new Color(0.6f, 0.6f, 0.6f);
            craftButtonLabel.text = selectedRecipe != null
                ? $"Craft {selectedRecipe.outputItem.itemName}"
                : "Select a recipe";
        }
    }

    private void OnCraftClicked()
    {
        if (selectedRecipe == null || PlayerInventory.Instance == null) return;

        if (selectedRecipe.Craft(PlayerInventory.Instance))
        {
            if (selectedRecipe.outputItem.itemType == ItemType.Tool ||
                selectedRecipe.outputItem.itemType == ItemType.Weapon ||
                selectedRecipe.outputItem.itemType == ItemType.Armor)
            {
                PlayerEquipment.Instance?.Equip(selectedRecipe.outputItem);
            }

            if (InventoryUI.Instance != null)
                InventoryUI.Instance.ForceRefresh();
            RefreshRecipes();
        }
    }

    public void ForceRefresh()
    {
        if (isOpen) RefreshRecipes();
    }

    public void ShowRecipesFor(CraftingStationType stationType)
    {
        isOpen = true;
        if (craftingPanel != null)
            craftingPanel.SetActive(true);
        activeStation = null;
        RefreshRecipes();
    }
}
