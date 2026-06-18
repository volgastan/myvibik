using UnityEngine;
using System.Collections;

namespace Vibies.Localization
{
    /// <summary>
    /// Базовый класс для скриптов, которые используют локализацию.
    /// Гарантирует, что OnLocalizationReady() будет вызван после готовности CsvLocalizationManager.
    /// </summary>
    public abstract class LocalizedBehaviourCsv : MonoBehaviour
    {
        private bool isLocalizationReady = false;

        protected virtual void Awake() { }

        protected virtual IEnumerator Start()
        {
            // Ждём, пока менеджер локализации появится и загрузит данные
            while (CsvLocalizationManager.Instance == null)
                yield return null;

            // Даём один кадр, чтобы менеджер успел загрузить данные
            yield return null;

            isLocalizationReady = true;
            OnLocalizationReady();
        }

        protected virtual void OnEnable()
        {
            if (CsvLocalizationManager.Instance != null && isLocalizationReady)
                OnLocalizationReady();
            else
                StartCoroutine(WaitForManagerAndCall());
        }

        private IEnumerator WaitForManagerAndCall()
        {
            while (CsvLocalizationManager.Instance == null || !isLocalizationReady)
                yield return null;
            yield return null; // один кадр для стабильности
            OnLocalizationReady();
        }

        protected virtual void OnDisable() { }

        /// <summary>
        /// Вызывается, когда локализация полностью готова.
        /// Здесь следует обновлять все тексты, зависящие от локализации.
        /// </summary>
        protected abstract void OnLocalizationReady();

        /// <summary>
        /// Безопасное получение локализованной строки с fallback
        /// </summary>
        protected virtual string GetLocalizedString(string key, string fallback = "")
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
    }
}