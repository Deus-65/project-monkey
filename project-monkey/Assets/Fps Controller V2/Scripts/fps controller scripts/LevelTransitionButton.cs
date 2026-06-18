using UnityEngine;
using UnityEngine.UI; // Image (Kararma efekti) için gerekli
using UnityEngine.SceneManagement; // Sahne yüklemek için gerekli
using System.Collections; // Coroutine (zamanlayýcý) için gerekli

public class LevelTransitionButton : Interactable
{
    [Header("Ses Ayarlarý")]
    public AudioClip interactSound;

    [Header("Sahne Ayarlarý")]
    public string nextSceneName = "Level2"; // Yüklenecek sahnenin tam adý

    [Header("Buton Animasyonu")]
    public Animator buttonAnimator;
    public string pushTriggerName = "Push"; // Buton basýlma animasyonunun Trigger adý
    [Tooltip("Animasyonun oynayýp bitmesi için beklenecek süre (saniye)")]
    public float animationWaitTime = 1.0f;

    [Header("Kararma (Fade) Ayarlarý")]
    public Image fadeImage; // Ekraný kaplayan siyah UI resmi
    [Tooltip("Ekranýn tamamen siyah olma süresi (saniye)")]
    public float fadeDuration = 1.5f;

    private bool isTriggered = false; // Butona birden fazla basýlmasýný engeller

    public override void OnFocus()
    {
        if (isTriggered) return;

        if (PointerPromptManager.Instance != null)
            PointerPromptManager.Instance.ChangePointerState(PointerPromptManager.PointerState.Possible);
    }

    public override void OnLoseFocus()
    {
        if (isTriggered) return;

        if (PointerPromptManager.Instance != null)
            PointerPromptManager.Instance.ChangePointerState(PointerPromptManager.PointerState.Empty);
    }

    public override void OnInteract()
    {
        // Eðer butona zaten basýldýysa hiçbir þey yapma
        if (isTriggered) return;

        isTriggered = true;
        print("Geçiþ butonuna basýldý! Sekans baþlýyor...");

        if (interactSound != null)
        {
            AudioSource.PlayClipAtPoint(interactSound, transform.position);
        }

        // Kursorü temizle
        if (PointerPromptManager.Instance != null)
            PointerPromptManager.Instance.ChangePointerState(PointerPromptManager.PointerState.Empty);

        // Sinematik geçiþ sürecini (Coroutine) baþlat
        StartCoroutine(TransitionSequence());
    }

    // Adým adým çalýþan zamanlayýcý fonksiyon
    private IEnumerator TransitionSequence()
    {
        // 1. ADIM: Buton Animasyonunu Oynat
        if (buttonAnimator != null)
        {
            buttonAnimator.SetTrigger(pushTriggerName);
        }

        // 2. ADIM: Animasyonun bitmesini bekle
        yield return new WaitForSeconds(animationWaitTime);

        // 3. ADIM: Ekraný yavaþça karart (Fade to Black)
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true); // Önce resmi görünür yap
            Color fadeColor = fadeImage.color;
            float timer = 0f;

            // Zaman geçtikçe resmin Alpha (saydamlýk) deðerini 0'dan 1'e doðru artýr
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                fadeColor.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                fadeImage.color = fadeColor;
                yield return null; // Bir sonraki frame'i (kareyi) bekle
            }
        }

        // Ufak bir es ver (tam siyah ekranda yarým saniye bekle)
        yield return new WaitForSeconds(0.5f);

        // 4. ADIM: Sonraki sahneyi yükle
        SceneManager.LoadScene(nextSceneName);
    }
}
