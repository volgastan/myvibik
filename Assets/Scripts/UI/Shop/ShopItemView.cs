using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Vibies.Localization;   // ← Добавляем пространство имён для локализации

public class ShopItemView : MonoBehaviour
{
    [Header("UI-элементы")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonText;

    private ShopItemData itemData;
    private GameManager gameManager;
    private ShopManager shopManager;

    public void Setup(ShopItemData data, GameManager gm, ShopManager sm)
    {
        itemData = data;
        gameManager = gm;
        shopManager = sm;

        if (iconImage != null && data.icon != null)
            iconImage.sprite = data.icon;

        Refresh();
    }

    public void Refresh()
    {
        if (itemData == null || gameManager == null) return;

        bool isUnlocked = IsItemUnlocked();
        bool isSelected = IsItemSelected();

        // Обновляем цену
        if (priceText != null)
        {
            priceText.text = isUnlocked ? "✓" : $"{itemData.price} 🪙";
        }

        // Обновляем имя (локализованное)
        if (nameText != null)
        {
            string localizedName = CsvLocalizationManager.Instance != null 
                ? CsvLocalizationManager.Instance.GetText(itemData.nameKey) 
                : itemData.nameKey;
            nameText.text = localizedName;
        }

        // Обновляем кнопку
        if (actionButton != null)
        {
            actionButton.interactable = !isUnlocked || !isSelected;

            if (actionButtonText != null)
            {
                if (isUnlocked)
                {
                    actionButtonText.text = isSelected ? "Выбрано" : "Выбрать";
                }
                else
                {
                    actionButtonText.text = "Купить";
                }
            }

            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnButtonClick);
        }
    }

    private bool IsItemUnlocked()
    {
        if (itemData.type == ShopItemData.ItemType.Background)
            return gameManager.IsBackgroundUnlocked(itemData.unlockIndex);
        else // Costume
            return gameManager.IsCharacterUnlocked(itemData.unlockIndex);
    }

    private bool IsItemSelected()
    {
        if (itemData.type == ShopItemData.ItemType.Background)
            return gameManager.SelectedBackgroundIndex == itemData.unlockIndex;
        else // Costume
            return gameManager.SelectedCostumeIndex == itemData.unlockIndex;
    }

    private void OnButtonClick()
    {
        if (IsItemUnlocked())
        {
            // Выбираем предмет (без покупки)
            if (itemData.type == ShopItemData.ItemType.Background)
            {
                if (gameManager.SelectedBackgroundIndex != itemData.unlockIndex)
                    gameManager.BuyBackground(itemData.unlockIndex, 0);
            }
            else
            {
                if (gameManager.SelectedCostumeIndex != itemData.unlockIndex)
                    gameManager.BuyCostume(itemData.unlockIndex, 0);
            }
            Refresh();
            shopManager.RefreshAllItems();
        }
        else
        {
            // Покупаем
            shopManager.OnItemPurchased(itemData);
        }
    }
}