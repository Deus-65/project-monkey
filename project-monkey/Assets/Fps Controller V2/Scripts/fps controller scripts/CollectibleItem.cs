using UnityEngine;

public class CollectibleItem : Interactable
{
    [Header("Eþya Bilgileri")]
    // 1. Adýmda oluþturduðumuz Eþya Kartýný buraya sürükleyeceðiz
    public ItemData itemConfig;

    public override void OnFocus()
    {
        // Kursor yönetimine haber gönder
        PointerPromptManager.Instance.ChangePointerState(PointerPromptManager.PointerState.Possible);
    }

    public override void OnLoseFocus()
    {
        PointerPromptManager.Instance.ChangePointerState(PointerPromptManager.PointerState.Empty);
    }

    public override void OnInteract()
    {
        // Eþyayý Envantere Ekle!
        InventoryManager.Instance.AddItem(itemConfig, 1);

        PointerPromptManager.Instance.ChangePointerState(PointerPromptManager.PointerState.Empty);
        Destroy(gameObject);
    }
}