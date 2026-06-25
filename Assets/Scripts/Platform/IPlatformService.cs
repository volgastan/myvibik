using System;
using System.Collections.Generic;

public interface IPlatformService
{
    // Свойства
    bool IsAuthorized { get; }
    string PlayerId { get; }
    string PlayerName { get; }
    bool IsVK { get; }
    bool IsEditor { get; }
    bool IsMobile { get; }

    // События (без параметров)
    event Action OnAuthorizedChanged;
    event Action OnDataLoaded;

    // Методы
    void Login(Action<bool> callback);
    void SaveGameData(Dictionary<string, object> data);
    void LoadGameData(Action<Dictionary<string, object>> callback);
    void ShareInVK(string text, string mediaUrl, string link);
    void OpenAppStore(string appId);
}