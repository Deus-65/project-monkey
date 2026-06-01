using UnityEngine;
using UnityEngine.UI;

// Bu script eklendiði objede mutlaka bir Image bileþeni olmasýný zorunlu kýlar
[RequireComponent(typeof(Image))]
public class PromptIcon : MonoBehaviour
{
    private Image targetImage;

    [Header("Bu Ýþlem Ýçin Kenney Ýkonlarý")]
    public Sprite keyboardIcon;
    public Sprite xboxIcon;
    public Sprite playstationIcon;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
    }

    private void Start()
    {
        // Manager'a abone oluyoruz (Cihaz deðiþirse bana haber ver!)
        if (InputPromptManager.Instance != null)
        {
            InputPromptManager.Instance.OnDeviceChanged += UpdateIcon;

            // Oyun baþladýðýnda ikonun doðru ayarlanmasý için bir kez güncelliyoruz
            UpdateIcon(InputPromptManager.Instance.CurrentDevice);
        }
        else
        {
            Debug.LogWarning("Sahnede InputPromptManager bulunamadý!");
        }
    }

    private void OnDestroy()
    {
        // Obje silindiðinde (örneðin UI kapandýðýnda) aboneliði iptal et
        if (InputPromptManager.Instance != null)
        {
            InputPromptManager.Instance.OnDeviceChanged -= UpdateIcon;
        }
    }

    // Manager'dan anons geldiðinde çalýþan metod
    private void UpdateIcon(InputPromptManager.DeviceType device)
    {
        switch (device)
        {
            case InputPromptManager.DeviceType.Keyboard:
                targetImage.sprite = keyboardIcon;
                break;
            case InputPromptManager.DeviceType.Xbox:
                targetImage.sprite = xboxIcon;
                break;
            case InputPromptManager.DeviceType.PlayStation:
                targetImage.sprite = playstationIcon;
                break;
        }

        // Opsiyonel: Ýkonun boyutunu orijinaline uygun hale getir
        targetImage.SetNativeSize();
    }
}