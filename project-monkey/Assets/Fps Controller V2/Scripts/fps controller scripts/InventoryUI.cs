using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class InventoryUI : MonoBehaviour
{
    [Header("Arayüz Baðlantýsý")]
    public TextMeshProUGUI inventoryTextDisplay;

    [Header("Input Ayarlarý (Inspector'dan Atanacak)")]
    // Tuþu artýk koddan deðil, Inspector'dan atayacaðýz
    [SerializeField] private InputAction toggleInventoryAction;

    private bool isInventoryOpen = false;

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += UpdateInventoryText;
        }

        inventoryTextDisplay.text = "";
        inventoryTextDisplay.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // Envanter tuþunu aktif et ve dinlemeye baþla
        toggleInventoryAction.Enable();
        toggleInventoryAction.performed += ctx => ToggleInventory();
    }

    private void OnDisable()
    {
        // Script kapanýrken dinlemeyi býrak
        toggleInventoryAction.performed -= ctx => ToggleInventory();
        toggleInventoryAction.Disable();

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= UpdateInventoryText;
        }
    }

    // Tuþa basýldýðýnda çalýþacak metot
    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryTextDisplay.gameObject.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            UpdateInventoryText();
        }
    }

    private void UpdateInventoryText()
    {
        if (!isInventoryOpen) return;

        string displayText = "--- CANTAM ---\n\n";

        foreach (var item in InventoryManager.Instance.inventoryItems)
        {
            string itemName = item.Key.itemName;
            int itemAmount = item.Value;

            if (itemAmount > 1)
            {
                displayText += itemName + " x" + itemAmount + "\n";
            }
            else
            {
                displayText += itemName + "\n";
            }
        }

        if (InventoryManager.Instance.inventoryItems.Count == 0)
        {
            displayText += "Çanta þu an boþ.";
        }

        inventoryTextDisplay.text = displayText;
    }
}