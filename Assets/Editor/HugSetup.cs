using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class HugSetup : EditorWindow
{
    [MenuItem("Tools/Setup Hug")]
    static void SetupHug()
    {
        GameObject charImage = GameObject.Find("CharacterImage");
        if (charImage == null)
        {
            Debug.LogError("CharacterImage не найден на сцене!");
            return;
        }

        // 1. Создаём круговой прогресс-бар
        GameObject progressObj = charImage.transform.Find("ProgressCircle")?.gameObject;
        if (progressObj == null)
        {
            progressObj = new GameObject("ProgressCircle", typeof(RectTransform));
            progressObj.transform.SetParent(charImage.transform);
            RectTransform rt = progressObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(200, 200);

            // Создаём круглый спрайт
            Texture2D tex = new Texture2D(128, 128);
            Color[] colors = new Color[128 * 128];
            Vector2 center = new Vector2(64, 64);
            float radius = 60f;
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    colors[y * 128 + x] = (dist <= radius) ? Color.white : Color.clear;
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            byte[] png = tex.EncodeToPNG();
            string path = "Assets/Sprites/Circle.png";
            if (!AssetDatabase.IsValidFolder("Assets/Sprites"))
                AssetDatabase.CreateFolder("Assets", "Sprites");
            System.IO.File.WriteAllBytes(path, png);
            AssetDatabase.Refresh();
            Sprite circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            Image img = progressObj.AddComponent<Image>();
            img.sprite = circleSprite;
            img.color = new Color(0.5f, 0.8f, 1f, 0.5f);
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Radial360;
            img.fillOrigin = (int)Image.Origin360.Bottom;
            img.fillAmount = 0f;
            img.raycastTarget = false;

            progressObj.SetActive(false);
            Debug.Log("ProgressCircle создан.");
        }

        // 2. Добавляем или обновляем HugHandler
        HugHandler handler = charImage.GetComponent<HugHandler>();
        if (handler == null)
        {
            handler = charImage.AddComponent<HugHandler>();
        }

        SerializedObject so = new SerializedObject(handler);
        so.FindProperty("progressCircle").objectReferenceValue = progressObj.GetComponent<Image>();
        so.FindProperty("vibikController").objectReferenceValue = charImage.GetComponent<VibikController>();

        AudioSource audioSrc = charImage.GetComponent<AudioSource>();
        if (audioSrc == null)
        {
            audioSrc = charImage.AddComponent<AudioSource>();
        }
        so.FindProperty("audioSource").objectReferenceValue = audioSrc;

        AudioClip heartbeat = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/heartbeat.wav");
        if (heartbeat != null)
        {
            so.FindProperty("heartbeatSound").objectReferenceValue = heartbeat;
        }
        else
        {
            Debug.LogWarning("heartbeat.wav не найден в Assets/Audio/SFX/. Добавьте звук вручную.");
        }

        so.ApplyModifiedProperties();

        // 3. Сохраняем сцену
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Debug.Log("Объятие полностью настроено! Круговой прогресс-бар будет масштабироваться до размера экрана.");
    }
}