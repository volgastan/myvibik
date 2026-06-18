using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class UIAdjuster : EditorWindow
{
    [MenuItem("Tools/Fix MyVibik Scene")]
    static void ApplyFixes()
    {
        // 1. Удаляем YandexManager
        GameObject yandex = GameObject.Find("YandexManager");
        if (yandex != null)
        {
            DestroyImmediate(yandex);
            Debug.Log("YandexManager удалён.");
        }

        // 2. Создаём ShopManager и ItemsContainer
        GameObject shopManagerObj = GameObject.Find("ShopManager");
        if (shopManagerObj == null)
        {
            shopManagerObj = new GameObject("ShopManager");
            shopManagerObj.AddComponent<ShopManager>();
        }

        ShopManager shopManager = shopManagerObj.GetComponent<ShopManager>();
        GameObject shopPanel = GameObject.Find("ShopPanel");
        if (shopPanel != null)
        {
            Transform itemsContainer = shopPanel.transform.Find("ItemsContainer");
            if (itemsContainer == null)
            {
                GameObject container = new GameObject("ItemsContainer");
                container.transform.SetParent(shopPanel.transform);
                RectTransform rt = container.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                // Добавим GridLayoutGroup для удобства
                GridLayoutGroup grid = container.AddComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(200, 250);
                grid.spacing = new Vector2(10, 10);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 2;
                itemsContainer = container.transform;
            }

            // Назначаем в ShopManager
            SerializedObject so = new SerializedObject(shopManager);
            so.FindProperty("itemsContainer").objectReferenceValue = itemsContainer.gameObject;
            so.ApplyModifiedProperties();
            Debug.Log("ShopManager настроен.");
        }

        // 3. Назначаем puzzleGridParent в UIManager
        GameObject uiManagerObj = GameObject.Find("UIManager");
        if (uiManagerObj != null)
        {
            UIManager uiManager = uiManagerObj.GetComponent<UIManager>();
            if (uiManager != null)
            {
                GameObject stickerGrid = GameObject.Find("StickerGrid");
                if (stickerGrid != null)
                {
                    SerializedObject so = new SerializedObject(uiManager);
                    so.FindProperty("puzzleGridParent").objectReferenceValue = stickerGrid.transform;
                    so.ApplyModifiedProperties();
                    Debug.Log("puzzleGridParent назначен.");
                }

                // Проверяем наличие префаба слота — используем stickerSlotPrefab
                var slotPrefabField = typeof(UIManager).GetField("stickerSlotPrefab", 
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                if (slotPrefabField != null)
                {
                    GameObject currentPrefab = slotPrefabField.GetValue(uiManager) as GameObject;
                    if (currentPrefab == null)
                    {
                        // Создаём временный префаб для слота
                        GameObject slotPrefab = new GameObject("PuzzleSlotPrefab");
                        Image img = slotPrefab.AddComponent<Image>();
                        img.color = Color.gray;
                        StickerSlot slotScript = slotPrefab.AddComponent<StickerSlot>();
                        // Сохраняем как префаб
                        string path = "Assets/Prefabs/PuzzleSlot.prefab";
                        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                            AssetDatabase.CreateFolder("Assets", "Prefabs");
                        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(slotPrefab, path);
                        DestroyImmediate(slotPrefab);

                        SerializedObject so = new SerializedObject(uiManager);
                        so.FindProperty("stickerSlotPrefab").objectReferenceValue = prefab;
                        so.ApplyModifiedProperties();
                        Debug.Log("Создан префаб PuzzleSlot и назначен в UIManager (stickerSlotPrefab).");
                    }
                }
                else
                {
                    Debug.LogWarning("Поле stickerSlotPrefab не найдено в UIManager. Пропускаем создание префаба.");
                }
            }
        }

        // 4. Активируем кнопки Share и Download и привяжем методы
        GameObject shareBtnObj = GameObject.Find("ShareButton");
        if (shareBtnObj != null)
        {
            shareBtnObj.SetActive(true);
            Button btn = shareBtnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    PlatformService.Instance.ShareInVK("Я обнимаю Вайбика! ❤️", "", "https://vk.com/app/...");
                });
                Debug.Log("ShareButton активирован.");
            }
        }

        GameObject downloadBtnObj = GameObject.Find("DownloadButton");
        if (downloadBtnObj != null)
        {
            downloadBtnObj.SetActive(true);
            Button btn = downloadBtnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    PlatformService.Instance.OpenAppStore("https://apps.rustore.ru/app/...");
                });
                Debug.Log("DownloadButton активирован.");
            }
        }

        // 5. Проверяем GameManager и CharacterController
        GameObject gmObj = GameObject.Find("GameManager");
        if (gmObj != null && gmObj.GetComponent<GameManager>() == null)
            Debug.LogWarning("GameManager не имеет компонента GameManager!");

        GameObject charImg = GameObject.Find("CharacterImage");
        if (charImg != null && charImg.GetComponent<CharacterController>() == null)
            Debug.LogWarning("CharacterImage не имеет CharacterController!");

        // 6. Сохраняем сцену
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
        Debug.Log("Все исправления применены! Сохраните сцену (Ctrl+S).");
    }
}