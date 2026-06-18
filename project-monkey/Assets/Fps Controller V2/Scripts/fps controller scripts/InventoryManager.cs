using System.Collections.Generic;
using UnityEngine;
using System;

public class InventoryManager : MonoBehaviour
{
    // Singleton
    public static InventoryManager Instance { get; private set; }

    // Envanterin içindeki eþyalarý tutan liste (Sözlük kullanarak eþyayý ve adetini tutuyoruz)
    public Dictionary<ItemData, int> inventoryItems = new Dictionary<ItemData, int>();

    // Arayüzü güncelleyeceðimiz zaman tetiklenecek olay (Observer mantýðý)
    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Eþya Toplama Metodu
    public void AddItem(ItemData itemData, int amount = 1)
    {
        // Eðer eþya birikebiliyorsa ve zaten envanterde varsa, sayýsýný artýr
        if (itemData.isStackable && inventoryItems.ContainsKey(itemData))
        {
            inventoryItems[itemData] += amount;
        }
        else
        {
            // Yeni bir eþyaysa listeye ekle
            inventoryItems.Add(itemData, amount);
        }

        Debug.Log($"{itemData.itemName} envantere eklendi! Toplam: {inventoryItems[itemData]}");

        // Arayüze "Envanter deðiþti, ekraný güncelle!" emrini gönder
        OnInventoryChanged?.Invoke();
    }
}