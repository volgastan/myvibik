using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using Vibies.Localization;

public class ShopManager : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private GameObject itemPrefab;

    [Header("Список товаров")]
    [SerializeField] private ShopItemData[] shopItems;

    private List<ShopItemView> itemViews = new List<ShopItemView>();
    private bool isInitialized = false;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (gameManager != null)
            gameManager.OnCoinsChanged += RefreshAllItems;
    }

    private void OnDestroy()
    {
        if (gameManager != null)
            gameManager.OnCoinsChanged -= RefreshAllItems;
    }

    private void Start()
    {
        InitializeShop();
    }

    private void InitializeShop()
    {
        if (isInitialized) return;

        foreach (Transform child in itemsContainer)
            Destroy(child.gameObject);
        itemViews.Clear();

        foreach (var itemData in shopItems)
        {
            GameObject itemObj = Instantiate(itemPrefab, itemsContainer);
            ShopItemView view = itemObj.GetComponent<ShopItemView>();
            if (view == null)
                view = itemObj.AddComponent<ShopItemView>();

            view.Setup(itemData, gameManager, this);
            itemViews.Add(view);
        }

        isInitialized = true;
        RefreshAllItems();
    }

    public void RefreshAllItems()
    {
        foreach (var view in itemViews)
            view.Refresh();
    }

    public void OnItemPurchased(ShopItemData itemData)
    {
        if (gameManager == null) return;

        // Проверяем, доступна ли покупка на этой платформе
        bool isPurchaseAvailable = PlatformService.Instance.IsAuthorized; // упрощённо

        #if UNITY_WEBGL && !UNITY_EDITOR
            // В VK-версии показываем тизер
            ShowTiser(itemData);
            return;
        #endif

        // В мобильной версии (или редакторе) — выполняем покупку
        bool success = false;
        if (itemData.type == ShopItemData.ItemType.Background)
        {
            success = gameManager.BuyBackground(itemData.unlockIndex, itemData.price);
            if (success) ApplyBackground(itemData);
        }
        else if (itemData.type == ShopItemData.ItemType.Costume)
        {
            success = gameManager.BuyCostume(itemData.unlockIndex, itemData.price);
            if (success) ApplyCostume(itemData);
        }

        if (success)
            RefreshAllItems();
    }

    private void ShowTiser(ShopItemData itemData)
    {
        // Показываем сообщение о том, что покупка доступна только в приложении
        Debug.Log($"[ShopManager] Тизер: {itemData.nameKey} — Доступно в приложении");
        // Можно показать всплывающее окно с текстом
    }

    private void ApplyBackground(ShopItemData itemData)
    {
        GameObject mainPanel = GameObject.Find("MainPanel");
        if (mainPanel != null)
        {
            Image img = mainPanel.GetComponent<Image>();
            if (img != null && itemData.previewSprite != null)
            {
                img.sprite = itemData.previewSprite;
                img.color = Color.white;
            }
        }
    }

    private void ApplyCostume(ShopItemData itemData)
    {
        GameObject charImage = GameObject.Find("CharacterImage");
        if (charImage != null)
        {
            Image img = charImage.GetComponent<Image>();
            if (img != null && itemData.previewSprite != null)
            {
                img.sprite = itemData.previewSprite;
            }
        }
    }
}