using System;
using System.Collections.Generic;
using Nova;
using UnityEngine;

namespace WILCommunityGame
{
    public class MarketplaceSellTabUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MarketplaceUI marketplaceUI;
        
        [Header("Crops")] [SerializeField] private GridView cropsGrid;
        [SerializeField] private TextBlock emptyInventoryText;
        [SerializeField] private int padding = 10;

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
        
        private readonly List<InventoryItem> crops = new();
        private ProduceItemSO selectedCrop;
        private int quantity;
        private bool gridInitialized;
        private UIManager inventory => marketplaceUI.Inventory;
        private PlayerStats playerStats => marketplaceUI.PlayerStats;
        private bool CanInteract => marketplaceUI.IsOpen && isActiveAndEnabled;

        private void Start()
        {
            RefreshCrops();
        }

        private void InitializeGrid()
        {
            if (gridInitialized) return;
            
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
                cropsGrid.RemoveDataBinder
                    <InventoryItem, MarketplaceCropItemVisuals>(BindCrop);

                cropsGrid.RemoveGestureHandler
                    <Gesture.OnClick, MarketplaceCropItemVisuals>(
                        HandleCropClicked);
            }
        }
        
        private void ProvideSlice(int index, GridView grid, ref GridSlice2D slice)
        {
            slice.Layout.AutoSize.Y = AutoSize.Shrink;
            slice.AutoLayout.AutoSpace = true;
            slice.Layout.Padding.Value = padding;
        }
        
        private void BindCrop(Data.OnBind<InventoryItem> evt, MarketplaceCropItemVisuals target, int index)
        {
            target.Bind(evt.UserData, evt.UserData.Produce == selectedCrop);
        }

        private void HandleCropClicked(Gesture.OnClick evt, MarketplaceCropItemVisuals target, int index)
        {
            if (!CanInteract || index < 0 || index >= crops.Count)
                return;

            selectedCrop = crops[index].Produce;
            quantity = 1;

            cropsGrid.Refresh();
            RefreshSaleDetails();
        }
        
        public void RefreshCrops()
        {
            if (!isActiveAndEnabled) return;

            InitializeGrid();
            
            crops.Clear();
            crops.AddRange(inventory.GetProduceItems());

            if (selectedCrop != null &&
                inventory.GetProduceCount(selectedCrop) == 0)
            {
                selectedCrop = null;
            }

            if (emptyInventoryText != null)
            {
                emptyInventoryText.Text = "No crops to sell.";
                emptyInventoryText.gameObject.SetActive(crops.Count == 0);
            }

            cropsGrid.Refresh();
            RefreshSaleDetails();
        }
        
        public void RefreshSaleDetails()
        {
            int available = inventory.GetProduceCount(selectedCrop);
            bool hasCrop = selectedCrop != null && available > 0;

            sellRoot.SetActive(hasCrop);

            if (!hasCrop)
            {
                quantity = 0;
                decreaseButton.enabled = false;
                increaseButton.enabled = false;
                sellButton.enabled = false;
                return;
            }

            quantity = Mathf.Clamp(quantity, 1, available);

            selectedCropIcon.SetImage(selectedCrop.itemDesc.Icon);
            selectedCropCount.Text = available.ToString();
            cropNameText.Text = selectedCrop.itemDesc.Name;
            priceAmountText.Text = selectedCrop.sellPrice.ToString();
            quantityText.Text = quantity.ToString();

            long total = (long)quantity * selectedCrop.sellPrice;
            totalAmountText.Text = total.ToString();

            decreaseButton.enabled = quantity > 1;
            increaseButton.enabled = quantity < available;
            sellButton.enabled = playerStats.CanReceiveCurrency(total);
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
            if (!CanInteract || selectedCrop == null)
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
                playerStats.AddCurrency((int)((long)removed * unitPrice));
        }
        
    }
}