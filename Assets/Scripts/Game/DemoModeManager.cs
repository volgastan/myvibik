using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DemoModeManager : MonoBehaviour
{
    [Header("UI Панель")]
    [SerializeField] private GameObject demoDialogPanel;

    [Header("Текст")]
    [SerializeField] private TMP_Text dialogText;

    [Header("Кнопки")]
    [SerializeField] private Button downloadButton;
    [SerializeField] private Button closeButton;

    [Header("Звук")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip voiceClip;

    [Header("Ссылка на RuStore")]
    [SerializeField] private string rustoreUrl = "https://apps.rustore.ru/app/...";

    // НОВОЕ: событие, которое будет вызываться при закрытии диалога
    public System.Action OnDialogClosed;

    private void Awake()
    {
        if (demoDialogPanel != null)
            demoDialogPanel.SetActive(false);

        if (downloadButton != null)
            downloadButton.onClick.AddListener(OnDownloadClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);
    }

    private void OnDestroy()
    {
        if (downloadButton != null)
            downloadButton.onClick.RemoveAllListeners();
        if (closeButton != null)
            closeButton.onClick.RemoveAllListeners();
    }

    public void ShowDemoDialog()
    {
        if (demoDialogPanel == null)
        {
            Debug.LogError("[DemoModeManager] demoDialogPanel не назначен!");
            return;
        }

        demoDialogPanel.SetActive(true);

        if (audioSource != null && voiceClip != null)
        {
            audioSource.PlayOneShot(voiceClip);
        }

        if (dialogText != null)
        {
            dialogText.text = "Позови маму или папу!\nОни помогут тебе скачать полную версию!";
        }
    }

    public void HideDemoDialog()
    {
        if (demoDialogPanel != null)
            demoDialogPanel.SetActive(false);

        // НОВОЕ: уведомляем подписчиков, что диалог закрыт
        OnDialogClosed?.Invoke();
    }

    private void OnDownloadClicked()
    {
        Debug.Log($"[DemoModeManager] Открываем RuStore: {rustoreUrl}");
        Application.OpenURL(rustoreUrl);
        HideDemoDialog(); // закрываем и вызываем событие
    }

    private void OnCloseClicked()
    {
        HideDemoDialog(); // закрываем и вызываем событие
    }
}