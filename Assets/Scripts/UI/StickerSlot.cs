using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ячейка для отображения части паззла (собрана/не собрана).
/// </summary>
public class StickerSlot : MonoBehaviour
{
    [Header("Спрайты")]
    [SerializeField] private Sprite collectedSprite; // цветная часть
    [SerializeField] private Sprite emptySprite;    // серая заглушка

    private Image image;
    private int index;

    private void Awake()
    {
        image = GetComponent<Image>();
        if (image == null)
        {
            Debug.LogError("[StickerSlot] Компонент Image отсутствует!");
            return;
        }

        // Если спрайты не назначены в инспекторе — загружаем из Resources
        if (collectedSprite == null)
            collectedSprite = Resources.Load<Sprite>("Puzzle/part_0");
        if (emptySprite == null)
            emptySprite = Resources.Load<Sprite>("Puzzle/part_empty");
    }

    public void SetIndex(int idx)
    {
        index = idx;
        // Загружаем цветной спрайт по индексу
        Sprite colored = Resources.Load<Sprite>($"Puzzle/part_{index}");
        if (colored != null)
            collectedSprite = colored;
        else
            Debug.LogWarning($"[StickerSlot] Не найден спрайт для части {index}");
    }

    public void SetCollected(bool collected)
    {
        if (image == null) return;
        image.sprite = collected ? collectedSprite : emptySprite;
    }
}