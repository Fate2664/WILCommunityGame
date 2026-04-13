using UnityEngine;

namespace WILCommunityGame
{
    [CreateAssetMenu(menuName = "Inventory/Seed")]
    public class SeedItemSO : InventoryItemData
    {
        public SeedType seedType;
        [Min(1)] public int daysToGrow = 1;
        public GameObject seedlingPrefab;
        public GameObject harvestablePrefab;
        public ProduceItemSO produceItem;
    }
}
