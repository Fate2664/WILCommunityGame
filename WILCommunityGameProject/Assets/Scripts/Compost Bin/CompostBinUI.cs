using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Nova;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WILCommunityGame
{
    public class CompostBinUI : MonoBehaviour
    {
        [Header("References")] [SerializeField] private GameObject panelRoot;
        [SerializeField] private UIManager inventory;
        [SerializeField] private BuildPlacer buildPlacer;

        [Header("Inventory")] [SerializeField] private GridView playerItemsGrid;
        [SerializeField] private float columnSpacing = 15f;
        [SerializeField] private float rowSpacing = 15f;

        [Header("Compost Slots")] [SerializeField] private ItemView cropWasteSlot;
        [SerializeField] private ItemView naturalWasteSlot;
        [SerializeField] private ItemView fertilizerSlot;
        [SerializeField] private UIBlock arrowSlider;

        [Header("Drag Preview")] [SerializeField] private UIBlock2D dragIcon;
        [SerializeField] private TextBlock dragCount;

        [Header("Drag Diagnostics")]
        [Tooltip("Log drag lifecycle, visibility and a render-state snapshot. Press F8 while dragging to capture and pause the Editor for Frame Debugger inspection.")]
        [SerializeField] private bool debugDragPreview;

        public bool IsOpen => uiOpen;
        public CompostBin ActiveBin { get; private set; }

        private const int InventoryColumns = 6;
        private const int InventoryRows = 2;
        private const float InventorySlotSize = 120f;

        private readonly List<InventoryItem> playerItems = new();
        private readonly List<UIBlockHit> dropHits = new();
        private ItemView[] binSlots;
        private PlayerController player;
        private PlayerInteractionDetector interactionDetector;
        private bool uiOpen;
        private bool inputReady;
        private bool refreshPending;
        private InventoryItemData draggedItem;
        private int draggedAmount;
        private int dragSourceIndex = -1;
        private uint dragControlID;
        private bool dragStarted;
        private string lastDragPreviewState;

        private bool CanInteract => isActiveAndEnabled && uiOpen && inputReady && ActiveBin != null &&
                                    ActiveBin.isActiveAndEnabled;

        private void Awake()
        {
            if (panelRoot == null)
                panelRoot = gameObject;

            if (inventory == null || playerItemsGrid == null || cropWasteSlot == null ||
                naturalWasteSlot == null || fertilizerSlot == null || arrowSlider == null)
            {
                Debug.LogError("Assign the compost bin UI inventory, grid, slots and arrow slider.", this);
                enabled = false;
                return;
            }

            binSlots = new[] { cropWasteSlot, naturalWasteSlot, fertilizerSlot };

            dragIcon?.gameObject.SetActive(false);

            playerItemsGrid.PrimaryAxis = Axis.Y;
            playerItemsGrid.CrossAxis = Axis.X;
            playerItemsGrid.CrossAxisItemCount = InventoryColumns;
            playerItemsGrid.UIBlock.AutoLayout.AutoSpace = false;
            playerItemsGrid.UIBlock.AutoLayout.Spacing.Value = rowSpacing;
            playerItemsGrid.SetSliceProvider(ProvideSlice);
            playerItemsGrid.AddDataBinder<InventoryItem, StorageSiloStorageSlotItemVisuals>(BindPlayerItem);
            playerItemsGrid.AddGestureHandler<Gesture.OnPress, StorageSiloStorageSlotItemVisuals>(HandleGridPress);
            playerItemsGrid.AddGestureHandler<Gesture.OnDrag, StorageSiloStorageSlotItemVisuals>(HandleGridDrag);
            playerItemsGrid.AddGestureHandler<Gesture.OnRelease, StorageSiloStorageSlotItemVisuals>(HandleGridRelease);
            playerItemsGrid.AddGestureHandler<Gesture.OnCancel, StorageSiloStorageSlotItemVisuals>(HandleGridCancel);
            playerItemsGrid.SetDataSource(playerItems);

            Scroller scroller = playerItemsGrid.GetComponent<Scroller>();
            if (scroller == null)
                scroller = playerItemsGrid.gameObject.AddComponent<Scroller>();
            scroller.DragScrolling = false;
            scroller.VectorScrolling = true;

            ClipMask clip = playerItemsGrid.GetComponent<ClipMask>();
            if (clip == null)
                clip = playerItemsGrid.gameObject.AddComponent<ClipMask>();
            clip.Clip = true;

            foreach (ItemView slot in binSlots)
            {
                slot.UIBlock.AddGestureHandler<Gesture.OnPress, StorageSiloStorageSlotItemVisuals>(HandlePress);
                slot.UIBlock.AddGestureHandler<Gesture.OnDrag, StorageSiloStorageSlotItemVisuals>(HandleDrag);
                slot.UIBlock.AddGestureHandler<Gesture.OnRelease, StorageSiloStorageSlotItemVisuals>(HandleRelease);
                slot.UIBlock.AddGestureHandler<Gesture.OnCancel, StorageSiloStorageSlotItemVisuals>(HandleCancel);
            }

            arrowSlider.AutoSize.X = AutoSize.None;
            arrowSlider.Size.X.Percent = 0f;
            panelRoot.transform.localScale = Vector3.zero;
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
            visuals.View.UIBlock.Size.X.Value = InventorySlotSize;
            visuals.View.UIBlock.Size.Y.Value = InventorySlotSize;
            visuals.Bind(evt.UserData, false);
            ConfigureDragging(visuals.View, true);
        }

        private static void ConfigureDragging(ItemView view, bool enabled)
        {
            Interactable interactable = view.GetComponent<Interactable>();
            if (interactable == null)
                return;

            interactable.Draggable = new ThreeD<bool> { X = enabled, Y = enabled, Z = false };
        }

        public void ToggleCompostUI(CompostBin bin, PlayerController interactor)
        {
            if (!isActiveAndEnabled || bin == null || interactor == null)
                return;

            if (uiOpen && ActiveBin == bin)
            {
                CloseCompostUI();
                return;
            }

            if (interactor.IsInventoryOpen)
                interactor.ToggleInventory();

            if (buildPlacer != null)
                buildPlacer.enabled = false;

            inventory.UnEquipItem();
            Unsubscribe();
            CancelDrag();
            ActiveBin = bin;
            player = interactor;
            interactionDetector = interactor.GetComponent<PlayerInteractionDetector>();
            inventory.OnInventoryChanged += RequestRefresh;
            ActiveBin.OnContentsChanged += RequestRefresh;
            uiOpen = true;
            inputReady = false;

            panelRoot.transform.DOKill();
            RefreshItems();
            panelRoot.transform.DOScale(1f, 0.35f).SetEase(Ease.OutCubic).SetUpdate(true).OnComplete(() =>
            {
                inputReady = true;
            });
        }

        public void closeUI() => CloseCompostUI();

        public void CloseCompostUI()
        {
            ResetPanel();
            panelRoot.transform.DOKill();
            panelRoot.transform.DOScale(0f, 0.35f).SetEase(Ease.OutCubic).SetUpdate(true);
        }

        private void ResetPanel()
        {
            Unsubscribe();
            CancelDrag();
            uiOpen = false;
            inputReady = false;
            refreshPending = false;
            ActiveBin = null;
            player = null;
            interactionDetector = null;
        }

        private void RequestRefresh() => refreshPending = true;

        private void RefreshItems()
        {
            playerItems.Clear();
            playerItems.AddRange(inventory.GetCompostItems());

            while (playerItems.Count < InventoryColumns * InventoryRows)
                playerItems.Add(new InventoryItem());

            playerItemsGrid.Refresh();

            for (int index = 0; index < binSlots.Length; index++)
            {
                ItemView slot = binSlots[index];
                if (slot.TryGetVisuals(out StorageSiloStorageSlotItemVisuals visuals))
                    visuals.Bind(ActiveBin.GetSlotSnapshot(index), ActiveBin.IsComposting);

                ConfigureDragging(slot, !ActiveBin.IsComposting);
            }

            arrowSlider.Size.X.Percent = ActiveBin.Progress;
            refreshPending = false;
        }

        private void Unsubscribe()
        {
            if (inventory != null)
                inventory.OnInventoryChanged -= RequestRefresh;
            if (ActiveBin != null)
                ActiveBin.OnContentsChanged -= RequestRefresh;
        }

        private void Update()
        {
            if (!uiOpen)
                return;

            bool lostBin = ActiveBin == null || !ActiveBin.isActiveAndEnabled || interactionDetector == null ||
                           !ReferenceEquals(interactionDetector.CurrentTarget, ActiveBin);

            if (lostBin || player == null || player.IsInventoryOpen)
            {
                CloseCompostUI();
                return;
            }

            arrowSlider.Size.X.Percent = ActiveBin.Progress;

            if (debugDragPreview && dragStarted && Keyboard.current != null && Keyboard.current.f8Key.wasPressedThisFrame)
                StartCoroutine(CaptureDragRenderState(pauseEditor: true));

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (draggedItem != null)
                    CancelDrag("Escape pressed");
                else
                    CloseCompostUI();
            }

            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
                CancelDrag("Right mouse button pressed");
        }

        private void LateUpdate()
        {
            if (draggedItem != null)
                LogDragPreviewState("Visibility changed during drag (LateUpdate)");

            if (uiOpen && ActiveBin != null && refreshPending && draggedItem == null)
                RefreshItems();
        }

        private void HandleGridPress(Gesture.OnPress evt, StorageSiloStorageSlotItemVisuals visuals, int index) => HandlePress(evt, visuals);
        private void HandleGridDrag(Gesture.OnDrag evt, StorageSiloStorageSlotItemVisuals visuals, int index) => HandleDrag(evt, visuals);
        private void HandleGridRelease(Gesture.OnRelease evt, StorageSiloStorageSlotItemVisuals visuals, int index) => HandleRelease(evt, visuals);
        private void HandleGridCancel(Gesture.OnCancel evt, StorageSiloStorageSlotItemVisuals visuals, int index) => HandleCancel(evt, visuals);

        private void HandlePress(Gesture.OnPress evt, StorageSiloStorageSlotItemVisuals visuals)
        {
            if (!CanInteract || draggedItem != null || dragIcon == null)
                return;

            int sourceIndex = System.Array.IndexOf(binSlots, visuals.View);
            InventoryItem item;

            if (sourceIndex >= 0)
            {
                if (ActiveBin.IsComposting)
                    return;

                item = ActiveBin.GetSlotSnapshot(sourceIndex);
            }
            else
            {
                if (!playerItemsGrid.TryGetSourceIndex(visuals.View, out int playerIndex) ||
                    playerIndex < 0 || playerIndex >= playerItems.Count)
                    return;

                item = playerItems[playerIndex];
            }

            if ((!item.IsCompostItem && !item.IsFertilizer) || item.count <= 0)
                return;

            draggedItem = item.item;
            draggedAmount = item.count;
            dragSourceIndex = sourceIndex;
            dragControlID = evt.Interaction.ControlID;
            dragStarted = false;
            dragIcon.SetImage(item.item.itemDesc.Icon);
            if (dragCount != null)
                dragCount.Text = draggedAmount.ToString();

            LogDragPreviewState("Press accepted; waiting for drag threshold", force: true);
        }

        private void HandleDrag(Gesture.OnDrag evt, StorageSiloStorageSlotItemVisuals visuals)
        {
            if (!CanInteract || draggedItem == null || evt.Interaction.ControlID != dragControlID)
                return;

            bool firstDrag = !dragStarted;
            if (!firstDrag)
                LogDragPreviewState("Visibility changed before HandleDrag reactivated the preview");

            dragStarted = true;
            dragIcon.gameObject.SetActive(true);
            Vector3 localPosition = dragIcon.transform.parent.InverseTransformPoint(evt.PointerPositions.Current);
            localPosition.z = 0f;
            dragIcon.TrySetLocalPosition(localPosition);
            LogDragPreviewState(firstDrag ? "Drag started" : "Visibility changed after HandleDrag", force: firstDrag);
            if (firstDrag && debugDragPreview)
                StartCoroutine(CaptureDragRenderState(pauseEditor: false));
        }

        private void HandleRelease(Gesture.OnRelease evt, StorageSiloStorageSlotItemVisuals visuals)
        {
            if (draggedItem == null || evt.Interaction.ControlID != dragControlID)
                return;

            int destination = CanInteract ? FindDropSlot(evt.Interaction.Ray) : -2;
            InventoryItemData item = draggedItem;
            int amount = draggedAmount;
            int source = dragSourceIndex;
            bool didDrag = dragStarted;
            CompostBin bin = ActiveBin;
            CancelDrag();

            if (!CanInteract || bin == null)
                return;

            if (source >= 0 && ((didDrag && destination == -1) ||
                                (!didDrag && source == CompostBin.OutputSlot && destination == source)))
            {
                bin.WithdrawTo(inventory, item, source, amount);
            }
            else if (source == -1 && didDrag && destination >= 0 && destination < CompostBin.OutputSlot &&
                     item is CompostItemSO compost)
            {
                bin.DepositFrom(inventory, compost, destination, amount);
            }

            RequestRefresh();
        }

        private int FindDropSlot(Ray ray)
        {
            Interaction.RaycastAll(ray, dropHits);

            foreach (UIBlockHit hit in dropHits)
            {
                Transform hitTransform = hit.UIBlock.transform;
                if (dragIcon != null && hitTransform.IsChildOf(dragIcon.transform))
                    continue;

                for (int index = 0; index < binSlots.Length; index++)
                {
                    if (hitTransform.IsChildOf(binSlots[index].transform))
                        return index;
                }

                if (hitTransform.IsChildOf(playerItemsGrid.transform))
                    return -1;

                if (hit.UIBlock.GetComponentInParent<ItemView>() != null)
                    return -2;
            }

            return -2;
        }

        private void CancelDrag([System.Runtime.CompilerServices.CallerMemberName] string reason = "")
        {
            bool hadDrag = draggedItem != null || dragStarted;
            if (hadDrag)
                LogDragPreviewState($"CancelDrag called by {reason} (before hiding)", force: true, includeStack: true);

            if (draggedItem != null && dragIcon != null)
                dragIcon.gameObject.SetActive(false);

            if (hadDrag)
                LogDragPreviewState("CancelDrag completed (after hiding)", force: true);

            draggedItem = null;
            draggedAmount = 0;
            dragSourceIndex = -1;
            dragStarted = false;
            lastDragPreviewState = null;
        }

        private void LogDragPreviewState(string reason, bool force = false, bool includeStack = false)
        {
            if (!debugDragPreview)
                return;

            string state = dragIcon == null
                ? "dragIcon=null"
                : $"activeSelf={dragIcon.gameObject.activeSelf}, " +
                  $"activeInHierarchy={dragIcon.gameObject.activeInHierarchy}, " +
                  $"enabled={dragIcon.enabled}, BodyEnabled={dragIcon.BodyEnabled}, " +
                  $"alpha={dragIcon.Color.a:R}";

            // Only report changes, rather than logging every frame of a drag.
            if (!force && state == lastDragPreviewState)
                return;

            lastDragPreviewState = state;
            string stack = includeStack ? "\n" + StackTraceUtility.ExtractStackTrace() : string.Empty;
            Debug.Log($"[Compost DragPreview] frame={Time.frameCount}: {reason}. " +
                      $"{state}; item={draggedItem?.name}, dragStarted={dragStarted}, " +
                      $"CanInteract={CanInteract}, focused={Application.isFocused}{stack}", this);
        }

        private IEnumerator CaptureDragRenderState(bool pauseEditor)
        {
            // Sample after Nova's layout/camera updates and rendering, not inside the input event.
            yield return new WaitForEndOfFrame();
            if (!debugDragPreview || !dragStarted || dragIcon == null || panelRoot == null)
                yield break;

            ScreenSpace screenSpace = dragIcon.GetComponentInParent<ScreenSpace>();
            Camera renderCamera = screenSpace != null ? screenSpace.TargetCamera : null;
            SortGroup screenGroup = screenSpace != null ? screenSpace.GetComponent<SortGroup>() : null;
            Debug.Log($"[Compost DragRender] frame={Time.frameCount}: " +
                      $"ScreenSpace={screenSpace?.name}, camera={renderCamera?.name}, " +
                      $"inheritedQueue={screenGroup?.RenderQueue}, " +
                      $"inheritedRenderOverOpaqueGeometry={screenGroup?.RenderOverOpaqueGeometry}. " +
                      "Child queue values below are stored settings, not the inherited effective queue.", this);

            LogDragRenderBlock("Preview", dragIcon, renderCamera);
            LogDragRenderBlock("Panel", panelRoot.GetComponent<UIBlock>(), renderCamera);
            Transform background = panelRoot.transform.Find("Background");
            if (background != null)
                LogDragRenderBlock("Panel background", background.GetComponent<UIBlock>(), renderCamera);

            Vector3 previewInPanel = panelRoot.transform.InverseTransformPoint(dragIcon.transform.position);
            Debug.Log($"[Compost DragRender] previewInPanel={previewInPanel.ToString("G9")}, " +
                      $"rotationDifference={Quaternion.Angle(panelRoot.transform.rotation, dragIcon.transform.rotation):G9} degrees. " +
                      "These are Transform measurements; use Frame Debugger to verify the actual draw order.", this);

#if UNITY_EDITOR
            if (pauseEditor)
            {
                Debug.Log("[Compost DragRender] Paused for inspection. Open Window > Analysis > Frame Debugger, " +
                          "enable it and step through the Nova draws. Disable Frame Debugger before resuming Play Mode.", this);
                Debug.Break();
            }
#endif
        }

        private void LogDragRenderBlock(string label, UIBlock block, Camera renderCamera)
        {
            if (block == null)
                return;

            SortGroup group = block.GetComponentInParent<SortGroup>();
            Vector3 viewport = renderCamera != null
                ? renderCamera.WorldToViewportPoint(block.transform.position)
                : Vector3.zero;
            string zIndex = block is UIBlock2D block2D ? block2D.ZIndex.ToString() : "n/a";
            Debug.Log($"[Compost DragRender] {label}={block.name} (instance={block.GetInstanceID()}): " +
                      $"active={block.gameObject.activeInHierarchy}, enabled={block.enabled}, " +
                      $"parent={block.transform.parent?.name}, novaRoot={block.Root?.name}, " +
                      $"sibling={block.transform.GetSiblingIndex()}, ZIndex={zIndex}, " +
                      $"sortGroup={group?.name}, sortGroupActive={group?.isActiveAndEnabled}, " +
                      $"sortingOrder={group?.SortingOrder}, storedQueue={group?.RenderQueue}, " +
                      $"localPosition={block.transform.localPosition.ToString("G9")}, " +
                      $"worldPosition={block.transform.position.ToString("G9")}, " +
                      $"worldScale={block.transform.lossyScale.ToString("G9")}, " +
                      $"calculatedSize={block.CalculatedSize.Value.ToString("G9")}, " +
                      $"viewportXYAndCameraDepth={viewport.ToString("G9")}", block);
        }

        private void HandleCancel(Gesture.OnCancel evt, StorageSiloStorageSlotItemVisuals visuals)
        {
            if (draggedItem != null && evt.Interaction.ControlID == dragControlID)
                CancelDrag();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused)
                CancelDrag("Application/Game view lost focus");
        }

        private void OnDisable()
        {
            ResetPanel();
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

            if (playerItemsGrid != null)
            {
                playerItemsGrid.RemoveDataBinder<InventoryItem, StorageSiloStorageSlotItemVisuals>(BindPlayerItem);
                playerItemsGrid.RemoveGestureHandler<Gesture.OnPress, StorageSiloStorageSlotItemVisuals>(HandleGridPress);
                playerItemsGrid.RemoveGestureHandler<Gesture.OnDrag, StorageSiloStorageSlotItemVisuals>(HandleGridDrag);
                playerItemsGrid.RemoveGestureHandler<Gesture.OnRelease, StorageSiloStorageSlotItemVisuals>(HandleGridRelease);
                playerItemsGrid.RemoveGestureHandler<Gesture.OnCancel, StorageSiloStorageSlotItemVisuals>(HandleGridCancel);
                playerItemsGrid.ClearSliceProvider();
            }

            if (binSlots == null)
                return;

            foreach (ItemView slot in binSlots)
            {
                if (slot == null)
                    continue;

                slot.UIBlock.RemoveGestureHandler<Gesture.OnPress, StorageSiloStorageSlotItemVisuals>(HandlePress);
                slot.UIBlock.RemoveGestureHandler<Gesture.OnDrag, StorageSiloStorageSlotItemVisuals>(HandleDrag);
                slot.UIBlock.RemoveGestureHandler<Gesture.OnRelease, StorageSiloStorageSlotItemVisuals>(HandleRelease);
                slot.UIBlock.RemoveGestureHandler<Gesture.OnCancel, StorageSiloStorageSlotItemVisuals>(HandleCancel);
            }
        }
    }
}
