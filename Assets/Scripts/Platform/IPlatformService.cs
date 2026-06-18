using System;
using System.Collections.Generic;

public interface IPlatformService
{
    bool IsAuthorized { get; }
    string PlayerName { get; }
    string PlayerId { get; }

    void Login(Action<bool> callback);
    void Logout();

    void SaveGameData(Dictionary<string, object> data);
    void LoadGameData(Action<Dictionary<string, object>> callback);

    void ShowRewardedVideo(Action<bool> callback);
    void Purchase(string productId, Action<bool> callback);
    void ShareInVK(string text, string imageUrl, string link);
    void OpenAppStore(string appUrl); // для кнопки "Скачать в RuStore"

    event Action OnAuthorizedChanged;
    event Action OnDataLoaded;
}