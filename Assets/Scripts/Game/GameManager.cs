using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Настройки")]
    [SerializeField] private int maxDailyCoins = 5;
    [SerializeField] private int coinsPerAction = 1;
    [SerializeField] private int totalStickers = 8;

    // Текущее состояние
    public int Coins { get; private set; }
    public int DaysCount { get; private set; }
    public bool[] StickersCollected { get; private set; }
    public int SelectedCharacterIndex { get; private set; }
    public int SelectedBackgroundIndex { get; private set; }
    public int SelectedCostumeIndex { get; private set; }

    // События
    public System.Action OnCoinsChanged;
    public System.Action OnDaysChanged;
    public System.Action OnStickersChanged;
    public System.Action OnCharacterChanged;
    public System.Action OnBackgroundChanged;
    public System.Action OnCostumeChanged;
    public System.Action<int> OnBackgroundUnlocked;
    public System.Action<int> OnCharacterUnlocked;

    private string lastVisitDate = "";
    private int todayCoinsEarned = 0;
    private bool isDataLoaded = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        StickersCollected = new bool[totalStickers];
        LoadAllData();
    }

    private void Start()
    {
        CheckNewDay();
        isDataLoaded = true;
    }

    // ========== ЗАГРУЗКА / СОХРАНЕНИЕ ==========

    private void LoadAllData()
    {
        if (YandexManager.Instance != null)
        {
            // Пытаемся загрузить из облака
            YandexManager.Instance.LoadGameData(OnCloudDataLoaded);
        }
        else
        {
            // Если YandexManager нет — из PlayerPrefs
            LoadFromPlayerPrefs();
            ApplyLoadedData();
        }
    }

    private void OnCloudDataLoaded(Dictionary<string, object> cloudData)
    {
        if (cloudData != null && cloudData.Count > 0)
        {
            LoadFromDictionary(cloudData);
            Debug.Log("[GameManager] Данные загружены из облака");
        }
        else
        {
            LoadFromPlayerPrefs();
            Debug.Log("[GameManager] Облачных данных нет, загружено из PlayerPrefs");
        }
        ApplyLoadedData();
    }

    private void LoadFromDictionary(Dictionary<string, object> data)
    {
        if (data.ContainsKey("Coins")) Coins = System.Convert.ToInt32(data["Coins"]);
        if (data.ContainsKey("DaysCount")) DaysCount = System.Convert.ToInt32(data["DaysCount"]);
        if (data.ContainsKey("LastVisitDate")) lastVisitDate = data["LastVisitDate"].ToString();
        if (data.ContainsKey("TodayCoinsEarned")) todayCoinsEarned = System.Convert.ToInt32(data["TodayCoinsEarned"]);

        if (data.ContainsKey("Stickers"))
        {
            string stickersStr = data["Stickers"].ToString();
            for (int i = 0; i < totalStickers && i < stickersStr.Length; i++)
                StickersCollected[i] = stickersStr[i] == '1';
        }

        if (data.ContainsKey("SelectedCharacter")) SelectedCharacterIndex = System.Convert.ToInt32(data["SelectedCharacter"]);
        if (data.ContainsKey("SelectedBackground")) SelectedBackgroundIndex = System.Convert.ToInt32(data["SelectedBackground"]);
        if (data.ContainsKey("SelectedCostume")) SelectedCostumeIndex = System.Convert.ToInt32(data["SelectedCostume"]);
    }

    private void LoadFromPlayerPrefs()
    {
        Coins = PlayerPrefs.GetInt("Coins", 0);
        DaysCount = PlayerPrefs.GetInt("DaysCount", 0);
        lastVisitDate = PlayerPrefs.GetString("LastVisitDate", "");
        todayCoinsEarned = PlayerPrefs.GetInt("TodayCoinsEarned", 0);

        for (int i = 0; i < totalStickers; i++)
            StickersCollected[i] = PlayerPrefs.GetInt($"Sticker_{i}", 0) == 1;

        SelectedCharacterIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);
        SelectedBackgroundIndex = PlayerPrefs.GetInt("SelectedBackground", 0);
        SelectedCostumeIndex = PlayerPrefs.GetInt("SelectedCostume", 0);
    }

    private void ApplyLoadedData()
    {
        OnCoinsChanged?.Invoke();
        OnDaysChanged?.Invoke();
        OnStickersChanged?.Invoke();
        OnCharacterChanged?.Invoke();
        OnBackgroundChanged?.Invoke();
        OnCostumeChanged?.Invoke();
    }

    private void SaveAllData()
    {
        // Всегда сохраняем в PlayerPrefs как резерв
        SaveToPlayerPrefs();

        // В облако
        if (YandexManager.Instance != null)
        {
            var data = new Dictionary<string, object>
            {
                ["Coins"] = Coins,
                ["DaysCount"] = DaysCount,
                ["LastVisitDate"] = lastVisitDate,
                ["TodayCoinsEarned"] = todayCoinsEarned,
                ["SelectedCharacter"] = SelectedCharacterIndex,
                ["SelectedBackground"] = SelectedBackgroundIndex,
                ["SelectedCostume"] = SelectedCostumeIndex
            };
            char[] stickerChars = new char[totalStickers];
            for (int i = 0; i < totalStickers; i++)
                stickerChars[i] = StickersCollected[i] ? '1' : '0';
            data["Stickers"] = new string(stickerChars);

            YandexManager.Instance.SaveGameData(data);
        }
    }

    private void SaveToPlayerPrefs()
    {
        PlayerPrefs.SetInt("Coins", Coins);
        PlayerPrefs.SetInt("DaysCount", DaysCount);
        PlayerPrefs.SetString("LastVisitDate", lastVisitDate);
        PlayerPrefs.SetInt("TodayCoinsEarned", todayCoinsEarned);

        for (int i = 0; i < StickersCollected.Length; i++)
            PlayerPrefs.SetInt($"Sticker_{i}", StickersCollected[i] ? 1 : 0);

        PlayerPrefs.SetInt("SelectedCharacter", SelectedCharacterIndex);
        PlayerPrefs.SetInt("SelectedBackground", SelectedBackgroundIndex);
        PlayerPrefs.SetInt("SelectedCostume", SelectedCostumeIndex);

        PlayerPrefs.Save();
    }

    // ========== ИГРОВАЯ ЛОГИКА ==========

    private void CheckNewDay()
    {
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");
        if (lastVisitDate != today)
        {
            todayCoinsEarned = 0;
            DaysCount++;
            OnDaysChanged?.Invoke();
            lastVisitDate = today;
            SaveAllData();

            if (DaysCount == 7)
            {
                UnlockBackground(1);
            }
        }
    }

    public void PerformAction(string actionType)
    {
        // Начисляем монету
        if (todayCoinsEarned < maxDailyCoins)
        {
            Coins += coinsPerAction;
            todayCoinsEarned++;
            OnCoinsChanged?.Invoke();
            SaveAllData();
        }

        TryGiveSticker();
        Debug.Log($"[GameManager] Action performed: {actionType}");
    }

    private void TryGiveSticker()
    {
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");
        string lastStickerDate = PlayerPrefs.GetString("LastStickerDate", "");
        if (lastStickerDate == today) return;

        List<int> missing = new List<int>();
        for (int i = 0; i < StickersCollected.Length; i++)
            if (!StickersCollected[i]) missing.Add(i);

        if (missing.Count == 0) return;

        int randomIndex = missing[Random.Range(0, missing.Count)];
        StickersCollected[randomIndex] = true;
        PlayerPrefs.SetString("LastStickerDate", today);
        OnStickersChanged?.Invoke();
        SaveAllData();

        bool allCollected = true;
        for (int i = 0; i < StickersCollected.Length; i++)
        {
            if (!StickersCollected[i]) { allCollected = false; break; }
        }
        if (allCollected)
        {
            UnlockCharacter(1);
        }
    }

    // ========== РАЗБЛОКИРОВКИ ==========

    private void UnlockBackground(int index)
    {
        PlayerPrefs.SetInt($"BackgroundUnlocked_{index}", 1);
        OnBackgroundUnlocked?.Invoke(index);
        SaveAllData();
    }

    private void UnlockCharacter(int index)
    {
        PlayerPrefs.SetInt($"CharacterUnlocked_{index}", 1);
        OnCharacterUnlocked?.Invoke(index);
        SaveAllData();
    }

    // ========== МАГАЗИН ==========

    public bool BuyBackground(int index, int cost)
    {
        if (Coins < cost || !IsBackgroundUnlocked(index)) return false;
        Coins -= cost;
        SelectedBackgroundIndex = index;
        OnCoinsChanged?.Invoke();
        OnBackgroundChanged?.Invoke();
        SaveAllData();
        return true;
    }

    public bool BuyCostume(int index, int cost)
    {
        if (Coins < cost) return false;
        Coins -= cost;
        SelectedCostumeIndex = index;
        OnCoinsChanged?.Invoke();
        OnCostumeChanged?.Invoke();
        SaveAllData();
        return true;
    }

    // ========== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ==========

    public bool IsCharacterUnlocked(int index)
    {
        if (index == 0) return true;
        return PlayerPrefs.GetInt($"CharacterUnlocked_{index}", 0) == 1;
    }

    public bool IsBackgroundUnlocked(int index)
    {
        if (index == 0) return true;
        return PlayerPrefs.GetInt($"BackgroundUnlocked_{index}", 0) == 1;
    }

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        LoadFromPlayerPrefs();
        ApplyLoadedData();
        SaveAllData();
    }
}