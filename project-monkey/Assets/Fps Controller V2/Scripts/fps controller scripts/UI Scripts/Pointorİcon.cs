using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class PointerIcon : MonoBehaviour
{
    private Image pointerImage;

    [Header("Kursor Görselleri")]
    public Sprite emptyPointer;      // Boşluğa bakarken (Örn: Nokta)
    public Sprite possiblePointer;   // Etkileşime geçilebilir (Örn: El işareti veya E tuşu)
    public Sprite impossiblePointer; // Kilitli veya etkileşilemez durum (Örn: Çarpı)

    private void Awake()
    {
        pointerImage = GetComponent<Image>();
    }

    private void Start()
    {
        if (PointerPromptManager.Instance != null)
        {
            // Manager'a abone ol
            PointerPromptManager.Instance.OnPointerStateChanged += UpdatePointerIcon;

            // Başlangıçta doğru ikonu koy
            UpdatePointerIcon(PointerPromptManager.Instance.CurrentState);
        }
    }

    private void OnDestroy()
    {
        // Aboneliği iptal et
        if (PointerPromptManager.Instance != null)
        {
            PointerPromptManager.Instance.OnPointerStateChanged -= UpdatePointerIcon;
        }
    }

    private void UpdatePointerIcon(PointerPromptManager.PointerState state)
    {
        switch (state)
        {
            case PointerPromptManager.PointerState.Empty:
                pointerImage.sprite = emptyPointer;
                break;
            case PointerPromptManager.PointerState.Possible:
                pointerImage.sprite = possiblePointer;
                break;
            case PointerPromptManager.PointerState.Impossible:
                pointerImage.sprite = impossiblePointer;
                break;
        }

        pointerImage.SetNativeSize();
    }
}