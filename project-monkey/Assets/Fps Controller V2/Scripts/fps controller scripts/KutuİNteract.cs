using UnityEngine;

public class KutuİNteract : Interactable
{
   [Header("Kilit Ayarları")]
    public ItemData requiredItem;

    [Header("Animasyon Ayarları")]
    public Animator doorAnimator;
    public string openTriggerName = "Open";

    [Header("2. Animasyon Ayarları")]
    public Animator secondAnimator; // İkinci objenin Animator'ü (örneğin Kutu)
    public string secondTriggerName = "kutu"; // İkinci animasyonun Trigger adı

    private bool isOpened = false;

    public override void OnFocus()
    {
        // Kapı zaten açılmışsa kursorü boş bırak
        if (isOpened) return;

        // Çantada eşya var mı diye baştan kontrol et
        bool hasItem = requiredItem == null || InventoryManager.Instance.inventoryItems.ContainsKey(requiredItem);

        if (PointerPromptManager.Instance != null)
        {
            if (hasItem)
            {
                // Eşya VARSA kursor "Etkileşime Girilebilir" (Örn: E Tuşu / El ikonu) olsun
                PointerPromptManager.Instance.ChangePointerState(PointerPromptManager.PointerState.Possible);
            }
            else
            {
                // Eşya YOKSA kursor "İmkansız" (Örn: Çarpı veya Kilit ikonu) olsun
                PointerPromptManager.Instance.ChangePointerState(PointerPromptManager.PointerState.Impossible);
            }
        }
    }

    public override void OnInteract()
    {
        if (isOpened) return;

        // Çantayı tekrar kontrol et
        bool hasItem = requiredItem == null || InventoryManager.Instance.inventoryItems.ContainsKey(requiredItem);

        // Eğer eşya yoksa işlemi iptal et (Zaten kursor çarpı durumunda)
        if (!hasItem)
        {
            print("KAPI KİLİTLİ! Gerekli eşya: " + requiredItem.itemName);
            return;
        }

        // --- KAPI AÇILMA İŞLEMLERİ ---
        print("KAPI BAŞARIYLA AÇILDI: " + gameObject.name);
        isOpened = true;

        // 1. Animasyonu tetikle (Örneğin Kapı)
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(openTriggerName);
        }

        // 2. Animasyonu tetikle (Örneğin Kutu)
        if (secondAnimator != null)
        {
            secondAnimator.SetTrigger(secondTriggerName);
        }

        // Kapı açıldıktan sonra kursorü ekrandan temizle
        if (PointerPromptManager.Instance != null)
            PointerPromptManager.Instance.ChangePointerState(PointerPromptManager.PointerState.Empty);

        // Işın bir daha bu kapıya takılmasın diye katmanını sıfırla
        gameObject.layer = 0;
    }

    public override void OnLoseFocus()
    {
        if (isOpened) return;

        // Kapıya bakmayı bırakınca kursorü varsayılan noktaya (Empty) geri döndür
        if (PointerPromptManager.Instance != null)
            PointerPromptManager.Instance.ChangePointerState(PointerPromptManager.PointerState.Empty);
    }
}
