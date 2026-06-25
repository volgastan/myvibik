using System;
using System.Collections.Generic;
using UnityEngine;

public class EditorPlatformService : IPlatformService
{
    private bool isAuthorized = false;
    private string playerId = "editor_player";
    private string playerName = "Editor";

    public bool IsAuthorized => isAuthorized;
    public string PlayerId => playerId;
    public string PlayerName => playerName;
    public bool IsVK => false;
    public bool IsEditor => true;
    public bool IsMobile => false;

    public event Action OnAuthorizedChanged;
    public event Action OnDataLoaded;

    public void Login(Action<bool> callback)
    {
        isAuthorized = true;
        OnAuthorizedChanged?.Invoke();
        callback?.Invoke(true);
    }

    public void SaveGameData(Dictionary<string, object> data)
    {
        Debug.Log("[EditorPlatformService] Saving data in editor (PlayerPrefs)");
        foreach (var kv in data)
        {
            PlayerPrefs.SetString(kv.Key, kv.Value?.ToString() ?? "");
        }
        PlayerPrefs.Save();
    }

    public void LoadGameData(Action<Dictionary<string, object>> callback)
    {
        Debug.Log("[EditorPlatformService] Loading data from editor (PlayerPrefs)");
        Dictionary<string, object> data = new Dictionary<string, object>();
        // Здесь можно загрузить известные ключи, но для простоты возвращаем пустой словарь
        callback?.Invoke(data);
    }

    public void ShareInVK(string text, string mediaUrl, string link)
    {
        Debug.Log($"[EditorPlatformService] Share in VK: {text}, {mediaUrl}, {link}");
    }

    public void OpenAppStore(string appId)
    {
        Debug.Log($"[EditorPlatformService] Open App Store: {appId}");
    }
}