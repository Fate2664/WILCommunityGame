using UnityEngine;

namespace WILCommunityGame
{
    [CreateAssetMenu(menuName = "Inventory/Seed")]
    public class SeedItemSO : InventoryItemData
    {
        public SeedType seedType; 
        public int daysToGrow = 1;
        public int harvestAmount = 10;
        public GameObject seedlingPrefab;
        public GameObject harvestablePrefab;
        public ProduceItemSO produceItem;
    }
}
