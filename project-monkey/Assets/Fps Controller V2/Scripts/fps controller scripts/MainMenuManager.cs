using UnityEngine;
using UnityEngine.SceneManagement; // Sahneleri yüklemek için gerekli olan kütüphane

public class MainMenuManager : MonoBehaviour
{
    [Header("Sahne Ayarlarý")]
    [Tooltip("Play butonuna basýldýðýnda açýlacak sahnenin tam adý (birebir ayný yazýlmalý)")]
    public string gameSceneName = "GameScene";

    // Bu metodu Unity arayüzündeki Play butonunun "On Click()" kýsmýna baðlayacaðýz
    public void PlayGame()
    {
        print("Oyun sahnesi yükleniyor: " + gameSceneName);

        // Ýsmi verilen sahneyi yükle
        SceneManager.LoadScene(gameSceneName);
    }

    // Elinin altýnda hazýr dursun diye Çýkýþ butonunu da ekledim
    public void QuitGame()
    {
        print("Oyundan çýkýlýyor...");
        Application.Quit(); // Derlenmiþ (Build alýnmýþ) oyunu kapatýr
    }
}