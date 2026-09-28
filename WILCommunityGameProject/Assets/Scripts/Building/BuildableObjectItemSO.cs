using UnityEngine;

namespace WILCommunityGame
{
    [CreateAssetMenu(menuName = "Inventory/Buildable Object")]
    public class BuildableObjectItemSO : InventoryItemData
    {
        public GameObject prefab;
    }
}