using UnityEngine;
using TMPro;
using System.Collections;

namespace Vibies.Localization
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedTextCsv : MonoBehaviour
    {
        public string key;
        public string fallbackText = "";

        private TextMeshProUGUI textComponent;
        private bool isSubscribed = false;

        private void Awake()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            if (!isSubscribed && CsvLocalizationManager.Instance != null)
            {
                CsvLocalizationManager.Instance.OnLanguageChanged += UpdateText;
                isSubscribed = true;
            }
            UpdateText();
        }

        private void OnDisable()
        {
            if (isSubscribed && CsvLocalizationManager.Instance != null)
            {
                CsvLocalizationManager.Instance.OnLanguageChanged -= UpdateText;
                isSubscribed = false;
            }
        }

        private void UpdateText()
        {
            if (textComponent == null) return;

            if (CsvLocalizationManager.Instance == null)
            {
                StartCoroutine(DelayedUpdate());
                return;
            }

            if (!isSubscribed)
            {
                CsvLocalizationManager.Instance.OnLanguageChanged += UpdateText;
                isSubscribed = true;
            }

            string localizedText = GetLocalizedString(key, fallbackText);
            textComponent.text = localizedText;
        }

        private IEnumerator DelayedUpdate()
        {
            yield return null;
            UpdateText();
        }

        private string GetLocalizedString(string key, string fallback = "")
        {
            if (string.IsNullOrEmpty(key))
                return fallback;

            if (CsvLocalizationManager.Instance != null)
            {
                string text = CsvLocalizationManager.Instance.GetText(key);
                if (!string.IsNullOrEmpty(text) && text != key)
                    return text;
            }
            return fallback;
        }

        public void RefreshText()
        {
            UpdateText();
        }
    }
}