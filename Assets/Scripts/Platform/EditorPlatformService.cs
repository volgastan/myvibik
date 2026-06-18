using System;
using System.Collections.Generic;
using UnityEngine;

public class EditorPlatformService : IPlatformService
{
    public bool IsAuthorized => true;
    public string PlayerName => "Игрок (Editor)";
    public string PlayerId => "editor_001";

    public event Action OnAuthorizedChanged;
    public event Action OnDataLoaded;

    private Dictionary<string, object> localData = new Dictionary<string, object>();

    public void Login(Action<bool> callback)
    {
        Debug.Log("[EditorPlatformService] Login (always success)");
        callback?.Invoke(true);
        OnAuthorizedChanged?.Invoke();
    }

    public void Logout()
    {
        Debug.Log("[EditorPlatformService] Logout");
        OnAuthorizedChanged?.Invoke();
    }

    public void SaveGameData(Dictionary<string, object> data)
    {
        foreach (var kvp in data)
            localData[kvp.Key] = kvp.Value;
        Debug.Log("[EditorPlatformService] Data saved locally");
    }

    public void LoadGameData(Action<Dictionary<string, object>> callback)
    {
        Debug.Log("[EditorPlatformService] Data loaded locally");
        callback?.Invoke(localData);
        OnDataLoaded?.Invoke();
    }

    public void ShowRewardedVideo(Action<bool> callback)
    {
        Debug.Log("[EditorPlatformService] Rewarded video shown (simulated)");
        callback?.Invoke(true);
    }

    public void Purchase(string productId, Action<bool> callback)
    {
        Debug.Log($"[EditorPlatformService] Purchase {productId} (simulated)");
        callback?.Invoke(true);
    }

    public void ShareInVK(string text, string imageUrl, string link)
    {
        Debug.Log($"[EditorPlatformService] Share in VK: {text}");
        // В редакторе просто выводим в консоль
    }

    public void OpenAppStore(string appUrl)
    {
        Debug.Log($"[EditorPlatformService] Open App Store: {appUrl}");
        Application.OpenURL(appUrl);
    }
}