using UnityEngine;
using UnityEditor;

public class FillCharacterData : EditorWindow
{
    [MenuItem("Tools/Fill All CharacterData")]
    static void FillAllCharacterData()
    {
        string[] guids = AssetDatabase.FindAssets("t:CharacterData");
        if (guids.Length == 0)
        {
            Debug.LogError("CharacterData не найдены в проекте!");
            return;
        }

        int filledCount = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CharacterData data = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
            if (data == null) continue;

            bool changed = false;

            // Заполняем звуки (если они пустые)
            if (data.eatSounds == null || data.eatSounds.Length == 0)
            {
                data.eatSounds = LoadAllSounds("Eat");
                changed = true;
            }
            if (data.playSounds == null || data.playSounds.Length == 0)
            {
                data.playSounds = LoadAllSounds("Play");
                changed = true;
            }
            if (data.hugSounds == null || data.hugSounds.Length == 0)
            {
                data.hugSounds = LoadAllSounds("Hug");
                changed = true;
            }
            if (data.idleSounds == null || data.idleSounds.Length == 0)
            {
                data.idleSounds = LoadAllSounds("Idle");
                changed = true;
            }

            // Заполняем фразы (если они пустые)
            if (data.eatPhrases == null || data.eatPhrases.Length == 0)
            {
                data.eatPhrases = new string[] { "phrase_eat_1", "phrase_eat_2", "phrase_eat_3", "phrase_eat_4" };
                changed = true;
            }
            if (data.playPhrases == null || data.playPhrases.Length == 0)
            {
                data.playPhrases = new string[] { "phrase_play_1", "phrase_play_2", "phrase_play_3", "phrase_play_4" };
                changed = true;
            }
            if (data.hugPhrases == null || data.hugPhrases.Length == 0)
            {
                data.hugPhrases = new string[] { "phrase_hug_1", "phrase_hug_2", "phrase_hug_3", "phrase_hug_4" };
                changed = true;
            }
            if (data.idlePhrases == null || data.idlePhrases.Length == 0)
            {
                data.idlePhrases = new string[] 
                { 
                    "idle_tip_hug", "idle_tip_feed", "idle_tip_play", "idle_tip_puzzle",
                    "idle_tip_friend", "idle_tip_coins", "idle_tip_shop", "idle_love_1",
                    "idle_love_2", "idle_play_offer"
                };
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(data);
                filledCount++;
                Debug.Log($"Заполнены данные для: {data.name}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Готово! Обработано {filledCount} ассетов.");
    }

    static AudioClip[] LoadAllSounds(string folder)
    {
        string path = $"Assets/Audio/Characters/Grummi/{folder}/";
        string[] files = System.IO.Directory.GetFiles(path, "*.ogg");
        AudioClip[] clips = new AudioClip[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            string assetPath = files[i].Replace("\\", "/");
            clips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
        }
        return clips;
    }
}