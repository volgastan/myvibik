using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class PuzzleSetup : EditorWindow
{
    [MenuItem("Tools/Setup Puzzle")]
    static void SetupPuzzle()
    {
        // 1. Создаём папки
        if (!AssetDatabase.IsValidFolder("Assets/Sprites/Puzzle"))
        {
            AssetDatabase.CreateFolder("Assets/Sprites", "Puzzle");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // 2. Создаём спрайты-заглушки (8 цветных + 1 серая)
        CreatePuzzleSprites();

        // 3. Создаём префаб PuzzleSlot
        GameObject prefab = CreatePuzzleSlotPrefab();

        // 4. Назначаем в UIManager
        AssignToUIManager(prefab);

        // 5. Сохраняем сцену
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Debug.Log("Паззл настроен!");
    }

    static void CreatePuzzleSprites()
    {
        Color[] colors = new Color[]
        {
            new Color(1f, 0.4f, 0.4f), // красный
            new Color(0.4f, 0.8f, 0.4f), // зелёный
            new Color(0.4f, 0.6f, 1f), // синий
            new Color(1f, 0.8f, 0.2f), // жёлтый
            new Color(1f, 0.5f, 0.8f), // розовый
            new Color(0.6f, 0.3f, 0.8f), // фиолетовый
            new Color(0.2f, 0.8f, 0.8f), // голубой
            new Color(1f, 0.5f, 0.2f), // оранжевый
        };

        // Создаём 8 цветных частей
        for (int i = 0; i < 8; i++)
        {
            Texture2D tex = new Texture2D(128, 128);
            Color[] pixels = new Color[128 * 128];
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    // Рисуем простую фигуру — например, круг или квадрат с закруглениями
                    float dx = x - 64f;
                    float dy = y - 64f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    pixels[y * 128 + x] = (dist < 55f) ? colors[i] : Color.clear;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            byte[] png = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes($"Assets/Sprites/Puzzle/part_{i}.png", png);
        }

        // Серая заглушка
        Texture2D grayTex = new Texture2D(128, 128);
        Color[] grayPixels = new Color[128 * 128];
        for (int y = 0; y < 128; y++)
        {
            for (int x = 0; x < 128; x++)
            {
                float dx = x - 64f;
                float dy = y - 64f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                grayPixels[y * 128 + x] = (dist < 55f) ? new Color(0.5f, 0.5f, 0.5f, 0.5f) : Color.clear;
            }
        }
        grayTex.SetPixels(grayPixels);
        grayTex.Apply();
        byte[] grayPng = grayTex.EncodeToPNG();
        System.IO.File.WriteAllBytes($"Assets/Sprites/Puzzle/part_empty.png", grayPng);

        AssetDatabase.Refresh();
    }

    static GameObject CreatePuzzleSlotPrefab()
    {
        // Создаём объект ячейки
        GameObject slotObj = new GameObject("PuzzleSlot", typeof(RectTransform));
        RectTransform rt = slotObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 100);

        // Добавляем Image
        Image img = slotObj.AddComponent<Image>();
        img.color = Color.white;
        img.raycastTarget = false;

        // Добавляем StickerSlot
        StickerSlot slotScript = slotObj.AddComponent<StickerSlot>();

        // Загружаем спрайты
        Sprite emptySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Puzzle/part_empty.png");
        // Для цветных частей пока оставляем null — они будут подгружаться по индексу
        // Но в StickerSlot мы можем загружать спрайты динамически

        // Сохраняем префаб
        string path = "Assets/Prefabs/PuzzleSlot.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(slotObj, path);
        DestroyImmediate(slotObj);

        Debug.Log($"Префаб PuzzleSlot создан: {path}");
        return prefab;
    }

    static void AssignToUIManager(GameObject prefab)
    {
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("UIManager не найден на сцене!");
            return;
        }

        // Находим StickerGrid внутри CollectionPanel
        GameObject collectionPanel = GameObject.Find("CollectionPanel");
        if (collectionPanel == null)
        {
            Debug.LogError("CollectionPanel не найден!");
            return;
        }

        Transform gridParent = collectionPanel.transform.Find("StickerGrid");
        if (gridParent == null)
        {
            // Если нет, создаём
            GameObject gridObj = new GameObject("StickerGrid", typeof(RectTransform));
            gridObj.transform.SetParent(collectionPanel.transform);
            RectTransform rt = gridObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, 0.05f);
            rt.anchorMax = new Vector2(0.95f, 0.95f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            // Добавляем GridLayoutGroup
            GridLayoutGroup grid = gridObj.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(100, 100);
            grid.spacing = new Vector2(10, 10);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            gridParent = gridObj.transform;
            Debug.Log("StickerGrid создан внутри CollectionPanel.");
        }

        // Назначаем в UIManager
        SerializedObject so = new SerializedObject(uiManager);
        so.FindProperty("puzzleGridParent").objectReferenceValue = gridParent.gameObject;
        so.FindProperty("puzzleSlotPrefab").objectReferenceValue = prefab;
        so.ApplyModifiedProperties();

        Debug.Log("UIManager обновлён: puzzleGridParent и puzzleSlotPrefab назначены.");
    }
}