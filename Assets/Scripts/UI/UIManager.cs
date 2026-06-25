using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Vibies.Localization;

public class UIManager : MonoBehaviour
{
    [Header("Ресурсы")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private VibikController vibikController;

    [Header("Панели")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject collectionPanel;

    // [DEMO] Ссылка на DemoModeManager (для демо-версии)
    [Header("Демо-режим")]
    [SerializeField] private DemoModeManager demoModeManager;

    [Header("Отображение ресурсов")]
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text daysText;
    [SerializeField] private TMP_Text hugCountText;

    [Header("Коллекция (паззл)")]
    [SerializeField] private Transform puzzleGridParent;
    [SerializeField] private GameObject puzzleSlotPrefab;

    [Header("Кнопки действий")]
    [SerializeField] private Button actionFeedButton;
    [SerializeField] private Button actionPlayButton;

    [Header("Кнопки навигации")]
    [SerializeField] private Button shopButton;
    [SerializeField] private Button collectionButton;
    [SerializeField] private Button shareButton;
    [SerializeField] private Button downloadButton;

    private List<StickerSlot> puzzleSlots = new List<StickerSlot>();

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================

    private void Awake()
    {
        // Поиск GameManager, если не назначен
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (vibikController == null)
            vibikController = FindFirstObjectByType<VibikController>();

        if (gameManager != null)
        {
            gameManager.OnCoinsChanged += UpdateCoinsUI;
            gameManager.OnDaysChanged += UpdateDaysUI;
            gameManager.OnPuzzleChanged += UpdatePuzzleUI;
            gameManager.OnHugCountChanged += UpdateHugUI;
        }
        else
        {
            Debug.LogError("[UIManager] GameManager не найден!");
        }

        CreatePuzzleSlots();

        if (CsvLocalizationManager.Instance != null)
            CsvLocalizationManager.Instance.OnLanguageChanged += UpdateCoinsUI;

        // Кнопки действий
        if (actionFeedButton != null)
            actionFeedButton.onClick.AddListener(() => {
                gameManager?.PerformAction("feed");
                vibikController?.PlayEatAnimation();
            });
        if (actionPlayButton != null)
            actionPlayButton.onClick.AddListener(() => {
                gameManager?.PerformAction("play");
                vibikController?.PlayPlayAnimation();
            });

        // Кнопки навигации
        if (shopButton != null)
            shopButton.onClick.AddListener(OpenShop);
        if (collectionButton != null)
            collectionButton.onClick.AddListener(OpenCollection);
        if (shareButton != null)
            shareButton.onClick.AddListener(ShareInVK);
        if (downloadButton != null)
            downloadButton.onClick.AddListener(DownloadApp);

        // [DEMO] Поиск DemoModeManager, если не назначен в инспекторе
        if (demoModeManager == null)
            demoModeManager = FindFirstObjectByType<DemoModeManager>();

        // [DEMO] Подписка на событие закрытия демо-диалога
        if (demoModeManager != null)
        {
            demoModeManager.OnDialogClosed += OnDemoDialogClosed;
        }

        UpdateAllUI();
        CloseShop();
        CloseCollection();

        if (mainPanel != null)
            mainPanel.SetActive(true);

        VoiceTipsManager.Instance?.PlayTipDay();
    }

    private void OnDestroy()
    {
        // Отписка от событий GameManager
        if (gameManager != null)
        {
            gameManager.OnCoinsChanged -= UpdateCoinsUI;
            gameManager.OnDaysChanged -= UpdateDaysUI;
            gameManager.OnPuzzleChanged -= UpdatePuzzleUI;
            gameManager.OnHugCountChanged -= UpdateHugUI;
        }

        if (CsvLocalizationManager.Instance != null)
            CsvLocalizationManager.Instance.OnLanguageChanged -= UpdateCoinsUI;

        // Отписка от кнопок
        if (actionFeedButton != null)
            actionFeedButton.onClick.RemoveAllListeners();
        if (actionPlayButton != null)
            actionPlayButton.onClick.RemoveAllListeners();
        if (shopButton != null)
            shopButton.onClick.RemoveAllListeners();
        if (collectionButton != null)
            collectionButton.onClick.RemoveAllListeners();
        if (shareButton != null)
            shareButton.onClick.RemoveAllListeners();
        if (downloadButton != null)
            downloadButton.onClick.RemoveAllListeners();

        // [DEMO] Отписка от события DemoModeManager
        if (demoModeManager != null)
        {
            demoModeManager.OnDialogClosed -= OnDemoDialogClosed;
        }
    }

    // ========================================================================
    // Обновление UI
    // ========================================================================

    private void UpdateAllUI()
    {
        UpdateCoinsUI();
        UpdateDaysUI();
        UpdateHugUI();
        UpdatePuzzleUI();
    }

    private void UpdateCoinsUI()
    {
        if (coinsText != null && gameManager != null)
        {
            string label = CsvLocalizationManager.Instance != null 
                ? CsvLocalizationManager.Instance.GetText("coins_label") 
                : "Монеты";
            coinsText.text = $"{label}: {gameManager.Coins}";
        }
    }

    private void UpdateDaysUI()
    {
        if (daysText != null && gameManager != null)
        {
            string label = CsvLocalizationManager.Instance != null 
                ? CsvLocalizationManager.Instance.GetText("days_label") 
                : "Дни";
            daysText.text = $"{label}: {gameManager.DaysCount}";
        }
    }

    private void UpdateHugUI()
    {
        if (hugCountText != null && gameManager != null)
        {
            string label = CsvLocalizationManager.Instance != null 
                ? CsvLocalizationManager.Instance.GetText("hugs_label") 
                : "Объятий";
            hugCountText.text = $"{label} {gameManager.HugCount}";
        }
    }

    private void UpdatePuzzleUI()
    {
        if (gameManager == null) return;
        bool[] parts = gameManager.PuzzlePartsCollected;
        for (int i = 0; i < puzzleSlots.Count && i < parts.Length; i++)
        {
            puzzleSlots[i].SetCollected(parts[i]);
        }
    }

    private void CreatePuzzleSlots()
    {
        if (puzzleGridParent == null || puzzleSlotPrefab == null)
        {
            Debug.LogWarning("[UIManager] puzzleGridParent или puzzleSlotPrefab не назначены");
            return;
        }

        foreach (Transform child in puzzleGridParent)
            Destroy(child.gameObject);
        puzzleSlots.Clear();

        int totalParts = 8;
        for (int i = 0; i < totalParts; i++)
        {
            GameObject slot = Instantiate(puzzleSlotPrefab, puzzleGridParent);
            StickerSlot slotScript = slot.GetComponent<StickerSlot>();
            if (slotScript == null)
                slotScript = slot.AddComponent<StickerSlot>();
            slotScript.SetIndex(i);
            puzzleSlots.Add(slotScript);
        }
    }

    // ========================================================================
    // Навигация
    // ========================================================================

    public void OpenShop()
    {
#if VK_DEMO
        // Демо-версия: показываем диалог вместо магазина
        if (demoModeManager != null)
        {
            demoModeManager.ShowDemoDialog();
            // Скрываем главную панель, чтобы диалог был поверх
            if (mainPanel != null)
                mainPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("[UIManager] DemoModeManager не назначен! Открываем магазин как запасной вариант.");
            OpenShopNormal();
        }
#else
        // Полная версия: открываем обычный магазин
        OpenShopNormal();
#endif
    }

    // Вспомогательный метод для открытия магазина (общий код)
    private void OpenShopNormal()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            if (mainPanel != null)
                mainPanel.SetActive(false);
            VoiceTipsManager.Instance?.PlayTipShop();
        }
    }

    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
            if (mainPanel != null)
                mainPanel.SetActive(true);
        }
    }

    public void OpenCollection()
    {
        if (collectionPanel != null)
        {
            collectionPanel.SetActive(true);
            if (mainPanel != null)
                mainPanel.SetActive(false);
        }
    }

    public void CloseCollection()
    {
        if (collectionPanel != null)
        {
            collectionPanel.SetActive(false);
            if (mainPanel != null)
                mainPanel.SetActive(true);
        }
    }

    // ========================================================================
    // Социальные механики
    // ========================================================================

    private void ShareInVK()
    {
        string text = "Я обнимаю Вайбика! ❤️ Присоединяйся!";
        string imageUrl = "";
        string link = "https://vk.com/app/...";
        PlatformService.Instance.ShareInVK(text, imageUrl, link);
    }

    private void DownloadApp()
    {
        string appUrl = "https://apps.rustore.ru/app/...";
        PlatformService.Instance.OpenAppStore(appUrl);
    }

    // ========================================================================
    // [DEMO] Обработчик закрытия демо-диалога
    // ========================================================================

    /// <summary>
    /// Вызывается, когда демо-диалог закрывается (через событие OnDialogClosed).
    /// Показывает главную панель обратно.
    /// </summary>
    private void OnDemoDialogClosed()
    {
        if (mainPanel != null)
            mainPanel.SetActive(true);
    }
}