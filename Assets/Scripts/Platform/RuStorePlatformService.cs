using System;
using System.Collections.Generic;
using UnityEngine;

public class RuStorePlatformService : IPlatformService
{
    public bool IsAuthorized => false; // через VK ID
    public string PlayerName => "Игрок RuStore";
    public string PlayerId => "";

    public event Action OnAuthorizedChanged;
    public event Action OnDataLoaded;

    public void Login(Action<bool> callback)
    {
        Debug.Log("[RuStorePlatformService] Login via VK ID");
        // TODO: интеграция VK ID SDK для Android
        callback?.Invoke(true);
        OnAuthorizedChanged?.Invoke();
    }

    public void Logout()
    {
        Debug.Log("[RuStorePlatformService] Logout");
        OnAuthorizedChanged?.Invoke();
    }

    public void SaveGameData(Dictionary<string, object> data)
    {
        Debug.Log("[RuStorePlatformService] Save via VK ID cloud");
        // TODO: сохранять через VK ID SDK
    }

    public void LoadGameData(Action<Dictionary<string, object>> callback)
    {
        Debug.Log("[RuStorePlatformService] Load via VK ID cloud");
        callback?.Invoke(new Dictionary<string, object>());
        OnDataLoaded?.Invoke();
    }

    public void ShowRewardedVideo(Action<bool> callback)
    {
        Debug.Log("[RuStorePlatformService] Show Rewarded Video (RuStore SDK)");
        callback?.Invoke(true);
    }

    public void Purchase(string productId, Action<bool> callback)
    {
        Debug.Log($"[RuStorePlatformService] Purchase {productId} via RuStore IAP");
        callback?.Invoke(true);
    }

    public void ShareInVK(string text, string imageUrl, string link)
    {
        Debug.Log($"[RuStorePlatformService] Share in VK via VK SDK");
        // TODO: интеграция VK SDK для Android
    }

    public void OpenAppStore(string appUrl)
    {
        Debug.Log($"[RuStorePlatformService] Open App Store: {appUrl}");
        // В RuStore версии это не нужно, т.к. это само приложение
    }
}