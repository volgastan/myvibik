using UnityEngine;

[CreateAssetMenu(fileName = "ShopItem", menuName = "Vibiki/Shop Item")]
public class ShopItemData : ScriptableObject
{
    public enum ItemType { Background, Costume }

    [Header("Основное")]
    public string id;                  // уникальный ID (например, "bg_forest")
    public ItemType type;
    public string nameKey;             // ключ локализации (например, "shop_bg_forest")
    public int price;

    [Header("Визуал")]
    public Sprite icon;                // иконка для магазина
    public Sprite previewSprite;       // для фона – спрайт самого фона, для костюма – спрайт персонажа в костюме

    [Header("Связи (опционально)")]
    public int unlockIndex;            // индекс для GameManager (SelectedBackgroundIndex или SelectedCostumeIndex)
}