using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class CreateDemoUI : EditorWindow
{
    [MenuItem("Tools/Create Demo UI")]
    public static void CreateDemoUIWindow()
    {
        // Находим Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("В сцене нет Canvas! Сначала создайте Canvas.");
            return;
        }

        // Создаём панель
        GameObject panel = new GameObject("DemoDialogPanel");
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.6f);
        panel.SetActive(false); // по умолчанию выключена

        // Текст
        GameObject textObj = new GameObject("DialogText");
        textObj.transform.SetParent(panel.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(400, 120);
        textRect.anchoredPosition = new Vector2(0, 60);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "Позови маму или папу!\nОни помогут тебе скачать полную версию!";
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        // Кнопка "Скачать"
        GameObject downloadBtn = new GameObject("DownloadButton");
        downloadBtn.transform.SetParent(panel.transform, false);
        RectTransform btnRect = downloadBtn.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(200, 50);
        btnRect.anchoredPosition = new Vector2(-120, -80);
        Image btnImage = downloadBtn.AddComponent<Image>();
        btnImage.color = Color.white;
        Button btn = downloadBtn.AddComponent<Button>();
        // Добавляем дочерний текст на кнопку
        GameObject btnTextObj = new GameObject("Text (TMP)");
        btnTextObj.transform.SetParent(downloadBtn.transform, false);
        RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;
        TextMeshProUGUI btnTmp = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnTmp.text = "Скачать в RuStore";
        btnTmp.fontSize = 24;
        btnTmp.alignment = TextAlignmentOptions.Center;
        btnTmp.color = Color.black;

        // Кнопка "Закрыть"
        GameObject closeBtn = new GameObject("CloseButton");
        closeBtn.transform.SetParent(panel.transform, false);
        RectTransform closeRect = closeBtn.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0.5f);
        closeRect.anchorMax = new Vector2(0.5f, 0.5f);
        closeRect.sizeDelta = new Vector2(200, 50);
        closeRect.anchoredPosition = new Vector2(120, -80);
        Image closeImg = closeBtn.AddComponent<Image>();
        closeImg.color = Color.white;
        Button closeBtnComp = closeBtn.AddComponent<Button>();
        GameObject closeTextObj = new GameObject("Text (TMP)");
        closeTextObj.transform.SetParent(closeBtn.transform, false);
        RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;
        TextMeshProUGUI closeTmp = closeTextObj.AddComponent<TextMeshProUGUI>();
        closeTmp.text = "Закрыть";
        closeTmp.fontSize = 24;
        closeTmp.alignment = TextAlignmentOptions.Center;
        closeTmp.color = Color.black;

        // AudioSource
        GameObject audioObj = new GameObject("VoiceSource");
        audioObj.transform.SetParent(panel.transform, false);
        AudioSource audioSrc = audioObj.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;

        // Выделяем созданную панель
        Selection.activeGameObject = panel;
        Debug.Log("Панель демо-диалога создана! Не забудьте назначить ссылки в DemoModeManager и UIManager.");
    }
}