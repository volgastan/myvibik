using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class AutoFillCharacterDataEditor : EditorWindow
{
    [MenuItem("Tools/Auto-Fill CharacterData Sounds")]
    public static void ShowWindow()
    {
        GetWindow<AutoFillCharacterDataEditor>("Fill CharacterData");
    }

    private CharacterData targetData;

    private void OnGUI()
    {
        GUILayout.Label("Выберите CharacterData для заполнения звуками", EditorStyles.boldLabel);
        targetData = (CharacterData)EditorGUILayout.ObjectField("CharacterData", targetData, typeof(CharacterData), false);

        if (targetData == null)
        {
            GUILayout.Label("Не выбран объект", EditorStyles.helpBox);
            return;
        }

        if (GUILayout.Button("Заполнить звуки из папок"))
        {
            FillSoundsFromFolders(targetData);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Заполнить фразы ключами (если нужно)"))
        {
            // Можно добавить логику автоматической генерации ключей, но проще сделать вручную
            EditorUtility.DisplayDialog("Подсказка", "Заполните массивы ключей в инспекторе вручную, используя существующие ключи из CSV", "OK");
        }
    }

    private void FillSoundsFromFolders(CharacterData data)
    {
        // Пути относительно Assets
        string basePath = "Assets/Audio/Characters/Grummi/"; // измените, если у вас другой путь
        string[] soundTypes = { "Eat", "Play", "Hug", "Idle", "Happy" };

        // Для каждого типа пытаемся найти файлы и назначить
        Dictionary<string, System.Action<AudioClip[]>> assignMap = new Dictionary<string, System.Action<AudioClip[]>>
        {
            ["Eat"] = clips => data.eatSounds = clips,
            ["Play"] = clips => data.playSounds = clips,
            ["Hug"] = clips => data.hugSounds = clips,
            ["Idle"] = clips => data.idleSounds = clips,
            ["Happy"] = clips => data.happySounds = clips
        };

        foreach (string type in soundTypes)
        {
            string folderPath = Path.Combine(basePath, type);
            if (!Directory.Exists(folderPath))
            {
                Debug.LogWarning($"Папка не найдена: {folderPath}");
                continue;
            }

            // Получаем все .wav .mp3 .ogg файлы в папке
            string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                                       .Where(f => f.EndsWith(".wav") || f.EndsWith(".mp3") || f.EndsWith(".ogg"))
                                       .ToArray();

            List<AudioClip> clips = new List<AudioClip>();
            foreach (string file in files)
            {
                // Конвертируем абсолютный путь в относительный от Assets
                string relativePath = file.Replace("\\", "/");
                if (relativePath.StartsWith("Assets/"))
                {
                    AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(relativePath);
                    if (clip != null)
                        clips.Add(clip);
                    else
                        Debug.LogWarning($"Не удалось загрузить {relativePath}");
                }
            }

            // Назначаем массив
            if (clips.Count > 0 && assignMap.ContainsKey(type))
            {
                assignMap[type].Invoke(clips.ToArray());
                Debug.Log($"Для {type} загружено {clips.Count} звуков");
            }
            else
            {
                // Если нет файлов – оставляем пустой массив (можно также установить null, но лучше оставить пустой)
                if (assignMap.ContainsKey(type))
                    assignMap[type].Invoke(new AudioClip[0]);
            }
        }

        // Помечаем объект как изменённый
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Готово", "Звуки успешно назначены.", "OK");
    }
}