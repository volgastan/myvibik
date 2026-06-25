using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Сервис для локального сохранения данных (используется в демо-версии).
/// Сохраняет весь словарь как JSON-строку в PlayerPrefs.
/// </summary>
public class LocalPlatformService : IPlatformService
{
    private const string SaveKey = "LocalSaveData";
    
    private bool isAuthorized = false;
    private string playerId = "local_player";
    private string playerName = "LocalPlayer";

    public bool IsAuthorized => isAuthorized;
    public string PlayerId => playerId;
    public string PlayerName => playerName;
    public bool IsVK => false;
    public bool IsEditor => false;
    public bool IsMobile => false;

    public event Action OnAuthorizedChanged;
    public event Action OnDataLoaded;

    public void Login(Action<bool> callback)
    {
        isAuthorized = true;
        OnAuthorizedChanged?.Invoke();
        callback?.Invoke(true);
        Debug.Log("[LocalPlatformService] Логин выполнен (локально)");
    }

    public void SaveGameData(Dictionary<string, object> data)
    {
        if (data == null || data.Count == 0)
        {
            Debug.LogWarning("[LocalPlatformService] Попытка сохранить пустые данные – пропускаем");
            return;
        }

        // Сериализуем словарь в JSON через вспомогательный класс
        var wrapper = new DictionaryWrapper(data);
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
        Debug.Log($"[LocalPlatformService] Данные сохранены локально (ключей: {data.Count})");
    }

    public void LoadGameData(Action<Dictionary<string, object>> callback)
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Debug.Log("[LocalPlatformService] Локальных данных нет, возвращаем пустой словарь");
            callback?.Invoke(new Dictionary<string, object>());
            OnDataLoaded?.Invoke();
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        try
        {
            var wrapper = JsonUtility.FromJson<DictionaryWrapper>(json);
            if (wrapper != null)
            {
                var data = wrapper.ToDictionary();
                Debug.Log($"[LocalPlatformService] Загружено {data.Count} полей");
                callback?.Invoke(data);
            }
            else
            {
                Debug.LogWarning("[LocalPlatformService] Не удалось десериализовать данные, возвращаем пустой словарь");
                callback?.Invoke(new Dictionary<string, object>());
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[LocalPlatformService] Ошибка десериализации: {e.Message}");
            callback?.Invoke(new Dictionary<string, object>());
        }
        OnDataLoaded?.Invoke();
    }

    public void ShareInVK(string text, string mediaUrl, string link)
    {
        Debug.Log($"[LocalPlatformService] ShareInVK: {text}, {mediaUrl}, {link}");
    }

    public void OpenAppStore(string appId)
    {
        Debug.Log($"[LocalPlatformService] OpenAppStore: {appId}");
    }

    // ---- Вспомогательный класс для сериализации (копия из CloudPlatformService) ----
    [Serializable]
    private class DictionaryWrapper
    {
        [Serializable]
        public class Field
        {
            public string key;
            public string value;
        }

        public Field[] fields;

        public DictionaryWrapper() { }

        public DictionaryWrapper(Dictionary<string, object> dict)
        {
            if (dict == null)
            {
                fields = new Field[0];
                return;
            }
            fields = new Field[dict.Count];
            int i = 0;
            foreach (var kv in dict)
            {
                fields[i] = new Field { key = kv.Key, value = kv.Value?.ToString() ?? "" };
                i++;
            }
        }

        public Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            if (fields == null) return dict;
            foreach (var f in fields)
            {
                dict[f.key] = f.value;
            }
            return dict;
        }
    }
}