using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class PointerIcon : MonoBehaviour
{
    private Image pointerImage;

    [Header("Kursor Görselleri")]
    public Sprite emptyPointer;      // Örn: Sadece ufak bir nokta
    public Sprite possiblePointer;   // Örn: El işareti
    public Sprite impossiblePointer; // Örn: Çarpı işareti

    private void Awake()
    {
        pointerImage = GetComponent<Image>();
    }

    private void Start()
    {
        if (PointerPromptManager.Instance != null)
        {
            PointerPromptManager.Instance.OnPointerStateChanged += UpdatePointerIcon;
            UpdatePointerIcon(PointerPromptManager.Instance.CurrentState);
        }
    }

    private void OnDestroy()
    {
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