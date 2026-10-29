using UnityEngine;

namespace WILCommunityGame
{
    [CreateAssetMenu(menuName = "Inventory/Seed")]
    public class SeedItemSO : InventoryItemData
    {
        public SeedType seedType; 
        public int daysToGrow = 1;
        public int harvestAmount = 10;
        public CompostItemSO cropWasteItem;
        public bool onlyWaterOnce;
        public GameObject sproutPrefab;
        public GameObject seedlingPrefab;
        public GameObject harvestablePrefab;
        public GameObject regrowingPrefab;
        public GameObject deadPrefab;
        public ProduceItemSO produceItem;
        public InformationTipSO informationTip;
    }
}
