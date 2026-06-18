using UnityEngine;

public static class PlatformService
{
    private static IPlatformService _instance;

    public static IPlatformService Instance
    {
        get
        {
            if (_instance == null)
            {
                #if UNITY_EDITOR
                    _instance = new EditorPlatformService();
                #elif UNITY_WEBGL && !UNITY_EDITOR
                    _instance = new VKPlatformService();
                #elif UNITY_ANDROID && !UNITY_EDITOR
                    _instance = new RuStorePlatformService();
                #else
                    _instance = new EditorPlatformService();
                #endif
            }
            return _instance;
        }
    }
}