using UnityEngine;

public class DoorInteractable : Interactable
{
    [Header("Kilit Ayarlarý")]
    public ItemData requiredItem;

    [Header("Animasyon Ayarlarý")]
    public Animator doorAnimator;
    public string openTriggerName = "Open";

    private bool isOpened = false;

    public override void OnFocus()
    {
        // Kapý zaten açýlmýþsa kursorü boþ býrak
        if (isOpened) return;

        // Çantada eþya var mý diye baþtan kontrol et
        bool hasItem = requiredItem == null || InventoryManager.Instance.inventoryItems.ContainsKey(requiredItem);

        if (PointerPromptManager.Instance != null)
        {
            if (hasItem)
            {
                // Eþya VARSA kursor "Etkileþime Girilebilir" (Örn: E Tuþu / El ikonu) olsun
                PointerPromptManager.Instance.ChangePointerState(PointerPromptManager.PointerState.Possible);
            }
            else
            {
                // Eþya YOKSA kursor "Ýmkansýz" (Örn: Çarpý veya Kilit ikonu) olsun
                PointerPromptManager.Instance.ChangePointerState(PointerPromptManager.PointerState.Impossible);
            }
        }
    }

    public override void OnInteract()
    {
        if (isOpened) return;

        // Çantayý tekrar kontrol et
        bool hasItem = requiredItem == null || InventoryManager.Instance.inventoryItems.ContainsKey(requiredItem);

        // Eðer eþya yoksa iþlemi iptal et (Zaten kursor Çarpý durumunda)
        if (!hasItem)
        {
            print("KAPI KÝLÝTLÝ! Gerekli eþya: " + requiredItem.itemName);
            return;
        }

        // --- KAPI AÇILMA ÝÞLEMLERÝ ---
        print("KAPI BAÞARIYLA AÇILDI: " + gameObject.name);
        isOpened = true;

        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(openTriggerName);
        }

        // Kapý açýldýktan sonra kursorü ekrandan temizle
        if (PointerPromptManager.Instance != null)
            PointerPromptManager.Instance.ChangePointerState(PointerPromptManager.PointerState.Empty);

        // Iþýn bir daha bu kapýya takýlmasýn diye katmanýný sýfýrla
        gameObject.layer = 0;
    }

    public override void OnLoseFocus()
    {
        if (isOpened) return;

        // Kapýya bakmayý býrakýnca kursorü varsayýlan noktaya (Empty) geri döndür
        if (PointerPromptManager.Instance != null)
            PointerPromptManager.Instance.ChangePointerState(PointerPromptManager.PointerState.Empty);
    }
}