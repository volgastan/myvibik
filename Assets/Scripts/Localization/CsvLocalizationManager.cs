using UnityEngine;
using System.Collections.Generic;

namespace Vibies.Localization
{
    public class CsvLocalizationManager : MonoBehaviour
    {
        public static CsvLocalizationManager Instance { get; private set; }

        [Header("Настройки")]
        [SerializeField] private string csvFileName = "Localization";
        [SerializeField] private string defaultLanguage = "RU";
        [SerializeField] private List<string> availableLanguages = new List<string> { "RU", "EN", "TR" };

        private Dictionary<string, Dictionary<string, string>> localizationData = new Dictionary<string, Dictionary<string, string>>();
        private string currentLanguage = "RU";

        public event System.Action OnLanguageChanged;

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

            currentLanguage = PlayerPrefs.GetString("Language", defaultLanguage);
            if (!availableLanguages.Contains(currentLanguage))
                currentLanguage = defaultLanguage;

            Debug.Log("[Localization] Awake – загрузка локализации...");
            LoadLocalization();
        }

        private void LoadLocalization()
        {
            TextAsset csvFile = Resources.Load<TextAsset>($"Localization/{csvFileName}");
            if (csvFile == null)
            {
                Debug.LogError($"[Localization] CSV файл не найден: Resources/Localization/{csvFileName}.csv");
                return;
            }

            Debug.Log($"[Localization] CSV загружен, длина: {csvFile.text.Length} символов");

            string[] lines = csvFile.text.Split('\n');
            if (lines.Length < 2)
            {
                Debug.LogError("[Localization] CSV файл пуст или имеет неправильный формат.");
                return;
            }

            string[] headers = lines[0].Trim().Split(';');
            if (headers.Length < 2)
            {
                Debug.LogError("[Localization] CSV должен содержать как минимум Key и один язык.");
                return;
            }

            Dictionary<string, int> languageIndexes = new Dictionary<string, int>();
            for (int i = 1; i < headers.Length; i++)
            {
                string lang = headers[i].Trim().ToUpper();
                if (availableLanguages.Contains(lang))
                    languageIndexes[lang] = i;
            }

            if (languageIndexes.Count == 0)
            {
                Debug.LogError("[Localization] Ни один из доступных языков не найден в заголовках CSV.");
                return;
            }

            int count = 0;
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] parts = line.Split(';');
                if (parts.Length < 2) continue;

                string key = parts[0].Trim();
                if (string.IsNullOrEmpty(key)) continue;

                var translations = new Dictionary<string, string>();
                foreach (var lang in availableLanguages)
                {
                    if (languageIndexes.TryGetValue(lang, out int index) && index < parts.Length)
                        translations[lang] = parts[index].Trim();
                    else
                        translations[lang] = key;
                }

                localizationData[key] = translations;
                count++;
            }

            Debug.Log($"[Localization] Загружено {count} ключей");
            OnLanguageChanged?.Invoke();
        }

        public string GetText(string key)
        {
            if (string.IsNullOrEmpty(key))
                return key;

            if (localizationData.TryGetValue(key, out var translations))
            {
                if (translations.TryGetValue(currentLanguage, out string text))
                    return text;
                if (translations.TryGetValue("EN", out string enText))
                    return enText;
                return key;
            }
            Debug.LogWarning($"[Localization] Ключ не найден: {key}");
            return key;
        }

        public string GetText(string key, string language)
        {
            if (string.IsNullOrEmpty(key))
                return key;

            if (localizationData.TryGetValue(key, out var translations))
            {
                if (translations.TryGetValue(language, out string text))
                    return text;
                if (translations.TryGetValue("EN", out string enText))
                    return enText;
                return key;
            }
            Debug.LogWarning($"[Localization] Ключ не найден: {key}");
            return key;
        }

        public void SetLanguage(string language)
        {
            if (!availableLanguages.Contains(language))
                return;

            if (currentLanguage == language)
                return;

            currentLanguage = language;
            PlayerPrefs.SetString("Language", language);
            PlayerPrefs.Save();

            OnLanguageChanged?.Invoke();
        }

        public string GetCurrentLanguage() => currentLanguage;
        public List<string> GetAvailableLanguages() => availableLanguages;
    }
}