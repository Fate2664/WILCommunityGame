using System;
using System.Collections.Generic;
using Nova;
using UnityEngine;

namespace WILCommunityGame
{
    public class MarketplaceSellTabUI : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private MarketplaceUI marketplaceUI;

        [SerializeField] private CommunityUIManager communityUIManager;
        [SerializeField] private TextBlock sellingLockedText;

        [Header("Crops")] [SerializeField] private GridView cropsGrid;
        [SerializeField] private TextBlock emptyInventoryText;
        [SerializeField] private ProduceItemSO[] allCrops = new ProduceItemSO[8];

        [Header("Grid Spacing")] [SerializeField, Min(0f)]
        private float columnSpacing = 30f;

        [SerializeField, Min(0f)] private float rowSpacing = 16f;

        [Header("Selected Crop")] [SerializeField]
        private GameObject sellRoot;

        [SerializeField] private UIBlock2D selectedCropIcon;
        [SerializeField] private TextBlock selectedCropCount;
        [SerializeField] private TextBlock cropNameText;
        [SerializeField] private TextBlock priceAmountText;
        [SerializeField] private TextBlock quantityText;
        [SerializeField] private TextBlock totalAmountText;

        [Header("Controls")] [SerializeField] private Interactable decreaseButton;
        [SerializeField] private Interactable increaseButton;
        [SerializeField] private Interactable sellButton;

        [Header("Currency")] [SerializeField] private TextBlock currencyText;
        
        [Header("Objectives")]
        [SerializeField] private ObjectiveListUI objectiveListUI;
        [SerializeField] private ObjectiveItem sellCropsObjective;

        private readonly List<InventoryItem> crops = new();
        private ProduceItemSO selectedCrop;
        private int quantity;
        private bool gridInitialized;
        private UIManager inventory => marketplaceUI.Inventory;
        private PlayerStats playerStats => marketplaceUI.PlayerStats;
        private bool CanInteract => marketplaceUI.IsOpen && isActiveAndEnabled;
        private bool CanSellCrops => communityUIManager.IsHappinessGreen;

        private void Start()
        {
            communityUIManager.OnHappinessGreenChanged += HandleHappinessChanged;
            RefreshCrops();
        }

        private void InitializeGrid()
        {
            if (gridInitialized) return;

            cropsGrid.PrimaryAxis = Axis.Y;
            cropsGrid.CrossAxis = Axis.X;
            cropsGrid.CrossAxisItemCount = 4;

            cropsGrid.UIBlock.AutoLayout.AutoSpace = false;
            cropsGrid.UIBlock.AutoLayout.Spacing.Value = rowSpacing;

            cropsGrid.AddDataBinder<InventoryItem, MarketplaceCropItemVisuals>(BindCrop);
            cropsGrid.AddGestureHandler<Gesture.OnClick, MarketplaceCropItemVisuals>(HandleCropClicked);

            cropsGrid.SetSliceProvider(ProvideSlice);
            cropsGrid.SetDataSource(crops);

            gridInitialized = true;
        }

        private void OnDestroy()
        {
            if (cropsGrid != null && gridInitialized)
            {
                cropsGrid.RemoveDataBinder<InventoryItem, MarketplaceCropItemVisuals>(BindCrop);
                cropsGrid.RemoveGestureHandler<Gesture.OnClick, MarketplaceCropItemVisuals>(HandleCropClicked);
            }

            communityUIManager.OnHappinessGreenChanged -= HandleHappinessChanged;
        }

        private void HandleHappinessChanged()
        {
            if (isActiveAndEnabled && gridInitialized)
                RefreshSaleDetails();
        }

        private void ProvideSlice(int index, GridView grid, ref GridSlice2D slice)
        {
            slice.Layout.AutoSize.Y = AutoSize.Shrink;
            // Outer padding belongs to CropsRoot; row padding adds to the visible row gap.
            slice.Layout.Padding.Value = 0f;
            slice.AutoLayout.AutoSpace = false;
            slice.AutoLayout.Spacing.Value = columnSpacing;
        }

        private void BindCrop(Data.OnBind<InventoryItem> evt, MarketplaceCropItemVisuals target, int index)
        {
            target.Bind(evt.UserData, evt.UserData.Produce == selectedCrop);
        }

        private void HandleCropClicked(Gesture.OnClick evt, MarketplaceCropItemVisuals target, int index)
        {
            if (!CanInteract || !CanSellCrops || index < 0 || index >= crops.Count)
                return;

            selectedCrop = crops[index].Produce;
            quantity = inventory.GetProduceCount(selectedCrop) > 0 ? 1 : 0;

            cropsGrid.Refresh();
            RefreshSaleDetails();
        }

        public void RefreshCrops()
        {
            if (!isActiveAndEnabled) return;

            InitializeGrid();

            crops.Clear();

            foreach (var crop in allCrops)
            {
                if (crop == null)
                    continue;

                crops.Add(new InventoryItem
                {
                    item = crop,
                    count = inventory.GetProduceCount(crop)
                });
            }

            if (selectedCrop != null && !crops.Exists(entry => entry.Produce == selectedCrop))
                selectedCrop = null;

            if (emptyInventoryText != null)
                emptyInventoryText.gameObject.SetActive(false);

            cropsGrid.Refresh();
            RefreshSaleDetails();
        }

        public void RefreshSaleDetails()
        {
            bool canSell = CanSellCrops;

            cropsGrid.gameObject.SetActive(canSell);

            if (sellingLockedText != null)
            {
                sellingLockedText.gameObject.SetActive(!canSell);
            }

            bool hasSelection = selectedCrop != null;
            sellRoot.SetActive(canSell && hasSelection);

            if (!canSell || !hasSelection)
            {
                quantity = 0;
                decreaseButton.enabled = false;
                increaseButton.enabled = false;
                sellButton.enabled = false;
                return;
            }

            int available = inventory.GetProduceCount(selectedCrop);
            quantity = available > 0 ? Mathf.Clamp(quantity, 1, available) : 0;

            selectedCropIcon.SetImage(selectedCrop.itemDesc.Icon);
            selectedCropCount.Text = available.ToString();
            cropNameText.Text = selectedCrop.itemDesc.Name;
            priceAmountText.Text = selectedCrop.sellPrice.ToString();
            quantityText.Text = quantity.ToString();

            long total = (long)quantity * selectedCrop.sellPrice;
            totalAmountText.Text = total.ToString();

            decreaseButton.enabled = quantity > 1;
            increaseButton.enabled = available > 0 && quantity < available;

            sellButton.enabled = CanSellCrops && available > 0 && quantity > 0 && selectedCrop.sellPrice > 0 &&
                                 playerStats.CanReceiveCurrency(total);
        }

        public void IncreaseQuantity()
        {
            ChangeQuantity(1);
        }

        public void DecreaseQuantity()
        {
            ChangeQuantity(-1);
        }

        private void ChangeQuantity(int change)
        {
            if (!CanInteract || !CanSellCrops || selectedCrop == null)
                return;

            int available = inventory.GetProduceCount(selectedCrop);

            if (available <= 0)
            {
                RefreshCrops();
                return;
            }

            quantity = Mathf.Clamp(quantity + change, 1, available);
            RefreshSaleDetails();
        }

        public void SellSelectedCrop()
        {
            if (!CanInteract || selectedCrop == null || quantity <= 0)
                return;

            if (!CanSellCrops)
            {
                RefreshSaleDetails();
                return;
            }

            ProduceItemSO crop = selectedCrop;
            int unitPrice = crop.sellPrice;
            int available = inventory.GetProduceCount(crop);

            if (unitPrice <= 0 || available < quantity)
            {
                RefreshCrops();
                return;
            }

            long total = (long)quantity * unitPrice;

            if (!playerStats.CanReceiveCurrency(total))
                return;

            int removed = inventory.RemoveProduce(crop, quantity);

            if (removed > 0)
            {
                playerStats.AddCurrency((int)((long)removed * unitPrice));
                objectiveListUI?.CompleteItem(sellCropsObjective);
            }
        }
    }
}
