using UnityEngine;

namespace WILCommunityGame
{
    [CreateAssetMenu(menuName = "Inventory/Tool")]
    public class ToolItemSO : InventoryItemData
    {
        public ToolType toolType;
    }
}
