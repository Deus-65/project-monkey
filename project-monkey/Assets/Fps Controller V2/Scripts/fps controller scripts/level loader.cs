using UnityEngine;
using UnityEngine.UI; // Image (Kararma efekti) için gerekli
using UnityEngine.SceneManagement; // Sahne yüklemek için gerekli
using System.Collections; // Coroutine (zamanlayıcı) için gerekli

public class levelloader : MonoBehaviour
{
    [Header("Sahne Ayarları")]
    [Tooltip("Geçiş yapılacak sahnenin adını tam olarak buraya yazın.")]
    public string sceneToLoad;

    [Header("Kararma (Fade) Ayarları")]
    public Image fadeImage; // Ekranı kaplayan siyah UI resmi
    [Tooltip("Ekranın tamamen siyah olma süresi (saniye)")]
    public float fadeDuration = 1.5f;

    private bool isTriggered = false; // Alana birden fazla girilip kodun tekrar çalışmasını engeller

    // Karakter (veya bir obje) bu alanın içine girdiğinde tetiklenir
    private void OnTriggerEnter(Collider other)
    {
        // Alana giren objenin etiketi "Player" ise ve geçiş henüz başlamadıysa
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true; // Geçişi kilitle
            Debug.Log("Karakter alana girdi, kararma sekansı başlıyor...");
            
            // Sinematik geçiş sürecini (Coroutine) başlat
            StartCoroutine(TransitionSequence());
        }
    }

    // Adım adım çalışan zamanlayıcı fonksiyon
    private IEnumerator TransitionSequence()
    {
        // 1. ADIM: Ekranı yavaşça karart (Fade to Black)
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true); // Önce resmi görünür yap
            Color fadeColor = fadeImage.color;
            float timer = 0f;

            // Zaman geçtikçe resmin Alpha (saydamlık) değerini 0'dan 1'e doğru artır
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                fadeColor.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                fadeImage.color = fadeColor;
                yield return null; // Bir sonraki frame'i (kareyi) bekle
            }
        }
        else
        {
            Debug.LogWarning("Fade Image atanmamış! Sadece süre bekleniyor.");
            yield return new WaitForSeconds(fadeDuration);
        }

        // Ufak bir es ver (tam siyah ekranda yarım saniye bekle)
        yield return new WaitForSeconds(0.5f);

        // 2. ADIM: Sonraki sahneyi yükle
        SceneManager.LoadScene(sceneToLoad);
    }
}