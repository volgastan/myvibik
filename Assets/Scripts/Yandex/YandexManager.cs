using UnityEngine;
using System;
using System.Collections.Generic;

public class YandexManager : MonoBehaviour
{
    public static YandexManager Instance { get; private set; }

    // Заглушки для совместимости (GameManager их не использует, но пусть будут)
    public bool IsAuthorized => false;
    public string PlayerName => "Игрок";
    public string PlayerId => "";

    public event Action OnAuthorizedChanged;
    public event Action OnDataLoaded;

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
    }

    private void Start()
    {
        OnDataLoaded?.Invoke();
        OnAuthorizedChanged?.Invoke();
    }

    // ========== СОХРАНЕНИЕ (через PlayerPrefs) ==========

    public void SaveGameData(Dictionary<string, object> data)
    {
        string json = JsonUtility.ToJson(new SerializableDictionaryWrapper(data));
        PlayerPrefs.SetString("CloudSave", json);
        PlayerPrefs.Save();
        Debug.Log("[YandexManager] Данные сохранены локально (PlayerPrefs)");
    }

    public void LoadGameData(Action<Dictionary<string, object>> callback)
    {
        if (PlayerPrefs.HasKey("CloudSave"))
        {
            string json = PlayerPrefs.GetString("CloudSave");
            try
            {
                var wrapper = JsonUtility.FromJson<SerializableDictionaryWrapper>(json);
                if (wrapper != null && wrapper.keys != null)
                {
                    var dict = new Dictionary<string, object>();
                    for (int i = 0; i < wrapper.keys.Count; i++)
                        dict[wrapper.keys[i]] = wrapper.values[i];
                    callback?.Invoke(dict);
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[YandexManager] Ошибка загрузки: {e.Message}");
            }
        }
        callback?.Invoke(null);
    }

    // ========== АВТОРИЗАЦИЯ (заглушка) ==========

    public void OpenAuthDialog()
    {
        Debug.Log("[YandexManager] Авторизация недоступна (локальный режим)");
    }

    // ========== ВСПОМОГАТЕЛЬНЫЙ КЛАСС ДЛЯ СЕРИАЛИЗАЦИИ ==========

    [Serializable]
    private class SerializableDictionaryWrapper
    {
        public List<string> keys = new List<string>();
        public List<string> values = new List<string>();
        public SerializableDictionaryWrapper() { }
        public SerializableDictionaryWrapper(Dictionary<string, object> dict)
        {
            foreach (var kvp in dict)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value?.ToString() ?? "");
            }
        }
    }
}