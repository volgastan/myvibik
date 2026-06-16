using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Ресурсы")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CharacterController characterController;

    [Header("Панели")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject collectionPanel;

    [Header("Отображение ресурсов")]
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text daysText;

    [Header("Коллекция наклеек")]
    [SerializeField] private Transform stickerGridParent;
    [SerializeField] private GameObject stickerSlotPrefab;

    [Header("Кнопки действий")]
    [SerializeField] private Button actionPetButton;
    [SerializeField] private Button actionFeedButton;
    [SerializeField] private Button actionPlayButton;

    [Header("Кнопки навигации")]
    [SerializeField] private Button shopButton;
    [SerializeField] private Button collectionButton;

    private List<StickerSlot> stickerSlots = new List<StickerSlot>();

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
        if (characterController == null)
            characterController = FindFirstObjectByType<CharacterController>();

        if (gameManager != null)
        {
            gameManager.OnCoinsChanged += UpdateCoinsUI;
            gameManager.OnDaysChanged += UpdateDaysUI;
            gameManager.OnStickersChanged += UpdateStickersUI;
        }
        else
        {
            Debug.LogError("[UIManager] GameManager не найден!");
        }

        CreateStickerSlots();

        if (actionPetButton != null)
            actionPetButton.onClick.AddListener(() => {
                gameManager?.PerformAction("pet");
                characterController?.PlayHappyAnimation();
            });
        if (actionFeedButton != null)
            actionFeedButton.onClick.AddListener(() => {
                gameManager?.PerformAction("feed");
                characterController?.PlayEatAnimation();
            });
        if (actionPlayButton != null)
            actionPlayButton.onClick.AddListener(() => {
                gameManager?.PerformAction("play");
                characterController?.PlayPlayAnimation();
            });

        if (shopButton != null)
            shopButton.onClick.AddListener(OpenShop);
        if (collectionButton != null)
            collectionButton.onClick.AddListener(OpenCollection);

        UpdateCoinsUI();
        UpdateDaysUI();
        UpdateStickersUI();

        CloseShop();
        CloseCollection();
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnCoinsChanged -= UpdateCoinsUI;
            gameManager.OnDaysChanged -= UpdateDaysUI;
            gameManager.OnStickersChanged -= UpdateStickersUI;
        }

        if (actionPetButton != null)
            actionPetButton.onClick.RemoveAllListeners();
        if (actionFeedButton != null)
            actionFeedButton.onClick.RemoveAllListeners();
        if (actionPlayButton != null)
            actionPlayButton.onClick.RemoveAllListeners();
        if (shopButton != null)
            shopButton.onClick.RemoveAllListeners();
        if (collectionButton != null)
            collectionButton.onClick.RemoveAllListeners();
    }

    private void UpdateCoinsUI()
    {
        if (coinsText != null && gameManager != null)
            coinsText.text = gameManager.Coins.ToString();
    }

    private void UpdateDaysUI()
    {
        if (daysText != null && gameManager != null)
            daysText.text = gameManager.DaysCount.ToString();
    }

    private void UpdateStickersUI()
    {
        if (gameManager == null) return;
        bool[] stickers = gameManager.StickersCollected;
        for (int i = 0; i < stickerSlots.Count && i < stickers.Length; i++)
        {
            stickerSlots[i].SetCollected(stickers[i]);
        }
    }

    private void CreateStickerSlots()
    {
        if (stickerGridParent == null || stickerSlotPrefab == null)
        {
            Debug.LogWarning("[UIManager] stickerGridParent или stickerSlotPrefab не назначены");
            return;
        }

        foreach (Transform child in stickerGridParent)
            Destroy(child.gameObject);
        stickerSlots.Clear();

        int totalStickers = 8;
        for (int i = 0; i < totalStickers; i++)
        {
            GameObject slot = Instantiate(stickerSlotPrefab, stickerGridParent);
            StickerSlot slotScript = slot.GetComponent<StickerSlot>();
            if (slotScript == null)
                slotScript = slot.AddComponent<StickerSlot>();
            slotScript.SetIndex(i);
            stickerSlots.Add(slotScript);
        }
    }

    public void OpenShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            mainPanel?.SetActive(false);
        }
    }

    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
            mainPanel?.SetActive(true);
        }
    }

    public void OpenCollection()
    {
        if (collectionPanel != null)
        {
            collectionPanel.SetActive(true);
            mainPanel?.SetActive(false);
        }
    }

    public void CloseCollection()
    {
        if (collectionPanel != null)
        {
            collectionPanel.SetActive(false);
            mainPanel?.SetActive(true);
        }
    }
}