using UnityEngine;
using System;

public class PointerPromptManager : MonoBehaviour
{
    public static PointerPromptManager Instance { get; private set; }

    // Kursor durumlarý
    public enum PointerState { Empty, Possible, Impossible }
    public PointerState CurrentState { get; private set; } = PointerState.Empty;

    // Durum deðiþtiðinde UI'a haber verecek Event
    public event Action<PointerState> OnPointerStateChanged;

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

    // Bu metodu FirstPersonController içinden (Raycast çarptýðýnda) çaðýracaðýz
    public void ChangePointerState(PointerState newState)
    {
        // Eðer zaten o durumdaysak boþuna UI'ý güncelleme (Performans için)
        if (CurrentState != newState)
        {
            CurrentState = newState;
            OnPointerStateChanged?.Invoke(CurrentState);
        }
    }
}