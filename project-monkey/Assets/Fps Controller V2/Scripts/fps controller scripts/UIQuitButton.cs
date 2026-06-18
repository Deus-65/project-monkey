using UnityEngine;

public class UIQuitButton : MonoBehaviour
{
    // Bu metodu UI Butonunun "On Click" kýsmýna baðlayacaðýz
    public void QuitGame()
    {
        Debug.Log("Oyundan çýkýlýyor...");

        // Oyunu kapatýr (Sadece Build alýndýðýnda çalýþýr)
        Application.Quit();

        // Unity Editörü içinde test ederken Play modunu durdurur
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}