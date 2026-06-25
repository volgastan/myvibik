using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Платформенный сервис для облачного сохранения через HTTP-запросы к серверу.
/// Используется на всех платформах, в качестве fallback – EditorPlatformService.
/// </summary>
public class CloudPlatformService : IPlatformService
{
    private const string BaseUrl = "https://vibiki.duckdns.org/api/";
    private const string SaveEndpoint = "save.php";
    private const string LoadEndpoint = "load.php";
    private const float RequestTimeout = 10f;

    // Fallback-сервис (EditorPlatformService) для работы офлайн
    private readonly IPlatformService fallbackService = new EditorPlatformService();

    private bool isAuthorized = false;
    private string playerId = "";
    private string playerName = "CloudPlayer";

    // Объект для запуска корутин (не зависит от GameManager)
    private MonoBehaviour coroutineRunner;

    public event Action OnAuthorizedChanged;
    public event Action OnDataLoaded;

    public bool IsAuthorized => isAuthorized;
    public string PlayerId => playerId;
    public string PlayerName => playerName;
    public bool IsVK => Application.platform == RuntimePlatform.WebGLPlayer;
    public bool IsEditor => Application.isEditor;
    public bool IsMobile => Application.isMobilePlatform;

    // ------------------------------------------------------------------------
    // Конструктор
    // ------------------------------------------------------------------------

    public CloudPlatformService()
    {
        // Создаём раннер для корутин
        coroutineRunner = new GameObject("[CloudPlatformService] CoroutineRunner").AddComponent<CoroutineRunner>();
        UnityEngine.Object.DontDestroyOnLoad(coroutineRunner.gameObject);

        // Определяем playerId
        playerId = GetPlayerId();
        Debug.Log($"[CloudPlatformService] Инициализирован с PlayerId = {playerId}");
    }

    // ------------------------------------------------------------------------
    // Получение PlayerId
    // ------------------------------------------------------------------------

    private string GetPlayerId()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        // Сначала пытаемся получить через VK Bridge (если доступен)
        try
        {
            // Используем JavaScript-интерфейс для получения параметров (если есть)
            // Но пока используем парсинг URL
        }
        catch { }

        // Парсинг URL как запасной вариант
        try
        {
            string url = Application.absoluteURL;
            if (string.IsNullOrEmpty(url))
                return "webgl_unknown";

            int queryStart = url.IndexOf('?');
            if (queryStart == -1)
                return "webgl_unknown";

            string query = url.Substring(queryStart + 1);
            string[] pairs = query.Split('&');
            foreach (string pair in pairs)
            {
                string[] kv = pair.Split('=');
                if (kv.Length == 2 && kv[0] == "vk_user_id")
                {
                    string id = Uri.UnescapeDataString(kv[1]);
                    Debug.Log($"[CloudPlatformService] Найден vk_user_id из URL: {id}");
                    return id;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[CloudPlatformService] Ошибка парсинга URL: {e.Message}");
        }
        return "webgl_unknown";
        #else
        // В редакторе или на других платформах используем PlayerPrefs или константу
        string id = PlayerPrefs.GetString("VK_User_Id", null);
        if (string.IsNullOrEmpty(id))
        {
            // Если нет сохранённого – генерируем для тестов
            id = "editor_user_" + System.DateTime.Now.Ticks;
            PlayerPrefs.SetString("VK_User_Id", id);
            PlayerPrefs.Save();
        }
        return id;
        #endif
    }

    // ------------------------------------------------------------------------
    // Реализация IPlatformService
    // ------------------------------------------------------------------------

    public void Login(Action<bool> callback)
    {
        // Проверяем доступность интернета
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogWarning("[CloudPlatformService] Нет интернета – работаем офлайн.");
            isAuthorized = false;
            OnAuthorizedChanged?.Invoke();
            callback?.Invoke(false);
            return;
        }

        // Здесь можно сделать пинг сервера, но для скорости пока пропускаем.
        // Вместо этого при первом сохранении/загрузке ошибка приведёт к fallback.
        isAuthorized = true;
        OnAuthorizedChanged?.Invoke();
        callback?.Invoke(true);
        Debug.Log("[CloudPlatformService] Login выполнен успешно (режим онлайн)");
    }

    public void SaveGameData(Dictionary<string, object> data)
    {
        if (data == null || data.Count == 0)
        {
            Debug.LogWarning("[CloudPlatformService] Попытка сохранить пустые данные – пропускаем");
            return;
        }

        // Если не авторизованы или нет интернета – сразу в fallback
        if (!isAuthorized || Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("[CloudPlatformService] Нет соединения или не авторизованы – сохраняем локально.");
            fallbackService.SaveGameData(data);
            return;
        }

        coroutineRunner.StartCoroutine(SaveCoroutine(data));
    }

    public void LoadGameData(Action<Dictionary<string, object>> callback)
    {
        if (!isAuthorized || Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("[CloudPlatformService] Нет соединения или не авторизованы – загружаем локально.");
            fallbackService.LoadGameData(callback);
            return;
        }

        coroutineRunner.StartCoroutine(LoadCoroutine(callback));
    }

    public void ShareInVK(string text, string mediaUrl, string link)
    {
        Debug.Log($"[CloudPlatformService] ShareInVK: text={text}, media={mediaUrl}, link={link}");
        // Можно реализовать через VK Bridge, если нужно
    }

    public void OpenAppStore(string appId)
    {
        Debug.Log($"[CloudPlatformService] OpenAppStore: {appId}");
        // Можно открыть ссылку на магазин
    }

    // ------------------------------------------------------------------------
    // Приватные корутины
    // ------------------------------------------------------------------------

    private IEnumerator SaveCoroutine(Dictionary<string, object> data)
    {
        // Добавляем user_id в объект
        var payload = new Dictionary<string, object>(data);
        payload["user_id"] = playerId;

        // Сериализуем в обычный JSON-объект
        string jsonData = JsonUtility.ToJson(new DictionaryWrapper(payload));
        byte[] body = System.Text.Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(BaseUrl + SaveEndpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = (int)RequestTimeout;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[CloudPlatformService] Ошибка сохранения: {request.error}");
                fallbackService.SaveGameData(data);
            }
            else
            {
                Debug.Log($"[CloudPlatformService] Данные сохранены. Ответ: {request.downloadHandler.text}");
            }
        }
    }

    private IEnumerator LoadCoroutine(Action<Dictionary<string, object>> callback)
    {
        string url = $"{BaseUrl}{LoadEndpoint}?vk_user_id={UnityWebRequest.EscapeURL(playerId)}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = (int)RequestTimeout;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[CloudPlatformService] Ошибка загрузки: {request.error}");
                fallbackService.LoadGameData(callback);
                yield break;
            }

            string response = request.downloadHandler.text;
            try
            {
                Dictionary<string, object> result = null;

                // Пытаемся распарсить как объект с полями (новый формат)
                var wrapper = JsonUtility.FromJson<DictionaryWrapper>(response);
                if (wrapper != null && wrapper.fields != null)
                {
                    // Исправлено: вызываем ToDictionary() вместо прямого присвоения
                    result = wrapper.ToDictionary();
                }
                else
                {
                    // Если не получилось – пробуем как массив (старый формат)
                    var oldWrapper = JsonUtility.FromJson<SerializableDictionary>(response);
                    if (oldWrapper != null)
                        result = oldWrapper.ToDictionary();
                }

                if (result == null || result.Count == 0)
                {
                    Debug.Log("[CloudPlatformService] Загружены пустые данные или сервер вернул пустой ответ");
                    result = new Dictionary<string, object>();
                }
                else
                {
                    // Удаляем поле user_id, если оно есть (чтобы не мешать)
                    if (result.ContainsKey("user_id"))
                        result.Remove("user_id");
                    Debug.Log($"[CloudPlatformService] Загружено {result.Count} полей");
                }

                OnDataLoaded?.Invoke();
                callback?.Invoke(result);
            }
            catch (Exception e)
            {
                Debug.LogError($"[CloudPlatformService] Ошибка парсинга JSON: {e.Message}\nОтвет: {response}");
                fallbackService.LoadGameData(callback);
            }
        }
    }

    // ------------------------------------------------------------------------
    // Вспомогательные классы для сериализации
    // ------------------------------------------------------------------------

    /// <summary>
    /// Обёртка для обычного Dictionary, чтобы JsonUtility мог его сериализовать.
    /// Поля сериализуются как массив { key, value }.
    /// </summary>
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

        public DictionaryWrapper() { } // для десериализации

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

    // Оставляем старую обёртку для обратной совместимости (если сервер возвращает массив)
    [Serializable]
    private class SerializableDictionary
    {
        [Serializable]
        public class Entry
        {
            public string key;
            public string value;
        }

        public Entry[] entries;

        public Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            if (entries == null) return dict;
            foreach (var e in entries)
            {
                dict[e.key] = e.value;
            }
            return dict;
        }
    }

    // ------------------------------------------------------------------------
    // Внутренний класс-раннер для корутин
    // ------------------------------------------------------------------------

    private class CoroutineRunner : MonoBehaviour { }
}