using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Настройки")]
    [SerializeField] private int maxDailyCoins = 5;
    [SerializeField] private int coinsPerAction = 1;
    [SerializeField] private int totalPuzzleParts = 8;

    public int Coins { get; private set; }
    public int DaysCount { get; private set; }
    public bool[] PuzzlePartsCollected { get; private set; }
    public int SelectedCharacterIndex { get; private set; }
    public int SelectedBackgroundIndex { get; private set; }
    public int SelectedCostumeIndex { get; private set; }
    public int HugCount { get; private set; }

    public System.Action OnCoinsChanged;
    public System.Action OnDaysChanged;
    public System.Action OnPuzzleChanged;
    public System.Action OnCharacterChanged;
    public System.Action OnBackgroundChanged;
    public System.Action OnCostumeChanged;
    public System.Action<int> OnBackgroundUnlocked;
    public System.Action<int> OnCharacterUnlocked;
    public System.Action OnHugCountChanged;

    private string lastVisitDate = "";
    private int todayCoinsEarned = 0;
    private bool isBridgeReadyChecked = false;

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

        PuzzlePartsCollected = new bool[totalPuzzleParts];
    }

    private void Start()
    {
        // Используем FindFirstObjectByType (новый API)
        VkBridgeController bridgeController = FindFirstObjectByType<VkBridgeController>();

        if (bridgeController != null && bridgeController.IsBridgeReady)
        {
            // Мост уже готов – сразу логинимся
            PlatformService.Instance.Login(OnLoginResult);
        }
        else if (bridgeController != null)
        {
            // Подписываемся на событие готовности
            bridgeController.OnBridgeReady += () =>
            {
                if (!isBridgeReadyChecked)
                {
                    isBridgeReadyChecked = true;
                    PlatformService.Instance.Login(OnLoginResult);
                }
            };
            // Таймаут – если Bridge не стал готов за 5 секунд, всё равно работаем офлайн
            Invoke(nameof(LoginFallback), 5f);
        }
        else
        {
            // Контроллера нет – сразу офлайн
            PlatformService.Instance.Login(OnLoginResult);
        }
    }

    private void LoginFallback()
    {
        if (!isBridgeReadyChecked)
        {
            isBridgeReadyChecked = true;
            Debug.LogWarning("[GameManager] Таймаут ожидания Bridge – работаем офлайн");
            PlatformService.Instance.Login(OnLoginResult);
        }
    }

    private void OnLoginResult(bool success)
    {
        if (success)
            Debug.Log("[GameManager] Платформа инициализирована успешно");
        else
            Debug.LogWarning("[GameManager] Платформа не инициализирована, работаем в офлайн-режиме");

        LoadAllData();
    }

    private void LoadAllData()
    {
        PlatformService.Instance.LoadGameData(OnCloudDataLoaded);
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
        CheckNewDay();
    }

    private void LoadFromDictionary(Dictionary<string, object> data)
    {
        if (data.ContainsKey("Coins")) Coins = System.Convert.ToInt32(data["Coins"]);
        if (data.ContainsKey("DaysCount")) DaysCount = System.Convert.ToInt32(data["DaysCount"]);
        if (data.ContainsKey("LastVisitDate")) lastVisitDate = data["LastVisitDate"].ToString();
        if (data.ContainsKey("TodayCoinsEarned")) todayCoinsEarned = System.Convert.ToInt32(data["TodayCoinsEarned"]);
        if (data.ContainsKey("HugCount")) HugCount = System.Convert.ToInt32(data["HugCount"]);

        if (data.ContainsKey("PuzzleParts"))
        {
            string partsStr = data["PuzzleParts"].ToString();
            for (int i = 0; i < totalPuzzleParts && i < partsStr.Length; i++)
                PuzzlePartsCollected[i] = partsStr[i] == '1';
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
        HugCount = PlayerPrefs.GetInt("HugCount", 0);

        for (int i = 0; i < totalPuzzleParts; i++)
            PuzzlePartsCollected[i] = PlayerPrefs.GetInt($"PuzzlePart_{i}", 0) == 1;

        SelectedCharacterIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);
        SelectedBackgroundIndex = PlayerPrefs.GetInt("SelectedBackground", 0);
        SelectedCostumeIndex = PlayerPrefs.GetInt("SelectedCostume", 0);
    }

    private void ApplyLoadedData()
    {
        OnCoinsChanged?.Invoke();
        OnDaysChanged?.Invoke();
        OnPuzzleChanged?.Invoke();
        OnCharacterChanged?.Invoke();
        OnBackgroundChanged?.Invoke();
        OnCostumeChanged?.Invoke();
        OnHugCountChanged?.Invoke();
    }

    private void SaveAllData()
    {
        SaveToPlayerPrefs();

        var data = new Dictionary<string, object>
        {
            ["Coins"] = Coins,
            ["DaysCount"] = DaysCount,
            ["LastVisitDate"] = lastVisitDate,
            ["TodayCoinsEarned"] = todayCoinsEarned,
            ["SelectedCharacter"] = SelectedCharacterIndex,
            ["SelectedBackground"] = SelectedBackgroundIndex,
            ["SelectedCostume"] = SelectedCostumeIndex,
            ["HugCount"] = HugCount
        };
        char[] partChars = new char[totalPuzzleParts];
        for (int i = 0; i < totalPuzzleParts; i++)
            partChars[i] = PuzzlePartsCollected[i] ? '1' : '0';
        data["PuzzleParts"] = new string(partChars);

        PlatformService.Instance.SaveGameData(data);
    }

    private void SaveToPlayerPrefs()
    {
        PlayerPrefs.SetInt("Coins", Coins);
        PlayerPrefs.SetInt("DaysCount", DaysCount);
        PlayerPrefs.SetString("LastVisitDate", lastVisitDate);
        PlayerPrefs.SetInt("TodayCoinsEarned", todayCoinsEarned);
        PlayerPrefs.SetInt("HugCount", HugCount);

        for (int i = 0; i < PuzzlePartsCollected.Length; i++)
            PlayerPrefs.SetInt($"PuzzlePart_{i}", PuzzlePartsCollected[i] ? 1 : 0);

        PlayerPrefs.SetInt("SelectedCharacter", SelectedCharacterIndex);
        PlayerPrefs.SetInt("SelectedBackground", SelectedBackgroundIndex);
        PlayerPrefs.SetInt("SelectedCostume", SelectedCostumeIndex);

        PlayerPrefs.Save();
    }

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
        if (todayCoinsEarned < maxDailyCoins)
        {
            Coins += coinsPerAction;
            todayCoinsEarned++;
            OnCoinsChanged?.Invoke();
            SaveAllData();

            if (todayCoinsEarned == 1)
                VoiceTipsManager.Instance?.PlayTipCoin();
        }

        if (actionType == "feed")
            VoiceTipsManager.Instance?.PlayTipFeed();
        else if (actionType == "play")
            VoiceTipsManager.Instance?.PlayTipPlay();

        TryGivePuzzlePart();
        Debug.Log($"[GameManager] Action performed: {actionType}");
    }

    public void AddHug()
    {
        HugCount++;
        OnHugCountChanged?.Invoke();
        SaveAllData();
    }

    private void TryGivePuzzlePart()
    {
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");
        string lastPuzzleDate = PlayerPrefs.GetString("LastPuzzleDate", "");
        if (lastPuzzleDate == today) return;

        List<int> missing = new List<int>();
        for (int i = 0; i < PuzzlePartsCollected.Length; i++)
            if (!PuzzlePartsCollected[i]) missing.Add(i);

        if (missing.Count == 0) return;

        int randomIndex = missing[Random.Range(0, missing.Count)];
        PuzzlePartsCollected[randomIndex] = true;
        PlayerPrefs.SetString("LastPuzzleDate", today);
        OnPuzzleChanged?.Invoke();
        SaveAllData();

        VoiceTipsManager.Instance?.PlayTipCollection();

        bool allCollected = true;
        for (int i = 0; i < PuzzlePartsCollected.Length; i++)
        {
            if (!PuzzlePartsCollected[i]) { allCollected = false; break; }
        }
        if (allCollected)
        {
            UnlockCharacter(1);
        }
    }

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