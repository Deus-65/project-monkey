using UnityEngine;
using System;

public class PointerPromptManager : MonoBehaviour
{
    public static PointerPromptManager Instance { get; private set; }

    // Kursor durumlarý: Boþluða bakýyor, Etkileþime geçilebilir, Ýmkansýz
    public enum PointerState { Empty, Possible, Impossible }
    public PointerState CurrentState { get; private set; } = PointerState.Empty;

    // Durum deðiþtiðinde UI'a haber verecek Event (Gözlemci Deseni)
    public event Action<PointerState> OnPointerStateChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Bu metodu FirstPersonController içinden çaðýracaðýz
    public void ChangePointerState(PointerState newState)
    {
        if (CurrentState != newState)
        {
            CurrentState = newState;
            OnPointerStateChanged?.Invoke(CurrentState);
        }
    }
}