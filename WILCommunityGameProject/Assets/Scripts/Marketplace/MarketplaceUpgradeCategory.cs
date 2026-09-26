using System.Collections.Generic;
using UnityEngine;

namespace WILCommunityGame
{
    [CreateAssetMenu(menuName = "Marketplace/Upgrade Category")]
    public class MarketplaceUpgradeCategory : ScriptableObject
    {
        public string displayName;
        public Sprite icon;
        public List<MarketplaceUpgradeItem> upgrades = new();
    }
}