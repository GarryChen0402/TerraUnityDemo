using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DebugItemPanel : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;
    [SerializeField] private int defaultAmount = 10;

    private GameObject panel;
    private Transform contentContainer;
    private int selectedAmount;
    private TMP_Text amountLabel;
    private bool isOpen;

    private static readonly int[] AmountPresets = { 1, 5, 10, 50, 99 };
    private static readonly Color BtnNormal = new Color(0.25f, 0.25f, 0.25f);
    private static readonly Color BtnActive = new Color(0.3f, 0.55f, 0.3f);

    private void Start()
    {
        selectedAmount = defaultAmount;
        CreatePanel();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isOpen = !isOpen;
            if (panel != null)
            {
                panel.SetActive(isOpen);
                if (isOpen) PopulateItems();
            }
        }
    }

    private void CreatePanel()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        // Panel root
        var panelGo = new GameObject("DebugItemPanel", typeof(RectTransform), typeof(Image));
        panelGo.transform.SetParent(canvas.transform, false);
        panelGo.SetActive(false);
        panel = panelGo;

        var panelRt = panelGo.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0, 0.5f);
        panelRt.anchorMax = new Vector2(0, 0.5f);
        panelRt.pivot = new Vector2(0, 0.5f);
        panelRt.anchoredPosition = new Vector2(8, 0);
        panelRt.sizeDelta = new Vector2(300, 520);
        panelGo.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.08f, 0.94f);

        // Title
        var titleGo = MakeText("Title", panelGo.transform, "Debug Items [F1]", 18);
        var titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0.5f, 1);
        titleRt.anchoredPosition = new Vector2(0, -4);
        titleRt.sizeDelta = new Vector2(0, 32);
        titleGo.GetComponent<TextMeshProUGUI>().color = new Color(0.6f, 0.9f, 0.6f);
        titleGo.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        titleGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Amount row
        var amtRow = new GameObject("AmountRow", typeof(RectTransform));
        amtRow.transform.SetParent(panelGo.transform, false);
        var amtRowRt = amtRow.GetComponent<RectTransform>();
        amtRowRt.anchorMin = new Vector2(0, 1);
        amtRowRt.anchorMax = new Vector2(1, 1);
        amtRowRt.pivot = new Vector2(0.5f, 1);
        amtRowRt.anchoredPosition = new Vector2(0, -36);
        amtRowRt.sizeDelta = new Vector2(0, 40);

        // "Qty:" label
        var qtyLabel = MakeText("QtyLabel", amtRow.transform, "Qty:", 16);
        var qtyRt = qtyLabel.GetComponent<RectTransform>();
        qtyRt.anchorMin = new Vector2(0, 0.5f);
        qtyRt.anchorMax = new Vector2(0, 0.5f);
        qtyRt.pivot = new Vector2(0, 0.5f);
        qtyRt.anchoredPosition = new Vector2(8, 0);
        qtyRt.sizeDelta = new Vector2(40, 30);
        qtyLabel.GetComponent<TextMeshProUGUI>().color = Color.white;

        // Amount buttons
        float btnX = 52;
        GameObject lastBtn = null;
        foreach (int amt in AmountPresets)
        {
            var btnGo = new GameObject($"Btn{amt}", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(amtRow.transform, false);
            var btnRt = btnGo.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0, 0.5f);
            btnRt.anchorMax = new Vector2(0, 0.5f);
            btnRt.pivot = new Vector2(0, 0.5f);
            btnRt.anchoredPosition = new Vector2(btnX, 0);
            btnRt.sizeDelta = new Vector2(40, 32);
            var btnImg = btnGo.GetComponent<Image>();
            btnImg.color = amt == selectedAmount ? BtnActive : BtnNormal;

            int capturedAmt = amt;
            btnGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                selectedAmount = capturedAmt;
                UpdateAmountButtons(amtRow.transform);
                if (amountLabel != null) amountLabel.text = $"x{selectedAmount}";
            });

            var btnLabel = MakeText("Label", btnGo.transform, amt.ToString(), 16);
            var btnLabelRt = btnLabel.GetComponent<RectTransform>();
            btnLabelRt.anchorMin = Vector2.zero;
            btnLabelRt.anchorMax = Vector2.one;
            btnLabelRt.sizeDelta = Vector2.zero;
            btnLabel.GetComponent<TextMeshProUGUI>().color = Color.white;
            btnLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            btnX += 44;
            lastBtn = btnGo;
        }

        // Selected amount display
        var amtDisplay = MakeText("AmountDisplay", amtRow.transform, $"x{selectedAmount}", 18);
        var amtDispRt = amtDisplay.GetComponent<RectTransform>();
        amtDispRt.anchorMin = new Vector2(1, 0.5f);
        amtDispRt.anchorMax = new Vector2(1, 0.5f);
        amtDispRt.pivot = new Vector2(1, 0.5f);
        amtDispRt.anchoredPosition = new Vector2(-4, 0);
        amtDispRt.sizeDelta = new Vector2(60, 30);
        amtDisplay.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 1f, 0.5f);
        amtDisplay.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        amountLabel = amtDisplay.GetComponent<TextMeshProUGUI>();

        // ScrollView for items
        var svGo = new GameObject("ScrollView", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        svGo.transform.SetParent(panelGo.transform, false);
        var svRt = svGo.GetComponent<RectTransform>();
        svRt.anchorMin = new Vector2(0, 0);
        svRt.anchorMax = new Vector2(1, 1);
        svRt.offsetMin = new Vector2(4, 4);
        svRt.offsetMax = new Vector2(-4, -80);

        var vpGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        vpGo.transform.SetParent(svGo.transform, false);
        var vpRt = vpGo.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.sizeDelta = Vector2.zero;
        vpGo.GetComponent<Image>().color = new Color(0, 0, 0, 0.15f);
        vpGo.GetComponent<Mask>().showMaskGraphic = false;

        var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentGo.transform.SetParent(vpGo.transform, false);
        var contentRt = contentGo.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.sizeDelta = Vector2.zero;
        var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 1;
        vlg.padding = new RectOffset(2, 2, 2, 2);
        contentGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var sr = svGo.GetComponent<ScrollRect>();
        sr.viewport = vpRt;
        sr.content = contentRt;
        sr.horizontal = false;
        sr.vertical = true;

        contentContainer = contentRt;
    }

    private void UpdateAmountButtons(Transform amtRow)
    {
        for (int i = 0; i < AmountPresets.Length; i++)
        {
            var child = amtRow.Find($"Btn{AmountPresets[i]}");
            if (child != null)
                child.GetComponent<Image>().color = AmountPresets[i] == selectedAmount ? BtnActive : BtnNormal;
        }
    }

    private static GameObject MakeText(string name, Transform parent, string text, int fontSize)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        return go;
    }

    private void PopulateItems()
    {
        foreach (Transform child in contentContainer)
            Destroy(child.gameObject);

        var items = CollectAllItems();
        foreach (var item in items)
        {
            var entry = new GameObject(item.itemName, typeof(RectTransform), typeof(Image), typeof(Button));
            entry.transform.SetParent(contentContainer, false);
            var rt = entry.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 36);

            entry.GetComponent<Image>().color = new Color(0.18f, 0.18f, 0.18f);

            // Horizontal layout for icon + label
            var hlg = entry.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 8;
            hlg.padding = new RectOffset(6, 4, 3, 3);

            // Icon
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(entry.transform, false);
            var iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.sizeDelta = new Vector2(28, 28);
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.sprite = item.icon;
            iconImg.preserveAspect = true;
            // Gray out if no icon
            if (item.icon == null)
                iconImg.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);

            // Label
            var labelGo = MakeText("Label", entry.transform,
                $"{item.itemName}  [{item.itemType}]", 14);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.sizeDelta = new Vector2(210, 28);
            labelGo.GetComponent<TextMeshProUGUI>().color = RarityColors.GetColor(item.rarity);
            labelGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

            // Click
            var btn = entry.GetComponent<Button>();
            var capturedItem = item;
            btn.onClick.AddListener(() =>
            {
                int amt = selectedAmount > 0 ? selectedAmount : 1;
                PlayerInventory.Instance?.AddItem(capturedItem, amt);
                InventoryUI.Instance?.ForceRefresh();
            });
        }
    }

    private List<ItemData> CollectAllItems()
    {
        var items = new List<ItemData>();
        var seen = new HashSet<int>();

        // Collect from CraftingUI recipes
        var craftingUI = FindObjectOfType<CraftingUI>();
        if (craftingUI != null && craftingUI.AllRecipes != null)
        {
            foreach (var recipe in craftingUI.AllRecipes)
            {
                AddItem(recipe.outputItem, items, seen);
                if (recipe.ingredients != null)
                    foreach (var ing in recipe.ingredients)
                        AddItem(ing.item, items, seen);
            }
        }

        // Collect from TileDataManager
        var tdm = TileDataManager.Instance;
        if (tdm != null && tdm.AllTileData != null)
        {
            foreach (var td in tdm.AllTileData)
                AddItem(td.dropItem, items, seen);
        }

        // Collect from PlayerInventory
        var inv = PlayerInventory.Instance;
        if (inv != null)
        {
            foreach (var slot in inv.slots)
            {
                if (slot.itemData != null)
                    AddItem(slot.itemData, items, seen);
            }
        }

        return items;
    }

    private static void AddItem(ItemData item, List<ItemData> list, HashSet<int> seen)
    {
        if (item == null || seen.Contains(item.itemID)) return;
        seen.Add(item.itemID);
        list.Add(item);
    }
}
