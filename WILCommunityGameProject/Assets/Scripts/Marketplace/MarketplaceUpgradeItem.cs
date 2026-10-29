using UnityEngine;

namespace WILCommunityGame
{
    [CreateAssetMenu(menuName = "Marketplace/Upgrade Item")]
    public class MarketplaceUpgradeItem : ScriptableObject
    {
        public string displayName;
        [TextArea(2,5)]
        public string description;
        public Sprite icon;
        public int price;
        [Tooltip("Allow this item to be bought again, charging its price and applying its purchase effect each time.")]
        public bool canPurchaseMultipleTimes;
    }
}
