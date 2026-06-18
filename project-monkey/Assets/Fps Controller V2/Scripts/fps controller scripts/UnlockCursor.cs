using UnityEngine;

public class UnlockCursor : MonoBehaviour
{
    private void Start()
    {
        // Farenin kilidini açar (ekranda serbestçe dolaþmasýný saðlar)
        Cursor.lockState = CursorLockMode.None;

        // Farenin ikonunu görünür hale getirir
        Cursor.visible = true;
    }
}