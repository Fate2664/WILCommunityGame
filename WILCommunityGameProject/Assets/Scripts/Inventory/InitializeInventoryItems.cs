using UnityEngine;
using System.Collections;
using WILCommunityGame;

public class InitializeInventoryItems : MonoBehaviour
{
    //This script gives items to the player at the start
    
    [SerializeField] private Item[] items;

    private IEnumerator Start()
    {
        InventoryPanel inventoryPanel = FindObjectOfType<InventoryPanel>();
        // Wait for one frame to ensure UpgradesPanel.Start() has run
        yield return null;

        foreach (var item in items)
        {
            if (item != null)
            {
                inventoryPanel.AddItemToInventory(item, 1);
            }
            else
            {
                Debug.LogWarning("Upgrade item is null, skipping initialization.");
            }
        }
    }
}
