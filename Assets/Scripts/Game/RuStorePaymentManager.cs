using UnityEngine;
using RuStore.CoreClient;
using RuStore.PayClient;
using System.Collections.Generic;

public class RuStorePaymentManager : MonoBehaviour
{
    [Header("ID товаров из консоли RuStore")]
    [SerializeField] private string[] productIds;

    private RuStorePayClient payClient;

    private void Start()
    {
        InitializePayClient();
    }

    private void InitializePayClient()
    {
        payClient = RuStorePayClient.Instance;
        GetProducts();
    }

    /// <summary>
    /// Получение списка товаров
    /// </summary>
    public void GetProducts()
    {
        if (productIds == null || productIds.Length == 0)
        {
            Debug.LogWarning("[RuStore] Нет ID товаров для запроса.");
            return;
        }

        ProductId[] ids = new ProductId[productIds.Length];
        for (int i = 0; i < productIds.Length; i++)
        {
            ids[i] = new ProductId(productIds[i]);
        }

        payClient.GetProducts(
            ids,
            (error) =>
            {
                Debug.LogError($"[RuStore] Ошибка получения товаров: {error.name} - {error.description}");
            },
            (products) =>
            {
                Debug.Log($"[RuStore] Получено {products.Count} товаров");
                foreach (var product in products)
                {
                    // Временно выводим JSON, чтобы увидеть все поля
                    string json = JsonUtility.ToJson(product);
                    Debug.Log($"[RuStore] Товар: {json}");
                    
                    // После того как узнаете правильные имена полей, замените на:
                    // Debug.Log($"[RuStore] Товар: {product.productName} - {product.price} {product.currency}");
                }
            }
        );
    }

    /// <summary>
    /// Покупка товара
    /// </summary>
    public void PurchaseProduct(string productId)
    {
        // Создаём параметры через конструктор (обязательный параметр productId)
        var paramsProduct = new ProductPurchaseParams(
            new ProductId(productId)
        );

        payClient.Purchase(
            paramsProduct,
            PreferredPurchaseType.ONE_STEP,
            (error) =>
            {
                Debug.LogError($"[RuStore] Ошибка покупки: {error.name} - {error.description}");
            },
            (result) =>
            {
                Debug.Log($"[RuStore] Покупка успешна! Товар: {result.productId}");
                OnProductPurchased(result.productId.value);
            }
        );
    }

    /// <summary>
    /// Восстановление покупок (при повторном запуске)
    /// </summary>
    public void RestorePurchases()
    {
        payClient.GetPurchases(
            (error) =>
            {
                Debug.LogError($"[RuStore] Ошибка восстановления: {error.name} - {error.description}");
            },
            (purchases) =>
            {
                Debug.Log($"[RuStore] Восстановлено {purchases.Count} покупок");
                foreach (var purchase in purchases)
                {
                    // Временно выводим JSON, чтобы увидеть все поля
                    string json = JsonUtility.ToJson(purchase);
                    Debug.Log($"[RuStore] Покупка: {json}");
                    
                    // После того как узнаете правильные имена полей, замените:
                    // string productId = purchase.productId.value;
                    // OnProductPurchased(productId);
                }
            }
        );
    }

    private void OnProductPurchased(string productId)
    {
        // TODO: ваша логика выдачи товара
        Debug.Log($"[RuStore] Товар {productId} выдан игроку!");
    }
}