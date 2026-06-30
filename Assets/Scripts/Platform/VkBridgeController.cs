using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public class VkBridgeController : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    // ---- DllImports (только для WebGL) ----
    [DllImport("__Internal")] private static extern void _VKWebAppInit();
    [DllImport("__Internal")] private static extern void _VKWebAppShowNativeAds(string adFormat, bool waterfall);
    [DllImport("__Internal")] private static extern void _VKWebAppShowWallPostBox(string text);
    [DllImport("__Internal")] private static extern void _VKWebAppStorageSet(string key, string value);
    [DllImport("__Internal")] private static extern void _VKWebAppStorageGet(string key);
    [DllImport("__Internal")] private static extern void _VKWebAppShowLeaderBoardBox(string value);
    [DllImport("__Internal")] private static extern void _VKWebAppShowInviteBox();
    [DllImport("__Internal")] private static extern void _VKWebAppShowSubscriptionBox(string action, string item, string subscription_id);
    [DllImport("__Internal")] private static extern void _VKWebAppAccelerometerStart(int refresh_rate);
    [DllImport("__Internal")] private static extern void _VKWebAppAccelerometerStop();
    [DllImport("__Internal")] private static extern void getInfoUser(string gameObject, string method);
    [DllImport("__Internal")] private static extern void consoleLoge(string value);
    [DllImport("__Internal")] private static extern void _Send(string name, string Params);
#endif

    // ---- Свойства и события ----
    [SerializeField] private bool _isBridgeReady = false;

    public bool IsBridgeReady => _isBridgeReady;
    public UnityAction OnBridgeReady;

    public UnityAction<VKWebAppShowNativeAdsResultStruct> _actionResultAdsShow;
    public UnityAction<string> _actionStorageGet;
    public UnityAction<AccelerometerData> _actionAccelerometerChange;
    public UnityAction<bool> _actionSubscriptionBox;
    public UnityAction<string> _actionCustomSend;

    // ---- Структуры (без изменений) ----
    [Serializable]
    public struct VKWebAppShowNativeAdsStruct
    {
        public AdFormat ad_format;
        public object use_waterfall;
    }

    public enum AdFormat
    {
        interstitial,
        rewarded
    }

    [Serializable]
    public struct VKWebAppShowNativeAdsResultStruct
    {
        public bool result;
    }

    [Serializable]
    public struct AccelerometerData
    {
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public struct SubscriptionBoxParamsAction
    {
        public SubscriptionBoxAction subscriptionAction;
        public string item;
        public string subscription_id;
    }

    public enum SubscriptionBoxAction
    {
        item,
        subscription
    }

    [Serializable]
    private struct ParamsStruct
    {
        public string[] Key;
        public string[] Body;
    }

    // ---- Методы Unity ----
    private void Start()
    {
        Debug.Log("[VkBridgeController] Ожидаем инициализацию Bridge из JavaScript");
    }

    public void OnBridgeReadyFromJS()
    {
        Debug.Log("[VkBridgeController] Bridge готов (вызов из JS)");
        _isBridgeReady = true;
        OnBridgeReady?.Invoke();
    }

    public void SetVKUserId(string userId)
    {
        Debug.Log($"[VkBridgeController] Получен vk_user_id из JS: {userId}");
        if (!string.IsNullOrEmpty(userId))
        {
            PlayerPrefs.SetString("VK_User_Id", userId);
            PlayerPrefs.Save();
            Debug.Log("[VkBridgeController] vk_user_id сохранён в PlayerPrefs");
        }
        else
        {
            Debug.LogWarning("[VkBridgeController] Получен пустой vk_user_id");
        }
    }

    public void LogMessage(string message)
    {
        Debug.Log("[VK Bridge] " + message);
    }

    // ---- Публичные методы (условно для WebGL, иначе пустые заглушки) ----
    public void VKWebAppInit()
    {
        InitializeBridge();
    }

    public void InitializeBridge()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!_isBridgeReady)
        {
            Debug.Log("[VkBridgeController] Попытка инициализации Bridge...");
            _VKWebAppInit();
        }
        else
        {
            Debug.Log("[VkBridgeController] Bridge уже инициализирован.");
            OnBridgeReady?.Invoke();
        }
#else
        Debug.LogWarning("[VkBridgeController] VK Bridge доступен только в WebGL!");
#endif
    }

    public void VKWebAppShowNativeAds(VKWebAppShowNativeAdsStruct paramsAd, UnityAction<VKWebAppShowNativeAdsResultStruct> actionResult)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!_isBridgeReady) { Debug.LogWarning("[VkBridge] Bridge не готов, реклама не показана"); actionResult?.Invoke(new VKWebAppShowNativeAdsResultStruct { result = false }); return; }
        _actionResultAdsShow = actionResult;
        bool waterfall = paramsAd.use_waterfall != null && bool.Parse(paramsAd.use_waterfall.ToString());
        _VKWebAppShowNativeAds(Enum.GetName(typeof(AdFormat), paramsAd.ad_format), waterfall);
#else
        Debug.LogWarning("[VkBridgeController] VK Bridge доступен только в WebGL!");
        actionResult?.Invoke(new VKWebAppShowNativeAdsResultStruct { result = false });
#endif
    }

    public void VKWebAppShowWallPostBox(string text)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!_isBridgeReady) { Debug.LogWarning("[VkBridge] Bridge не готов, пост не создан"); return; }
        _VKWebAppShowWallPostBox(text);
#else
        Debug.LogWarning("[VkBridgeController] VK Bridge доступен только в WebGL!");
#endif
    }

    public void VKWebAppStorageSet(string key, string value)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!_isBridgeReady) { Debug.LogWarning("[VkBridge] Bridge не готов, данные не сохранены"); return; }
        _VKWebAppStorageSet(key, value);
#else
        Debug.LogWarning("[VkBridgeController] VK Bridge доступен только в WebGL!");
#endif
    }

    public void VKWebAppStorageGet(string key, UnityAction<string> action)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!_isBridgeReady) { Debug.LogWarning("[VkBridge] Bridge не готов, чтение невозможно"); action?.Invoke(null); return; }
        _actionStorageGet = action;
        _VKWebAppStorageGet(key);
#else
        Debug.LogWarning("[VkBridgeController] VK Bridge доступен только в WebGL!");
        action?.Invoke(null);
#endif
    }

    public void VKWebAppAccelerometerStart(int refresh_rate, UnityAction<AccelerometerData> action)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!_isBridgeReady) { Debug.LogWarning("[VkBridge] Bridge не готов, акселерометр не запущен"); return; }
        _actionAccelerometerChange = action;
        _VKWebAppAccelerometerStart(refresh_rate);
#else
        Debug.LogWarning("[VkBridgeController] VK Bridge доступен только в WebGL!");
#endif
    }

    public void VKWebAppAccelerometerStop()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!_isBridgeReady) { Debug.LogWarning("[VkBridge] Bridge не готов, остановка невозможна"); return; }
        _VKWebAppAccelerometerStop();
#else
        Debug.LogWarning("[VkBridgeController] VK Bridge доступен только в WebGL!");
#endif
    }

    public void VKWebAppShowLeaderBoardBox(int value)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!_isBridgeReady) { Debug.LogWarning("[VkBridge] Bridge не готов, таблица не открыта"); return; }
        _VKWebAppShowLeaderBoardBox(value.ToString());
#else
        Debug.LogWarning("[VkBridgeController] VK Bridge доступен только в WebGL!");
#endif
    }

    public void VKWebAppShowInviteBox()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!_isBridgeReady) { Debug.LogWarning("[VkBridge] Bridge не готов, приглашение не открыто"); return; }
        _VKWebAppShowInviteBox();
#else
        Debug.LogWarning("[VkBridgeController] VK Bridge доступен только в WebGL!");
#endif
    }

    public void VKWebAppShowSubscriptionBox(SubscriptionBoxParamsAction actionSubscript, UnityAction<bool> action)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!_isBridgeReady) { Debug.LogWarning("[VkBridge] Bridge не готов, подписка не показана"); action?.Invoke(false); return; }
        _actionSubscriptionBox = action;
        string actionStr = Enum.GetName(typeof(SubscriptionBoxAction), actionSubscript.subscriptionAction);
        if (actionSubscript.subscription_id != null)
            _VKWebAppShowSubscriptionBox(actionStr, null, actionSubscript.subscription_id);
        else
            _VKWebAppShowSubscriptionBox(actionStr, actionSubscript.item, null);
#else
        Debug.LogWarning("[VkBridgeController] VK Bridge доступен только в WebGL!");
        action?.Invoke(false);
#endif
    }

    public void Send(string name, Dictionary<string, string> paramsDict, UnityAction<string> action)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!_isBridgeReady) { Debug.LogWarning("[VkBridge] Bridge не готов, Send не выполнен"); action?.Invoke(null); return; }
        _actionCustomSend = action;
        if (paramsDict != null)
        {
            ParamsStruct ps = new ParamsStruct();
            ps.Key = new string[paramsDict.Count];
            ps.Body = new string[paramsDict.Count];
            int i = 0;
            foreach (var kv in paramsDict)
            {
                ps.Key[i] = kv.Key;
                ps.Body[i] = kv.Value;
                i++;
            }
            _Send(name, JsonUtility.ToJson(ps));
        }
        else
        {
            _Send(name, "none");
        }
#else
        Debug.LogWarning("[VkBridgeController] VK Bridge доступен только в WebGL!");
        action?.Invoke(null);
#endif
    }
}