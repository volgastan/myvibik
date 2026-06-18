using System;
using System.Collections.Generic;
using UnityEngine;

public class VKPlatformService : IPlatformService
{
    public bool IsAuthorized => false; // будет запрос через VK Bridge
    public string PlayerName => "Игрок VK";
    public string PlayerId => "";

    public event Action OnAuthorizedChanged;
    public event Action OnDataLoaded;

    public void Login(Action<bool> callback)
    {
        Debug.Log("[VKPlatformService] Login via VK Bridge");
        // TODO: интеграция VK Bridge
        callback?.Invoke(true);
        OnAuthorizedChanged?.Invoke();
    }

    public void Logout()
    {
        Debug.Log("[VKPlatformService] Logout");
        OnAuthorizedChanged?.Invoke();
    }

    public void SaveGameData(Dictionary<string, object> data)
    {
        Debug.Log("[VKPlatformService] Save via VK Bridge");
        // TODO: сохранять через VK Bridge
    }

    public void LoadGameData(Action<Dictionary<string, object>> callback)
    {
        Debug.Log("[VKPlatformService] Load via VK Bridge");
        // TODO: загружать через VK Bridge
        callback?.Invoke(new Dictionary<string, object>());
        OnDataLoaded?.Invoke();
    }

    public void ShowRewardedVideo(Action<bool> callback)
    {
        Debug.Log("[VKPlatformService] Show Rewarded Video via VK Bridge");
        callback?.Invoke(true);
    }

    public void Purchase(string productId, Action<bool> callback)
    {
        Debug.Log($"[VKPlatformService] Purchase {productId} -> показать тизер 'Доступно в приложении'");
        // В VK-версии покупка недоступна, показываем заглушку
        callback?.Invoke(false);
    }

    public void ShareInVK(string text, string imageUrl, string link)
    {
        Debug.Log($"[VKPlatformService] Share in VK: {text}");
        // TODO: через VK Bridge
    }

    public void OpenAppStore(string appUrl)
    {
        Debug.Log($"[VKPlatformService] Open App Store: {appUrl}");
        Application.OpenURL(appUrl);
    }
}