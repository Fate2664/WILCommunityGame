using UnityEngine;

namespace WILCommunityGame
{
    [CreateAssetMenu(menuName = "Inventory/Produce")]
    public class ProduceItemSO : InventoryItemData
    {
        public ProduceType produceType;
        
        [Header("Marketplace")] public int sellPrice = 1;
    }
}
