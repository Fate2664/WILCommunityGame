using UnityEngine;

namespace WILCommunityGame
{
    [CreateAssetMenu(menuName = "Inventory/CompostItem")]
    public class CompostItemSO : InventoryItemData
    {
        public CompostItem compostItem;

        public bool IsIngredient => compostItem == CompostItem.CropWaste || compostItem == CompostItem.NaturalWaste;
    }
}
