using System;
using UnityEngine;

namespace WILCommunityGame
{
    [Serializable]
    public class TradeItem
    {
        public InventoryItemData Item;
        [Min(1)] public int Amount = 1;
    }

    [CreateAssetMenu(menuName = "Community/Trade")]
    public class CommunityTradeSO : ScriptableObject
    {
        public TradeItem[] Give;
        public TradeItem[] Receive;

        public bool IsValid => AreItemsValid(Give) && AreItemsValid(Receive);

        private bool AreItemsValid(TradeItem[] items)
        {
            return items != null && items.Length > 0 &&
                   Array.TrueForAll(items, entry => entry != null && entry.Item != null && entry.Amount > 0);
        }
    }
}
