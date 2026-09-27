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
    }
}