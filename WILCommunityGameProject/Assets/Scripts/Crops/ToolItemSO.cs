using UnityEngine;

namespace WILCommunityGame
{
    [CreateAssetMenu(menuName = "Inventory/Tool")]
    public class ToolItemSO : InventoryItemData
    {
        public ToolType toolType;

        [Min(1)] public int waterCapacity = 10;

        [Header("Fertilizer")]
        [Min(1)] public int fertilizerDays = 5;
        [Min(1f)] public float fertilizerGrowthMultiplier = 2f;
        public GameObject fertilizerGlowPrefab;
    }
}
