using UnityEngine;

// Unity içinde sað týklayarak kolayca yeni eþyalar oluþturmamýzý saðlar
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;      // Eþyanýn adý (Örn: Altýn Bilet)
    public Sprite itemIcon;      // Envanterde görünecek ikon (Kenney paketinden)
    [TextArea]
    public string description;   // Eþyanýn açýklamasý
    public bool isStackable;     // Üst üste birikebilir mi? (Örn: 5 Muz ayný kutuda dursun)
}