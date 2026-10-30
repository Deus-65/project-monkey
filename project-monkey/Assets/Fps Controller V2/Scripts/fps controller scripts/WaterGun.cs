using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public struct AudioSegment
{
    public float startTime; // Sesin başlayacağı saniye
    public float duration;  // Sesin çalma süresi
}

public class WaterGun : MonoBehaviour
{
    [Header("Gun Settings")]
    [SerializeField] private float fireRange = 50f;
    [SerializeField] private float fireRate = 0.2f;
    private float nextFireTime = 0f;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private ParticleSystem waterShootParticle;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip singleShootClip; // Tek parça uzun ses dosyan
    [SerializeField] private AudioSegment[] soundSegments; // Zaman aralıkları listesi
    [SerializeField] private float audioVolume = 0.5f;

    [Header("Input")]
    [SerializeField] private InputAction shootAction;

    private void OnEnable()
    {
        shootAction.Enable();
        shootAction.performed += ctx => TryShoot();
    }

    private void OnDisable()
    {
        shootAction.performed -= ctx => TryShoot();
        shootAction.Disable();
    }

    private void TryShoot()
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + fireRate;

        Shoot();
    }

    private void Shoot()
    {
        // Görsel efekti oynat
        if (waterShootParticle != null) waterShootParticle.Play();

        // Rastgele sesi oynat
        PlayRandomShootSound();

        // Lazer/Blaster mantığı: Ekranın tam ortasından ileriye Raycast yolla
        Vector3 rayOrigin = playerCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        
        if (Physics.Raycast(rayOrigin, playerCamera.transform.forward, out RaycastHit hit, fireRange))
        {
            Interactable hitInteractable = hit.collider.GetComponent<Interactable>();
            
            if (hitInteractable != null && hit.collider.CompareTag("CarnivalDuck"))
            {
                hitInteractable.OnInteract(); 
            }
        }
    }

    private void PlayRandomShootSound()
    {
        if (soundSegments.Length == 0 || singleShootClip == null) return;

        // Listeden rastgele bir zaman aralığı seç
        AudioSegment randomSegment = soundSegments[Random.Range(0, soundSegments.Length)];

        // Atış anında üst üste binen sesler için geçici bir AudioSource oluştur
        AudioSource tempSource = gameObject.AddComponent<AudioSource>();
        tempSource.clip = singleShootClip;
        tempSource.volume = audioVolume;
        tempSource.spatialBlend = 1f; // 3D ses istiyorsan 1 yap, her yerden duyulsun diyorsan 0 yap

        // Belirlenen saniyeden başlat ve oynat
        tempSource.time = randomSegment.startTime;
        tempSource.Play();

        // Ses süresi dolduğunda (duration) geçici bileşeni hafızadan sil
        Destroy(tempSource, randomSegment.duration);
    }
}