using UnityEngine;
using System.Collections.Generic;
using WILCommunityGame;

[CreateAssetMenu(menuName = "Inventory/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField]
    private List<Item> items = new ();

    public List<InventoryItem> GetEmptyItems(int count)
    {
        List<InventoryItem> toRet = new List<InventoryItem>(count);
        for (int i = 0; i < count; i++)
        {
            toRet.Add(new InventoryItem());
        }
        return toRet;
    }
}
