using UnityEngine;

public class VkBridgeHandler : MonoBehaviour
{
    private VkBridgeController controller;

    private void Start()
    {
        controller = GetComponent<VkBridgeController>();
        if (controller == null)
            Debug.LogError("[VkBridgeHandler] VkBridgeController не найден!");
    }

    // Эти методы вызываются из JavaScript через SendMessage
    public void ShowAdsResult(string result)
    {
        if (controller == null) return;
        var res = new VkBridgeController.VKWebAppShowNativeAdsResultStruct();
        res.result = (result == "good");
        controller._actionResultAdsShow?.Invoke(res);
    }

    public void StorageGetResult(string result)
    {
        controller?._actionStorageGet?.Invoke(result);
    }

    public void SubscriptionBoxResult(string res)
    {
        if (controller == null) return;
        bool success = (res != "error") && bool.TryParse(res, out bool val) && val;
        controller._actionSubscriptionBox?.Invoke(success);
    }

    public void AccelerometerChanged(string source)
    {
        if (controller == null) return;
        var data = new VkBridgeController.AccelerometerData();
        if (source != "error")
        {
            string[] cords = source.Split(' ');
            if (cords.Length >= 3)
            {
                float.TryParse(cords[0], out data.x);
                float.TryParse(cords[1], out data.y);
                float.TryParse(cords[2], out data.z);
            }
        }
        controller._actionAccelerometerChange?.Invoke(data);
    }

    public void SendResult(string json)
    {
        controller?._actionCustomSend?.Invoke(json);
    }
}