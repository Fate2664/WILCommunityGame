using System;
using System.Collections.Generic;
using Nova;
using UnityEngine;

namespace WILCommunityGame
{
    public class MarketplaceUpgradesTabUI : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private MarketplaceUI marketplaceUI;
        [SerializeField] private MarketplaceUpgradeManager upgradeManager;

        [Header("Lists")] 
        [SerializeField] private ListView categoryList;
        [SerializeField] private ListView upgradeList;

        [Header("Catalogue")] 
        [SerializeField] private List<MarketplaceUpgradeCategory> categories = new();
        
        private readonly List<MarketplaceUpgradeCategory> displayedCategories = new ();
        private readonly List<MarketplaceUpgradeItem> displayedUpgrades = new ();
        private MarketplaceUpgradeCategory selectedCategory;
        private bool initialized;
        private bool CanInteract => marketplaceUI.IsOpen && isActiveAndEnabled;

        private void Start()
        {
            RefreshTab();
        }

        private void InitializeLists()
        {
            if (initialized) return;
            
            categoryList.AddDataBinder<MarketplaceUpgradeCategory, UpgradeCategoryItemVisuals>(BindCategory);
            categoryList.AddGestureHandler<Gesture.OnClick, UpgradeCategoryItemVisuals>(HandleCategoryClicked);
            
            upgradeList.AddDataBinder<MarketplaceUpgradeItem, UpgradeItemVisuals>(BindUpgrade);
            upgradeList.AddGestureHandler<Gesture.OnClick, UpgradeItemVisuals>(HandleBuyClicked);
            
            categoryList.SetDataSource(displayedCategories);
            upgradeList.SetDataSource(displayedUpgrades);

            upgradeManager.OnUpgradesChanged += RefreshPurchaseStates;
            
            initialized = true;
        }

        private void BindUpgrade(Data.OnBind<MarketplaceUpgradeItem> evt, UpgradeItemVisuals target, int index)
        {
            target.Bind(evt.UserData, upgradeManager.GetPurchaseState(evt.UserData));
        }

        private void BindCategory(Data.OnBind<MarketplaceUpgradeCategory> evt, UpgradeCategoryItemVisuals target, int index)
        {
            target.Bind(evt.UserData, evt.UserData == selectedCategory);
        }
        
        private void HandleCategoryClicked(Gesture.OnClick evt, UpgradeCategoryItemVisuals target, int index)
        {
            if (!CanInteract || index >= displayedCategories.Count)
                return;
            
            selectedCategory = displayedCategories[index];
            RefreshTab();
            
            if (displayedUpgrades.Count > 0)
                upgradeList.JumpToIndex(0);
        }

        private void HandleBuyClicked(Gesture.OnClick evt, UpgradeItemVisuals target, int index)
        {
            if (!CanInteract || index >= displayedUpgrades.Count)
                return;
            
            upgradeManager.TryPurchase(displayedUpgrades[index]);
            RefreshPurchaseStates();
        }

        public void RefreshTab()
        {
            if (!isActiveAndEnabled) return;
            
            InitializeLists();
            displayedCategories.Clear();

            foreach (var category in categories)
            {
                if (category != null && !displayedCategories.Contains(category))
                    displayedCategories.Add(category);
            }

            if (selectedCategory == null || !displayedCategories.Contains(selectedCategory))
                selectedCategory = displayedCategories.Count > 0 ? displayedCategories[0] : null;
            
            displayedUpgrades.Clear();

            if (selectedCategory != null)
            {
                foreach (var upgrade in selectedCategory.upgrades)
                {
                    if (upgrade != null && !displayedUpgrades.Contains(upgrade))
                        displayedUpgrades.Add(upgrade);
                }
            }
            
            categoryList.Refresh();
            upgradeList.Refresh();
        }

        public void RefreshPurchaseStates()
        {
            if (!initialized || !isActiveAndEnabled) return;
            
            upgradeList.Refresh();
        }

        private void OnDestroy()
        {
            if (!initialized)
                return;

            if (upgradeManager != null)
            {
                upgradeManager.OnUpgradesChanged -= RefreshPurchaseStates;
            }

            if (categoryList != null)
            {
                categoryList.RemoveDataBinder<MarketplaceUpgradeCategory, UpgradeCategoryItemVisuals>(BindCategory);
                categoryList.RemoveGestureHandler<Gesture.OnClick, UpgradeCategoryItemVisuals>(HandleCategoryClicked);
            }

            if (upgradeList != null)
            {
                upgradeList.RemoveDataBinder<MarketplaceUpgradeItem, UpgradeItemVisuals>(BindUpgrade);
                upgradeList.RemoveGestureHandler<Gesture.OnClick, UpgradeItemVisuals>(HandleBuyClicked);
            }
        }
    }
}