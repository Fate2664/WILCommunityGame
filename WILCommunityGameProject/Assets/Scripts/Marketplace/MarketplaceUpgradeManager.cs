using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WILCommunityGame
{
    public enum UpgradePurchaseState
    {
        Available,
        Owned,
        Locked,
        InsufficientFunds
    }

    [Serializable]
    public class MarketplaceUpgradeBinding
    {
        public MarketplaceUpgradeItem upgrade;
        public UnityEvent onPurchased = new();
    }
    
    public class MarketplaceUpgradeManager : MonoBehaviour
    {
        [SerializeField] private PlayerStats playerStats;
        
        [Header("Upgrade Effects")]
        [SerializeField] private List<MarketplaceUpgradeBinding> bindings = new();
        
        private readonly HashSet<MarketplaceUpgradeItem> purchased = new();
        private bool purchaseInProgress;

        public event Action OnUpgradesChanged;
        public bool HasUpgrade(MarketplaceUpgradeItem upgrade) => purchased.Contains(upgrade);

        private MarketplaceUpgradeBinding GetBinding(MarketplaceUpgradeItem upgrade)
        {
            return bindings.Find(binding => binding.upgrade == upgrade);
        }

        public UpgradePurchaseState GetPurchaseState(MarketplaceUpgradeItem upgrade)
        {
            if (HasUpgrade(upgrade))
                return UpgradePurchaseState.Owned;
            
            //TODO If they have not passed tutorial
            /*
            if (!playerStats.hasCompletedTutorial)
            {
                return UpgradePurchaseState.Locked;
            }
            */
            
            if (playerStats.Currency < upgrade.price)
                return UpgradePurchaseState.InsufficientFunds;

            return UpgradePurchaseState.Available;
        }

        public bool TryPurchase(MarketplaceUpgradeItem upgrade)
        {
            if (purchaseInProgress || GetPurchaseState(upgrade) != UpgradePurchaseState.Available)
                return false;
            
            MarketplaceUpgradeBinding binding = GetBinding(upgrade);
            purchaseInProgress =  true;

            try
            {
                purchased.Add(upgrade);
                if (!playerStats.TrySpendCurrency(upgrade.price))
                {
                    purchased.Remove(upgrade);
                    return false;
                }

                binding.onPurchased.Invoke();
                return true;
            }
            finally
            {
                purchaseInProgress = false;
                OnUpgradesChanged?.Invoke();
            }
        }
    }
}