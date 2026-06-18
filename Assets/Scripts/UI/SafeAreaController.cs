using UnityEngine;

public class SafeAreaController : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect currentSafeArea = new Rect(0, 0, 0, 0);
    private bool isFullScreenMode = true;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("SafeAreaController requires a RectTransform component!");
            return;
        }
    }

    void OnEnable()
    {
        ApplySafeArea();
    }

    void OnRectTransformDimensionsChange()
    {
        ApplySafeArea();
    }

    void Update()
    {
        if (Screen.fullScreen != isFullScreenMode)
        {
            isFullScreenMode = Screen.fullScreen;
            ApplySafeArea();
        }
    }

    void ApplySafeArea()
    {
        if (rectTransform == null) return;

        Rect safeArea = Screen.safeArea;

        if (Mathf.Approximately(safeArea.x, currentSafeArea.x) &&
            Mathf.Approximately(safeArea.y, currentSafeArea.y) &&
            Mathf.Approximately(safeArea.width, currentSafeArea.width) &&
            Mathf.Approximately(safeArea.height, currentSafeArea.height))
        {
            return;
        }

        currentSafeArea = safeArea;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}