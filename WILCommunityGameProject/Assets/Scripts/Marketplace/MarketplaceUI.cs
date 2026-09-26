using System;
using System.Collections.Generic;
using DG.Tweening;
using Nova;
using UnityEngine;
using UnityEngine.Rendering;

namespace WILCommunityGame
{
    public class MarketplaceUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UIManager inventory;
        [SerializeField] private PlayerStats playerStats;

        [Header("Tabs")]
        [SerializeField] private MarketplaceSellTabUI sellTabUI;
        [SerializeField] private GameObject sellCropsRoot;
        [SerializeField] private MarketplaceUpgradesTabUI upgradesTabUI;
        [SerializeField] private GameObject buyUpgradesRoot;
        
        [Header("Currency")]
        [SerializeField] private TextBlock currencyText;

        public bool IsOpen => uiOpen;
        public UIManager Inventory => inventory;
        public PlayerStats PlayerStats => playerStats;

        private bool uiOpen = false;

        private void Awake()
        {
            transform.localScale = Vector3.zero;
        }

        private void Start()
        {
            inventory.OnInventoryChanged += HandleInventoryChanged;
            playerStats.OnCurrencyChanged += RefreshCurrency;

            ShowSellTab();
            RefreshCurrency(playerStats.Currency);
        }

        private void OnDestroy()
        {
            if (inventory != null)
                inventory.OnInventoryChanged -= HandleInventoryChanged;

            if (playerStats != null)
                playerStats.OnCurrencyChanged -= RefreshCurrency;

            transform.DOKill();
        }

        public void ToggleMarketplaceUI()
        {
            if (uiOpen)
            {
                uiOpen = false;

                transform.DOKill();
                transform.DOScale(0f, 0.35f).SetEase(Ease.OutCubic).SetUpdate(true);
                return;
            }

            uiOpen = true;

            ShowSellTab();
            RefreshCurrency(playerStats.Currency);

            transform.DOKill();
            transform.DOScale(1f, 0.35f).SetEase(Ease.OutCubic).SetUpdate(true);
        }


        public void ShowSellTab()
        {
            buyUpgradesRoot.SetActive(false);
            sellCropsRoot.SetActive(true);

            sellTabUI.RefreshCrops();
        }
        
        public void ShowBuyTab()
        {
            sellCropsRoot.SetActive(false);
            buyUpgradesRoot.SetActive(true);
            
            upgradesTabUI.RefreshTab();
        }
        
        private void HandleInventoryChanged()
        {
            if (uiOpen && sellCropsRoot.activeInHierarchy)
                sellTabUI.RefreshCrops();
        }
        
        private void RefreshCurrency(int amount)
        {
            currencyText.Text = amount.ToString();

            if (uiOpen && sellCropsRoot.activeInHierarchy)
                sellTabUI.RefreshSaleDetails();
            
            if (uiOpen && buyUpgradesRoot.activeInHierarchy)
                upgradesTabUI.RefreshPurchaseStates();
        }

    }
}