// ============================================================================
// DEVTOOLS. CS
// ============================================================================
// PURPOSE:      Developer tools for testing
// ============================================================================

using UnityEngine;

public class DevTools : MonoBehaviour
{
    [Header("Dev Tools Settings")]
    [SerializeField] private bool _enableDevTools = true;
    [SerializeField] private int _resourceAddAmount = 100;

    private void Update()
    {
        if (! _enableDevTools) return;

        HandleDevInput();
    }

    private void HandleDevInput()
    {
        // F1-F5: Add resources
        if (Input.GetKeyDown(KeyCode.F1)) AddResource(InventoryManager.RESOURCE_CASH, _resourceAddAmount);
        if (Input.GetKeyDown(KeyCode.F2)) AddResource(InventoryManager.RESOURCE_CORN, _resourceAddAmount);
        if (Input.GetKeyDown(KeyCode.F3)) AddResource(InventoryManager.RESOURCE_MASH, _resourceAddAmount);
        if (Input.GetKeyDown(KeyCode.F4)) AddResource(InventoryManager.RESOURCE_WHISKEY, _resourceAddAmount);
        if (Input.GetKeyDown(KeyCode.F5)) AddResource(InventoryManager. RESOURCE_AGED_WHISKEY, _resourceAddAmount);

        // F6: Sell All Aged Whiskey
        if (Input.GetKeyDown(KeyCode. F6))
        {
            if (SellManager.Instance != null)
            {
                int sold = SellManager.Instance.SellAll();
                Debug.Log($"[DevTools] 💰 Sold {sold} Aged Whiskey.");
            }
        }

        // F7: Save Game
        if (Input.GetKeyDown(KeyCode.F7))
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveGame();
                Debug.Log("[DevTools] 💾 Game saved.");
            }
        }

        // F8: Load Game
        if (Input.GetKeyDown(KeyCode.F8))
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance. LoadGame();
                Debug.Log("[DevTools] 📂 Game loaded.");
            }
        }

        // F9: Log all resources
        if (Input.GetKeyDown(KeyCode.F9)) LogAllResources();

        // F10: Reset resources
        if (Input.GetKeyDown(KeyCode.F10)) ResetAllResources();
    }

    private void AddResource(string resourceName, int amount)
    {
        if (InventoryManager.Instance == null) return;
        InventoryManager.Instance. AddResource(resourceName, amount);
        Debug.Log($"[DevTools] ✅ Added {amount} {resourceName}");
    }

    private void LogAllResources()
    {
        if (InventoryManager. Instance == null) return;

        Debug.Log("========== [DevTools] RESOURCE DUMP ==========");
        Debug.Log($"  Cash: {InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_CASH)}");
        Debug.Log($"  Corn: {InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_CORN)}");
        Debug.Log($"  Mash: {InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_MASH)}");
        Debug.Log($"  Whiskey: {InventoryManager. Instance.GetResource(InventoryManager.RESOURCE_WHISKEY)}");
        Debug.Log($"  Aged Whiskey:  {InventoryManager.Instance. GetResource(InventoryManager. RESOURCE_AGED_WHISKEY)}");
        Debug.Log("===============================================");
    }

    private void ResetAllResources()
    {
        if (InventoryManager.Instance == null) return;

        InventoryManager.Instance.SetResource(InventoryManager. RESOURCE_CASH, 200);
        InventoryManager. Instance.SetResource(InventoryManager.RESOURCE_CORN, 0);
        InventoryManager. Instance.SetResource(InventoryManager.RESOURCE_MASH, 0);
        InventoryManager.Instance.SetResource(InventoryManager.RESOURCE_WHISKEY, 0);
        InventoryManager.Instance. SetResource(InventoryManager. RESOURCE_AGED_WHISKEY, 0);

        Debug.Log("[DevTools] 🔄 Resources reset.");
    }
}