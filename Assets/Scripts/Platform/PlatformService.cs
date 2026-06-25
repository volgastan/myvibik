using UnityEngine;

public static class PlatformService
{
    private static IPlatformService instance;

    public static IPlatformService Instance
    {
        get
        {
            if (instance == null)
            {
#if VK_DEMO
                // В демо-версии используем локальное сохранение
                instance = new LocalPlatformService();
#else
                // В полной версии – облачное с фолбэком
                instance = new CloudPlatformService();
#endif
            }
            return instance;
        }
    }

    public static void Initialize()
    {
        Instance.Login((success) =>
        {
            Debug.Log($"[PlatformService] Инициализация: {(success ? "успешно" : "не удалась")}");
        });
    }
}