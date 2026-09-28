using System;
using System.Collections.Generic;
using DG.Tweening;
using Nova;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WILCommunityGame
{
    public class StorageSiloUI : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private GameObject panelRoot;

        [SerializeField] private UIManager inventory;
        [SerializeField] private BuildPlacer buildPlacer;

        [Header("Grids")] [SerializeField] private GridView playerCropsGrid;
        [SerializeField] private GridView siloInventoryGrid;
        [SerializeField] private float columnSpacing = 15f;
        [SerializeField] private float rowSpacing = 15f;

        [Header("Drag Preview")] [SerializeField]
        private UIBlock2D dragIcon;

        [SerializeField] private TextBlock dragCount;

        public bool IsOpen => uiOpen;
        public StorageSiloManager ActiveSilo { get; private set; }

        private readonly List<InventoryItem> playerItems = new();
        private readonly List<InventoryItem> siloItems = new();
        private readonly List<UIBlockHit> dropHits = new();

        private PlayerController player;
        private PlayerInteractionDetector interactionDetector;

        private bool uiOpen;
        private bool inputReady;
        private bool refreshPending;

        private ProduceItemSO draggedProduce;
        private int draggedAmount;
        private uint dragControlID;
        private bool dragStarted;
        private bool draggingFromSilo;
        private int dragSourceIndex = -1;

        private bool CanInteract => isActiveAndEnabled && uiOpen && inputReady && ActiveSilo != null &&
                                    ActiveSilo.isActiveAndEnabled;

        private void Awake()
        {
            if (panelRoot == null)
                panelRoot = gameObject;

            for (int i = 0; i < StorageSiloManager.SlotCount; i++)
            {
                playerItems.Add(new InventoryItem());
                siloItems.Add(new InventoryItem());
            }

            ConfigureGrid(playerCropsGrid);
            ConfigureGrid(siloInventoryGrid);

            playerCropsGrid.AddDataBinder<InventoryItem, StorageSiloStorageSlotItemVisuals>(BindPlayerItem);
            siloInventoryGrid.AddDataBinder<InventoryItem, StorageSiloStorageSlotItemVisuals>(BindSiloItem);

            RegisterDragHandlers(playerCropsGrid);
            RegisterDragHandlers(siloInventoryGrid);

            playerCropsGrid.SetDataSource(playerItems);
            siloInventoryGrid.SetDataSource(siloItems);

            dragIcon?.gameObject.SetActive(false);
            panelRoot.transform.localScale = Vector3.zero;
        }

        private void RegisterDragHandlers(GridView grid)
        {
            grid.AddGestureHandler<Gesture.OnPress, StorageSiloStorageSlotItemVisuals>(HandlePress);
            grid.AddGestureHandler<Gesture.OnDrag, StorageSiloStorageSlotItemVisuals>(HandleDrag);
            grid.AddGestureHandler<Gesture.OnRelease, StorageSiloStorageSlotItemVisuals>(HandleRelease);
            grid.AddGestureHandler<Gesture.OnCancel, StorageSiloStorageSlotItemVisuals>(HandleCancel);
        }

        private void UnregisterDragHandlers(GridView grid)
        {
            grid.RemoveGestureHandler<Gesture.OnPress, StorageSiloStorageSlotItemVisuals>(HandlePress);
            grid.RemoveGestureHandler<Gesture.OnDrag, StorageSiloStorageSlotItemVisuals>(HandleDrag);
            grid.RemoveGestureHandler<Gesture.OnRelease, StorageSiloStorageSlotItemVisuals>(HandleRelease);
            grid.RemoveGestureHandler<Gesture.OnCancel, StorageSiloStorageSlotItemVisuals>(HandleCancel);
        }

        private void ConfigureGrid(GridView grid)
        {
            grid.PrimaryAxis = Axis.Y;
            grid.CrossAxis = Axis.X;
            grid.CrossAxisItemCount = 3;

            grid.UIBlock.AutoLayout.AutoSpace = false;
            grid.UIBlock.AutoLayout.Spacing.Value = rowSpacing;

            grid.SetSliceProvider(ProvideSlice);
        }

        private void ProvideSlice(int index, GridView grid, ref GridSlice2D slice)
        {
            slice.Layout.AutoSize.X = AutoSize.Expand;
            slice.Layout.AutoSize.Y = AutoSize.Shrink;
            slice.Layout.Padding.Value = 0f;

            slice.AutoLayout.AutoSpace = false;
            slice.AutoLayout.Spacing.Value = columnSpacing;
        }

        private void BindPlayerItem(Data.OnBind<InventoryItem> evt, StorageSiloStorageSlotItemVisuals visuals, int index)
        {
            visuals.Bind(evt.UserData, false);
            ConfigureDragging(visuals, true);
        }

        private void BindSiloItem(Data.OnBind<InventoryItem> evt, StorageSiloStorageSlotItemVisuals visuals, int index)
        {
            visuals.Bind(evt.UserData, false);
            ConfigureDragging(visuals, true);
        }

        private static void ConfigureDragging(StorageSiloStorageSlotItemVisuals visuals, bool enabled)
        {
            Interactable interactable = visuals.View.GetComponent<Interactable>();

            if (interactable == null)
                return;

            interactable.Draggable = new ThreeD<bool>
            {
                X = enabled,
                Y = enabled,
                Z = false
            };
        }

        public void ToggleSiloUI(StorageSiloManager silo, PlayerController interactor)
        {
            if (silo == null || interactor == null)
                return;

            if (uiOpen && ActiveSilo == silo)
            {
                CloseSiloUI();
                return;
            }

            if (interactor.IsInventoryOpen)
            {
                interactor.ToggleInventory();
            }

            buildPlacer.enabled = false;
            inventory.UnEquipItem();
            Unsubscribe();
            CancelDrag();

            ActiveSilo = silo;
            player = interactor;
            interactionDetector = interactor.GetComponent<PlayerInteractionDetector>();
            inventory.OnInventoryChanged += RequestRefresh;
            ActiveSilo.OnStorageChanged += RequestRefresh;
            
            uiOpen = true;
            inputReady = false;

            panelRoot.transform.DOKill();
            // Bind current contents before the opening animation reveals the slots.
            RefreshGrids();
            panelRoot.transform.DOScale(1f, 0.35f).SetEase(Ease.OutCubic).SetUpdate(true).OnComplete(() =>
                {
                    inputReady = true;
                });
        }

        public void CloseSiloUI()
        {
            Unsubscribe();
            CancelDrag();
            uiOpen = false;
            inputReady = false;
            refreshPending = false;
            ActiveSilo = null;
            player = null;
            interactionDetector = null;

            panelRoot.transform.DOKill();
            panelRoot.transform.DOScale(0f, 0.35f).SetEase(Ease.OutCubic).SetUpdate(true);
        }

        private void RequestRefresh()
        {
            refreshPending = true;
        }

        private void RefreshGrids()
        {
            if (!uiOpen || ActiveSilo == null)
                return;

            List<InventoryItem> produce = inventory.GetProduceItems();

            playerItems.Clear();

            for (int i = 0; i < StorageSiloManager.SlotCount; i++)
            {
                int sourceIndex = i;
                playerItems.Add(sourceIndex < produce.Count ? produce[sourceIndex] : new InventoryItem());
            }

            siloItems.Clear();
            siloItems.AddRange(ActiveSilo.GetContentsSnapshot());

            playerCropsGrid.Refresh();
            siloInventoryGrid.Refresh();

            refreshPending = false;
        }
        
        private void Unsubscribe()
        {
            if (inventory != null)
                inventory.OnInventoryChanged -= RequestRefresh;

            if (ActiveSilo != null)
                ActiveSilo.OnStorageChanged -= RequestRefresh;
        }

        private void Update()
        {
            if (!uiOpen)
                return;

            bool lostSilo = ActiveSilo == null || !ActiveSilo.isActiveAndEnabled || interactionDetector == null ||
                            !ReferenceEquals(interactionDetector.CurrentTarget, ActiveSilo);

            if (lostSilo || player == null || player.IsInventoryOpen)
            {
                CloseSiloUI();
                return;
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (draggedProduce != null)
                    CancelDrag();
                else
                    CloseSiloUI();

                return;
            }

            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                CancelDrag();
            }
        }

        private void LateUpdate()
        {
            if (uiOpen && ActiveSilo != null && refreshPending && draggedProduce == null)
            {
                RefreshGrids();
            }
        }

        private void HandlePress(Gesture.OnPress evt, StorageSiloStorageSlotItemVisuals visuals, int index)
        {
            if (!CanInteract || draggedProduce != null || dragIcon == null) return;

            bool fromSilo = siloInventoryGrid.TryGetSourceIndex(visuals.View, out int sourceIndex);
            if (!fromSilo && !playerCropsGrid.TryGetSourceIndex(visuals.View, out sourceIndex))
                return;

            List<InventoryItem> sourceItems = fromSilo ? siloItems : playerItems;
            if (sourceIndex < 0 || sourceIndex >= sourceItems.Count)
                return;

            InventoryItem item = sourceItems[sourceIndex];

            if (!item.IsProduce || item.count <= 0)
                return;

            draggedProduce = item.Produce;
            draggedAmount = item.count;
            dragControlID = evt.Interaction.ControlID;
            dragStarted = false;
            draggingFromSilo = fromSilo;
            dragSourceIndex = sourceIndex;

            dragIcon.SetImage(item.item.itemDesc.Icon);

            if (dragCount != null)
                dragCount.Text = draggedAmount.ToString();
        }

        private void HandleDrag(Gesture.OnDrag evt, StorageSiloStorageSlotItemVisuals visuals, int index)
        {
            if (!CanInteract || draggedProduce == null || evt.Interaction.ControlID != dragControlID) return;

            dragStarted = true;
            dragIcon.gameObject.SetActive(true);

            dragIcon.TrySetWorldPosition(evt.PointerPositions.Current);
        }

        private void HandleRelease(Gesture.OnRelease evt, StorageSiloStorageSlotItemVisuals visuals, int index)
        {
            if (draggedProduce == null || evt.Interaction.ControlID != dragControlID) return;

            GridView destination = draggingFromSilo ? playerCropsGrid : siloInventoryGrid;
            int targetIndex = CanInteract && dragStarted ? FindDropSlot(evt.Interaction.Ray, destination) : -1;

            ProduceItemSO produce = draggedProduce;
            int amount = draggedAmount;
            StorageSiloManager silo = ActiveSilo;
            bool fromSilo = draggingFromSilo;
            int sourceIndex = dragSourceIndex;

            CancelDrag();

            if (targetIndex >= 0 && silo != null)
            {
                if (fromSilo)
                {
                    // PlayerCrops is a filtered display, not the actual inventory slots.
                    silo.WithdrawTo(inventory, produce, sourceIndex, amount);
                }
                else
                {
                    silo.DepositFrom(inventory, produce, targetIndex, amount);
                }
            }

            RequestRefresh();
        }

        private int FindDropSlot(Ray ray, GridView destination)
        {
            Interaction.RaycastAll(ray, dropHits);

            foreach (UIBlockHit hit in dropHits)
            {
                Transform hitTransform = hit.UIBlock.transform;

                if (hitTransform.IsChildOf(dragIcon.transform))
                    continue;

                ItemView view = hit.UIBlock.GetComponentInParent<ItemView>();

                if (view != null)
                {
                    return destination.TryGetSourceIndex(view, out int index) ? index : -1;
                }

                if (hit.UIBlock.GetComponentInParent<Interactable>() != null)
                    return -1;
            }

            return -1;
        }

        private void CancelDrag()
        {
            draggedProduce = null;
            draggedAmount = 0;
            dragStarted = false;
            draggingFromSilo = false;
            dragSourceIndex = -1;

            if (dragIcon != null)
                dragIcon.gameObject.SetActive(false);

            RequestRefresh();
        }

        private void HandleCancel(Gesture.OnCancel evt, StorageSiloStorageSlotItemVisuals visuals, int index)
        {
            if (draggedProduce != null && evt.Interaction.ControlID == dragControlID)
                CancelDrag();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused)
                CancelDrag();
        }

        private void OnDisable()
        {
            Unsubscribe();
            CancelDrag();
            uiOpen = false;
            inputReady = false;
            refreshPending = false;
            ActiveSilo = null;
            player = null;
            interactionDetector = null;

            if (panelRoot != null)
            {
                panelRoot.transform.DOKill();
                panelRoot.transform.localScale = Vector3.zero;
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
            if (panelRoot != null)
                panelRoot.transform.DOKill();

            if (playerCropsGrid != null)
            {
                playerCropsGrid.RemoveDataBinder<InventoryItem, StorageSiloStorageSlotItemVisuals>(BindPlayerItem);
                UnregisterDragHandlers(playerCropsGrid);
                playerCropsGrid.ClearSliceProvider();
            }

            if (siloInventoryGrid != null)
            {
                siloInventoryGrid.RemoveDataBinder<InventoryItem, StorageSiloStorageSlotItemVisuals>(BindSiloItem);
                UnregisterDragHandlers(siloInventoryGrid);
                siloInventoryGrid.ClearSliceProvider();
            }
        }
    }
}
