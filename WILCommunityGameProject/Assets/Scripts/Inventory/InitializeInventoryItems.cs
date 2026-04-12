using UnityEngine;
using System.Collections;
using WILCommunityGame;

public class InitializeInventoryItems : MonoBehaviour
{
    //This script gives items to the player at the start
    
    [SerializeField] private Item[] items;

    private IEnumerator Start()
    {
        UIManager uiManager = FindObjectOfType<UIManager>();
        // Wait for one frame to ensure UpgradesPanel.Start() has run
        yield return null;

        foreach (var item in items)
        {
            if (item != null)
            {
                uiManager.AddItemToInventory(item, 1);
            }
        }
    }
}
