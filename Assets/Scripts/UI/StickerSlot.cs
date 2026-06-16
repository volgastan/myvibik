using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class StickerSlot : MonoBehaviour
{
    [Header("Спрайты")]
    [SerializeField] private Sprite collectedSprite;
    [SerializeField] private Sprite emptySprite;

    private Image image;
    private int index;

    private void Awake()
    {
        image = GetComponent<Image>();
        if (collectedSprite == null)
            collectedSprite = Resources.Load<Sprite>("Stickers/Collected");
        if (emptySprite == null)
            emptySprite = Resources.Load<Sprite>("Stickers/Empty");
    }

    public void SetIndex(int idx) { index = idx; }
    public void SetCollected(bool collected)
    {
        if (image != null)
            image.sprite = collected ? collectedSprite : emptySprite;
    }
}