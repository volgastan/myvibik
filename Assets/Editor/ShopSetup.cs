using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class ShopSetup : EditorWindow
{
    [MenuItem("Tools/Setup Shop")]
    static void CreateAllShopItems()
    {
        CreateFolders();
        CreateShopItems();
        CreateShopItemPrefab();
        SetupShopManager();

        Debug.Log("Магазин полностью настроен!");
    }

    static void CreateFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder("Assets/Data/Shop"))
            AssetDatabase.CreateFolder("Assets/Data", "Shop");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Sprites/Shop"))
            AssetDatabase.CreateFolder("Assets/Sprites", "Shop");
        AssetDatabase.Refresh();
    }

    static void CreateShopItems()
    {
        CreateShopItem("BG_Forest", ShopItemData.ItemType.Background, "shop_bg_forest", 5, 0);
        CreateShopItem("BG_Beach", ShopItemData.ItemType.Background, "shop_bg_beach", 10, 1);
        CreateShopItem("BG_Night", ShopItemData.ItemType.Background, "shop_bg_night", 15, 2);

        CreateShopItem("Costume_Hat", ShopItemData.ItemType.Costume, "shop_costume_hat", 3, 0);
        CreateShopItem("Costume_Scarf", ShopItemData.ItemType.Costume, "shop_costume_scarf", 5, 1);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static void CreateShopItem(string name, ShopItemData.ItemType type, string nameKey, int price, int unlockIndex)
    {
        ShopItemData item = ScriptableObject.CreateInstance<ShopItemData>();
        item.type = type;
        item.id = name.ToLower();
        item.nameKey = nameKey;
        item.price = price;
        item.unlockIndex = unlockIndex;

        string iconPath = $"Assets/Sprites/Shop/{name}_icon.png";
        if (!System.IO.File.Exists(iconPath))
        {
            Texture2D tex = new Texture2D(128, 128);
            Color[] colors = new Color[128 * 128];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Random.ColorHSV(0.5f, 0.8f, 0.7f, 0.9f, 0.8f, 1f);
            tex.SetPixels(colors);
            tex.Apply();
            byte[] png = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(iconPath, png);
            AssetDatabase.Refresh();
        }
        Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
        if (icon != null) item.icon = icon;

        item.previewSprite = icon;

        string path = $"Assets/Data/Shop/{name}.asset";
        AssetDatabase.CreateAsset(item, path);
    }

    static void CreateShopItemPrefab()
    {
        GameObject prefabObj = new GameObject("ShopItemPrefab");
        RectTransform rt = prefabObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 250);

        Image bg = prefabObj.AddComponent<Image>();
        bg.color = new Color(0.95f, 0.95f, 0.95f);

        // Иконка
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(prefabObj.transform);
        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.color = Color.white;
        RectTransform iconRt = iconObj.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.5f, 0.7f);
        iconRt.anchorMax = new Vector2(0.5f, 0.7f);
        iconRt.anchoredPosition = Vector2.zero;
        iconRt.sizeDelta = new Vector2(100, 100);

        // Название
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(prefabObj.transform);
        TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
        nameTmp.text = "Название";
        nameTmp.fontSize = 18;
        nameTmp.alignment = TextAlignmentOptions.Center;
        nameTmp.color = Color.black;
        RectTransform nameRt = nameObj.GetComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0, 0.4f);
        nameRt.anchorMax = new Vector2(1, 0.6f);
        nameRt.offsetMin = Vector2.zero;
        nameRt.offsetMax = Vector2.zero;

        // Цена
        GameObject priceObj = new GameObject("Price");
        priceObj.transform.SetParent(prefabObj.transform);
        TextMeshProUGUI priceTmp = priceObj.AddComponent<TextMeshProUGUI>();
        priceTmp.text = "5 🪙";
        priceTmp.fontSize = 16;
        priceTmp.alignment = TextAlignmentOptions.Center;
        priceTmp.color = Color.black;
        RectTransform priceRt = priceObj.GetComponent<RectTransform>();
        priceRt.anchorMin = new Vector2(0, 0.15f);
        priceRt.anchorMax = new Vector2(1, 0.3f);
        priceRt.offsetMin = Vector2.zero;
        priceRt.offsetMax = Vector2.zero;

        // Кнопка
        GameObject btnObj = new GameObject("ActionButton");
        btnObj.transform.SetParent(prefabObj.transform);
        Button btn = btnObj.AddComponent<Button>();
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.6f, 1f);
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.1f, 0.02f);
        btnRt.anchorMax = new Vector2(0.9f, 0.15f);
        btnRt.offsetMin = Vector2.zero;
        btnRt.offsetMax = Vector2.zero;

        GameObject btnTextObj = new GameObject("Text (TMP)");
        btnTextObj.transform.SetParent(btnObj.transform);
        TextMeshProUGUI btnTmp = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnTmp.text = "Купить";
        btnTmp.fontSize = 16;
        btnTmp.alignment = TextAlignmentOptions.Center;
        btnTmp.color = Color.white;
        RectTransform btnTextRt = btnTextObj.GetComponent<RectTransform>();
        btnTextRt.anchorMin = Vector2.zero;
        btnTextRt.anchorMax = Vector2.one;
        btnTextRt.offsetMin = Vector2.zero;
        btnTextRt.offsetMax = Vector2.zero;

        ShopItemView view = prefabObj.AddComponent<ShopItemView>();
        SerializedObject so = new SerializedObject(view);
        so.FindProperty("iconImage").objectReferenceValue = iconImg;
        so.FindProperty("nameText").objectReferenceValue = nameTmp;
        so.FindProperty("priceText").objectReferenceValue = priceTmp;
        so.FindProperty("actionButton").objectReferenceValue = btn;
        so.FindProperty("actionButtonText").objectReferenceValue = btnTmp;
        so.ApplyModifiedProperties();

        string path = "Assets/Prefabs/ShopItemPrefab.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(prefabObj, path);
        DestroyImmediate(prefabObj);
        Debug.Log($"Префаб ShopItemPrefab создан по пути {path}");
    }

    static void SetupShopManager()
    {
        GameObject shopManagerObj = GameObject.Find("ShopManager");
        if (shopManagerObj == null)
        {
            shopManagerObj = new GameObject("ShopManager");
            shopManagerObj.AddComponent<ShopManager>();
        }

        ShopManager shopManager = shopManagerObj.GetComponent<ShopManager>();
        GameObject shopPanel = GameObject.Find("ShopPanel");
        if (shopPanel == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            shopPanel = new GameObject("ShopPanel");
            shopPanel.transform.SetParent(canvas.transform);
            RectTransform rt = shopPanel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image img = shopPanel.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.5f);
            shopPanel.SetActive(false);
        }

        Transform container = shopPanel.transform.Find("ItemsContainer");
        if (container == null)
        {
            GameObject containerObj = new GameObject("ItemsContainer");
            containerObj.transform.SetParent(shopPanel.transform);
            RectTransform rt = containerObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(20, 20);
            rt.offsetMax = new Vector2(-20, -80);
            GridLayoutGroup grid = containerObj.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(200, 250);
            grid.spacing = new Vector2(15, 15);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            container = containerObj.transform;
        }

        SerializedObject so = new SerializedObject(shopManager);
        so.FindProperty("itemsContainer").objectReferenceValue = container.gameObject;
        so.FindProperty("itemPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ShopItemPrefab.prefab");

        string[] guids = AssetDatabase.FindAssets("t:ShopItemData");
        var items = new System.Collections.Generic.List<ShopItemData>();
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ShopItemData item = AssetDatabase.LoadAssetAtPath<ShopItemData>(path);
            if (item != null) items.Add(item);
        }
        so.FindProperty("shopItems").arraySize = items.Count;
        for (int i = 0; i < items.Count; i++)
            so.FindProperty($"shopItems.Array.data[{i}]").objectReferenceValue = items[i];
        so.ApplyModifiedProperties();

        Transform backBtn = shopPanel.transform.Find("BackButton");
        if (backBtn == null)
        {
            GameObject backObj = new GameObject("BackButton");
            backObj.transform.SetParent(shopPanel.transform);
            Button btn = backObj.AddComponent<Button>();
            Image img = backObj.AddComponent<Image>();
            img.color = Color.white;
            RectTransform rt = backObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(50, -50);
            rt.sizeDelta = new Vector2(100, 50);
            GameObject textObj = new GameObject("Text (TMP)");
            textObj.transform.SetParent(backObj.transform);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "Назад";
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.black;
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            UIManager uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager != null)
                btn.onClick.AddListener(uiManager.CloseShop);
        }

        Debug.Log("ShopManager полностью настроен.");
    }
}