using UnityEngine;
using System.Collections.Generic;
using YG;

public class YandexManager : MonoBehaviour
{
    public static YandexManager Instance { get; private set; }

    [Header("Настройки")]
    [SerializeField] private bool useCloudSave = true;

    public bool IsAuthorized => YG2.Player.auth;
    public string PlayerName => YG2.Player.name;
    public string PlayerId => YG2.Player.id;

    public System.Action OnAuthorizedChanged;
    public System.Action OnDataLoaded;

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

        YG2.OnGetSDKData += OnSDKReady;
        YG2.OnAuthSuccess += OnAuthSuccess;
        YG2.OnAuthFail += OnAuthFail;
    }

    private void OnDestroy()
    {
        YG2.OnGetSDKData -= OnSDKReady;
        YG2.OnAuthSuccess -= OnAuthSuccess;
        YG2.OnAuthFail -= OnAuthFail;
    }

    private void OnSDKReady()
    {
        Debug.Log("[YandexManager] SDK готов");
        YG2.GameReadyAPI();
        OnDataLoaded?.Invoke();
        OnAuthorizedChanged?.Invoke();
    }

    private void OnAuthSuccess()
    {
        Debug.Log("[YandexManager] Авторизация успешна");
        OnAuthorizedChanged?.Invoke();
    }

    private void OnAuthFail()
    {
        Debug.Log("[YandexManager] Авторизация не удалась");
        OnAuthorizedChanged?.Invoke();
    }

    public void SaveGameData(Dictionary<string, object> data)
    {
        if (!useCloudSave) return;
        if (YG2.Saves == null)
        {
            Debug.LogWarning("[YandexManager] YG2.Saves is null");
            return;
        }

        string json = JsonUtility.ToJson(new SerializableDictionaryWrapper(data));
        YG2.Saves.cloudData = json;
        YG2.SaveProgress();
        Debug.Log("[YandexManager] Data saved to cloud");
    }

    public void LoadGameData(System.Action<Dictionary<string, object>> callback)
    {
        if (!useCloudSave)
        {
            callback?.Invoke(null);
            return;
        }

        if (!YG2.isSDKEnabled)
        {
            System.Action onReady = null;
            onReady = () =>
            {
                YG2.OnGetSDKData -= onReady;
                LoadGameDataInternal(callback);
            };
            YG2.OnGetSDKData += onReady;
            return;
        }

        LoadGameDataInternal(callback);
    }

    private void LoadGameDataInternal(System.Action<Dictionary<string, object>> callback)
    {
        if (YG2.Saves != null && !string.IsNullOrEmpty(YG2.Saves.cloudData))
        {
            try
            {
                var wrapper = JsonUtility.FromJson<SerializableDictionaryWrapper>(YG2.Saves.cloudData);
                if (wrapper != null && wrapper.keys != null)
                {
                    var dict = new Dictionary<string, object>();
                    for (int i = 0; i < wrapper.keys.Count; i++)
                        dict[wrapper.keys[i]] = wrapper.values[i];
                    callback?.Invoke(dict);
                    return;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Load error: {e.Message}");
            }
        }
        callback?.Invoke(null);
    }

    public void OpenAuthDialog()
    {
        if (!YG2.isSDKEnabled) return;
        if (!YG2.Player.auth)
            YG2.OpenAuthDialog();
        else
            Debug.Log("[YandexManager] Already authorized");
    }

    [System.Serializable]
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