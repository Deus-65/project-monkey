using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using System;

public class InputPromptManager : MonoBehaviour
{
    // Her yerden eriþim için Singleton yapýsý
    public static InputPromptManager Instance { get; private set; }

    public enum DeviceType { Keyboard, Xbox, PlayStation }
    public DeviceType CurrentDevice { get; private set; }

    // Cihaz deðiþtiðinde UI objelerine haber verecek Event
    public event Action<DeviceType> OnDeviceChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Unity'nin global Input sistemine abone oluyoruz
        // Oyuncu herhangi bir tuþa bastýðýnda bize haber verecek
        InputSystem.onActionChange += HandleActionChange;
    }

    private void OnDisable()
    {
        // Script kapandýðýnda aboneliði temizliyoruz
        InputSystem.onActionChange -= HandleActionChange;
    }

    // Bir aksiyon gerçekleþtiðinde (tuþa basýldýðýnda, mouse çevrildiðinde vb.) tetiklenir
    private void HandleActionChange(object obj, InputActionChange change)
    {
        // Sadece aksiyon aktif olarak yapýldýðýnda cihazý kontrol et (Performans için)
        if (change == InputActionChange.ActionPerformed)
        {
            InputAction action = (InputAction)obj;
            InputDevice device = action.activeControl.device;

            UpdateDeviceType(device);
        }
    }

    private void UpdateDeviceType(InputDevice device)
    {
        DeviceType newDevice = CurrentDevice;

        // Gelen cihazýn türüne göre belirleme yapýyoruz
        if (device is Keyboard || device is Mouse)
        {
            newDevice = DeviceType.Keyboard;
        }
        else if (device is Gamepad)
        {
            if (device is DualShockGamepad)
                newDevice = DeviceType.PlayStation;
            else
                newDevice = DeviceType.Xbox; // Varsayýlan Gamepad
        }

        // Eðer cihaz gerçekten deðiþtiyse Event'i fýrlat
        if (CurrentDevice != newDevice)
        {
            CurrentDevice = newDevice;
            OnDeviceChanged?.Invoke(CurrentDevice);
        }
    }
}