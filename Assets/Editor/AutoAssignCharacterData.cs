using UnityEngine;
using UnityEditor;

public class AutoAssignCharacterData : EditorWindow
{
    [MenuItem("Tools/Auto Assign Character Data")]
    static void AssignCharacterData()
    {
        // Находим объект CharacterImage на сцене
        GameObject charImage = GameObject.Find("CharacterImage");
        if (charImage == null)
        {
            Debug.LogError("CharacterImage не найден на сцене!");
            return;
        }

        // Находим компонент VibikController
        VibikController controller = charImage.GetComponent<VibikController>();
        if (controller == null)
        {
            Debug.LogError("VibikController не найден на CharacterImage!");
            return;
        }

        // Ищем CharacterData в папке Assets/Data/Characters/
        string[] guids = AssetDatabase.FindAssets("t:CharacterData");
        if (guids.Length == 0)
        {
            Debug.LogError("CharacterData не найдены в проекте! Создайте хотя бы один через Create → Vibiki → Character Data.");
            return;
        }

        // Берём первый попавшийся (можно изменить логику выбора)
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        CharacterData data = AssetDatabase.LoadAssetAtPath<CharacterData>(path);

        if (data == null)
        {
            Debug.LogError("Не удалось загрузить CharacterData по пути: " + path);
            return;
        }

        // Назначаем в контроллер
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("characterData").objectReferenceValue = data;
        so.ApplyModifiedProperties();

        Debug.Log($"CharacterData '{data.name}' назначен на VibikController.");
    }
}