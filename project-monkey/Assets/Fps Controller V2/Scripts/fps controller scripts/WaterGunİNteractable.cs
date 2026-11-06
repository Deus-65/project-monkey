using UnityEngine;

public class WaterGunİNteractable : Interactable
{

     [Header("Kilit Ayarlar�")]
    public ItemData requiredItem;

     [Header("Animasyon Ayarlar�")]
    public Animator doorAnimator;
    public string openTriggerName = "Open";

    private bool isOpened = false;

    [Header("E�ya Bilgileri")]
    // 1. Ad�mda olu�turdu�umuz E�ya Kart�n� buraya s�r�kleyece�iz
    public ItemData itemConfig;

    public GameObject objectToActivate;

    public override void OnFocus()
    {
        // Kursor y�netimine haber g�nder
        PointerPromptManager.Instance.ChangePointerState(PointerPromptManager.PointerState.Possible);
    }

    public override void OnLoseFocus()
    {
        PointerPromptManager.Instance.ChangePointerState(PointerPromptManager.PointerState.Empty);
    }

    public override void OnInteract()
    {       

        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
        }

        PointerPromptManager.Instance.ChangePointerState(PointerPromptManager.PointerState.Empty);
        Destroy(gameObject);

        

     if (isOpened) return;

        // �antay� tekrar kontrol et
        bool hasItem = requiredItem == null || InventoryManager.Instance.inventoryItems.ContainsKey(requiredItem);

        // E�er e�ya yoksa i�lemi iptal et (Zaten kursor �arp� durumunda)
        if (!hasItem)
        {
            print("KAPI K�L�TL�! Gerekli e�ya: " + requiredItem.itemName);
            return;
        }

        // --- KAPI A�ILMA ��LEMLER� ---
        print("KAPI BA�ARIYLA A�ILDI: " + gameObject.name);
        isOpened = true;

        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(openTriggerName);
        }

        // Kap� a��ld�ktan sonra kursor� ekrandan temizle
        if (PointerPromptManager.Instance != null)
            PointerPromptManager.Instance.ChangePointerState(PointerPromptManager.PointerState.Empty);

        // I��n bir daha bu kap�ya tak�lmas�n diye katman�n� s�f�rla
        gameObject.layer = 0;
        

    }
}
