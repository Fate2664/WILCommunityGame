using System;
using Nova;
using UnityEngine;
using System.Collections.Generic;
using WILCommunityGame;


public class UIManager : MonoBehaviour, ITimeTracker
{
    #region Class Variables

    [Header("References")] [SerializeField]
    private PlayerController playerController;

    [SerializeField] private BuildPlacer buildPlacer;

    [Header("Inventory")] [SerializeField] private ItemDatabase ItemDatabase = null;
    [SerializeField] private ItemView EquipItemRoot = null;
    [SerializeField] private ItemView closeButtonRoot = null;
    [SerializeField] private ItemView fenceButtonRoot = null;
    [SerializeField] private ItemView destructionButtonRoot = null;

    [Header("Information")] [SerializeField]
    private InformationTipSO hoeTip;

    [Space(10)] [Header("Grid Layout")] public GridView Grid = null;
    public int Count = 24;

    [Space(10)] [Header("Row Styling")] [SerializeField]
    private int padding = 10;

    private int columnSpacing = 10;

    [Header("Date & Time")] [SerializeField]
    private TextBlock TimeText = null;

    [SerializeField] private TextBlock TimePrefix = null;
    [SerializeField] private TextBlock DayText = null;

    public InventoryItem EquippedItem => equippedItem;
    public event Action<InventoryItem> OnEquippedItemChanged;
    private List<InventoryItem> Items;
    private readonly InventoryItem emptyEquippedItem = new();
    private InventoryItem equippedItem;
    private bool inventoryNeedsRefresh;
    public event Action<InformationTipSO> OnInformationTipRequested;
    private readonly HashSet<InformationTipSO> shownInformtationTips = new();
    private bool firstTimeHoeEquipped = true; //<-- Nice naming lol
    public event Action OnInventoryChanged;

    #endregion

    private void Start()
    {
        Items = ItemDatabase.GetEmptyItems(Count);
        InitGrid(Grid, Items);
        RegisterStandaloneGestureHandlers();
        RefreshEquippedItem();
        TimeManager.Instance.RegisterTracker(this);
        OnInventoryChanged?.Invoke();
    }

    #region Inventory Methods

    public void AddItemToInventory(InventoryItemData item, int count = 1)
    {
        if (item == null || count <= 0)
            return;

        int added = TryAddItem(item, count);
    }

    public void Add5ItemsToInventory(InventoryItemData item)
    {
        TryAddItem(item, 5);
    }
    
    public void Add1ItemToInventory(InventoryItemData item)
    {
        TryAddItem(item, 1);
    }

    public int GetRemainingCapacity(InventoryItemData item)
    {
        if (Items == null || item == null)
            return 0;

        int capacity = 0;

        foreach (InventoryItem stack in Items)
        {
            if (stack == null || stack.isEmpty)
            {
                capacity += InventoryItem.maxCount;
            }
            else if (stack.item == item)
            {
                capacity += Mathf.Max(0, InventoryItem.maxCount - stack.count);
            }
        }

        return capacity;
    }

    public int TryAddItem(InventoryItemData item, int amount)
    {
        if (Items == null || item == null || amount <= 0)
            return 0;

        int remaining = amount;

        for (int i = 0; i < Items.Count && remaining > 0; i++)
        {
            InventoryItem stack = Items[i];

            if (stack == null || stack.isEmpty || stack.item != item)
                continue;

            int availableSpace = Mathf.Max(0, InventoryItem.maxCount - stack.count);
            int toAdd = Mathf.Min(remaining, availableSpace);

            if (toAdd <= 0)
                continue;

            stack.IncreaseCount(toAdd);
            remaining -= toAdd;
        }

        // Put the remaining quantity into empty slots.
        for (int i = 0; i < Items.Count && remaining > 0; i++)
        {
            InventoryItem stack = Items[i];

            if (stack != null && !stack.isEmpty)
                continue;

            int toAdd = Mathf.Min(remaining, InventoryItem.maxCount);

            Items[i] = new InventoryItem
            {
                item = item,
                count = toAdd
            };

            remaining -= toAdd;
        }

        int added = amount - remaining;

        if (added > 0)
        {
            inventoryNeedsRefresh = true;
            RefreshInventory();

            if (equippedItem != null && equippedItem.item == item)
            {
                RefreshEquippedItem();
            }

            OnInventoryChanged?.Invoke();
        }

        return added;
    }

    public bool TryAddItemExact(InventoryItemData item, int amount)
    {
        if (item == null || amount <= 0)
            return false;

        if (GetRemainingCapacity(item) < amount)
            return false;

        return TryAddItem(item, amount) == amount;
    }

    public int RemoveProduce(ProduceType type, int amount)
    {
        return RemoveMatchingProduce(stack => stack.Produce.produceType == type, amount);
    }

    public int RemoveProduce(ProduceItemSO produce, int amount)
    {
        if (produce == null)
            return 0;

        return RemoveMatchingProduce(stack => stack.Produce == produce, amount);
    }

    private int RemoveMatchingProduce(Predicate<InventoryItem> matches, int amount)
    {
        if (Items == null || amount <= 0) return 0;

        int remaining = amount;

        for (int i = 0; i < Items.Count && remaining > 0; i++)
        {
            InventoryItem stack = Items[i];

            if (stack == null || !stack.IsProduce || stack.count <= 0 || !matches(stack))
                continue;

            int removed = Mathf.Min(stack.count, remaining);

            stack.DecreaseCount(removed);
            remaining -= removed;

            if (stack.count <= 0)
            {
                if (ReferenceEquals(equippedItem, stack))
                    equippedItem = null;

                Items[i] = new InventoryItem();
            }
        }

        int removedTotal = amount - remaining;

        if (removedTotal > 0)
        {
            inventoryNeedsRefresh = true;
            RefreshInventory();
            RefreshEquippedItem();
            OnInventoryChanged?.Invoke();
        }

        return removedTotal;
    }

    public void RefreshInventory()
    {
        if (!Grid.gameObject.activeInHierarchy || !inventoryNeedsRefresh)
        {
            return;
        }

        Grid.Refresh();
        inventoryNeedsRefresh = false;
    }

    public List<InventoryItem> GetProduceItems()
    {
        List<InventoryItem> crops = new();
        if (Items == null) return crops;

        foreach (var stack in Items)
        {
            if (stack == null || !stack.IsProduce || stack.count <= 0)
                continue;

            InventoryItem existing = crops.Find(x => x.item == stack.item);

            if (existing != null)
            {
                existing.count += stack.count;
            }
            else
            {
                crops.Add(new InventoryItem
                {
                    item = stack.item,
                    count = stack.count
                });
            }
        }

        return crops;
    }

    public int GetProduceCount(ProduceItemSO produce)
    {
        if (Items == null || produce == null) return 0;

        int total = 0;

        foreach (var stack in Items)
        {
            if (stack != null && stack.item == produce && stack.count > 0)
                total += stack.count;
        }

        return total;
    }

    #endregion

    #region Register Methods

    private void InitGrid(GridView grid, List<InventoryItem> datasource)
    {
        grid.AddDataBinder<InventoryItem, InventoryItemVisuals>(BindItem);

        grid.SetSliceProvider(ProvideSlice);

        grid.AddGestureHandler<Gesture.OnHover, InventoryItemVisuals>(InventoryItemVisuals.HandleHover);
        grid.AddGestureHandler<Gesture.OnUnhover, InventoryItemVisuals>(InventoryItemVisuals.HandleUnhover);
        grid.AddGestureHandler<Gesture.OnPress, InventoryItemVisuals>(InventoryItemVisuals.HandlePress);
        grid.AddGestureHandler<Gesture.OnRelease, InventoryItemVisuals>(InventoryItemVisuals.HandleRelease);

        grid.SetDataSource(datasource);
    }

    private void ProvideSlice(int sliceIndex, GridView gridview, ref GridSlice2D gridslice)
    {
        gridslice.Layout.AutoSize.Y = AutoSize.Shrink;
        gridslice.AutoLayout.AutoSpace = false;
        gridslice.AutoLayout.Spacing.Value = columnSpacing;
        gridslice.Layout.Padding.Value = 0f;
        gridslice.Layout.Padding.XY.Value = padding;
    }

    private void BindItem(Data.OnBind<InventoryItem> evt, InventoryItemVisuals target, int index)
    {
        target.Bind(evt.UserData, this);
    }

    private void RegisterStandaloneGestureHandlers()
    {
        if (EquipItemRoot != null)
        {
            EquipItemRoot.UIBlock.AddGestureHandler<Gesture.OnHover, InventoryItemVisuals>(InventoryItemVisuals
                .HandleHover);
            EquipItemRoot.UIBlock.AddGestureHandler<Gesture.OnUnhover, InventoryItemVisuals>(InventoryItemVisuals
                .HandleUnhover);
            EquipItemRoot.UIBlock.AddGestureHandler<Gesture.OnPress, InventoryItemVisuals>(InventoryItemVisuals
                .HandlePress);
            EquipItemRoot.UIBlock.AddGestureHandler<Gesture.OnRelease, InventoryItemVisuals>(InventoryItemVisuals
                .HandleRelease);
        }

        if (closeButtonRoot != null)
        {
            closeButtonRoot.UIBlock.AddGestureHandler<Gesture.OnHover, InventoryButtonVisuals>(InventoryButtonVisuals
                .HandleHover);
            closeButtonRoot.UIBlock.AddGestureHandler<Gesture.OnUnhover, InventoryButtonVisuals>(InventoryButtonVisuals
                .HandleUnhover);
            closeButtonRoot.UIBlock.AddGestureHandler<Gesture.OnPress, InventoryButtonVisuals>(InventoryButtonVisuals
                .HandlePress);
            closeButtonRoot.UIBlock.AddGestureHandler<Gesture.OnRelease, InventoryButtonVisuals>(InventoryButtonVisuals
                .HandleRelease);
        }

        if (fenceButtonRoot != null)
        {
            fenceButtonRoot.UIBlock.AddGestureHandler<Gesture.OnHover, InventoryButtonVisuals>(InventoryButtonVisuals
                .HandleHover);
            fenceButtonRoot.UIBlock.AddGestureHandler<Gesture.OnUnhover, InventoryButtonVisuals>(InventoryButtonVisuals
                .HandleUnhover);
            fenceButtonRoot.UIBlock.AddGestureHandler<Gesture.OnPress, InventoryButtonVisuals>(InventoryButtonVisuals
                .HandlePress);
            fenceButtonRoot.UIBlock.AddGestureHandler<Gesture.OnRelease, InventoryButtonVisuals>(InventoryButtonVisuals
                .HandleRelease);
        }

        if (destructionButtonRoot != null)
        {
            destructionButtonRoot.UIBlock.AddGestureHandler<Gesture.OnHover, InventoryButtonVisuals>(
                InventoryButtonVisuals.HandleHover);
            destructionButtonRoot.UIBlock.AddGestureHandler<Gesture.OnUnhover, InventoryButtonVisuals>(
                InventoryButtonVisuals.HandleUnhover);
            destructionButtonRoot.UIBlock.AddGestureHandler<Gesture.OnPress, InventoryButtonVisuals>(
                InventoryButtonVisuals.HandlePress);
            destructionButtonRoot.UIBlock.AddGestureHandler<Gesture.OnRelease, InventoryButtonVisuals>(
                InventoryButtonVisuals.HandleRelease);
        }
    }

    #endregion

    #region Equip Item Methods

    public void EquipItem(InventoryItem item)
    {
        buildPlacer.enabled = false;
        equippedItem = item != null && !item.isEmpty && item.count > 0 ? item : null;

        if (playerController.IsInventoryOpen)
        {
            playerController.ToggleInventory();
        }

        RefreshEquippedItem();

        if (equippedItem == null)
            return;

        if (equippedItem.item is BuildableObjectItemSO buildable)
        {
            if (!buildPlacer.BeginBuildablePlacement(buildable, this))
            {
                UnEquipItem();
            }

            return;
        }

        if (equippedItem.item is ToolItemSO tool)
        {
            switch (tool.toolType)
            {
                case ToolType.Hoe:
                    HoeEquipped();
                    break;

                case ToolType.WateringCan:
                    break;
            }
        }
    }

    public bool TryUseEquippedItem(int amount = 1)
    {
        if (equippedItem == null || equippedItem.isEmpty || amount <= 0 || equippedItem.count < amount)
            return false;

        equippedItem.DecreaseCount(amount);

        if (equippedItem.count <= 0)
        {
            int index = Items.IndexOf(equippedItem);
            if (index >= 0) Items[index] = new InventoryItem();
            equippedItem = null;
        }

        inventoryNeedsRefresh = true;
        RefreshInventory();
        RefreshEquippedItem();
        OnInventoryChanged?.Invoke();
        return true;
    }

    public void UnEquipItem()
    {
        equippedItem = null;
        RefreshEquippedItem();
    }

    private void HoeEquipped()
    {
        buildPlacer.enabled = true;
        buildPlacer.SetDestroyMode(false);
        buildPlacer.placementPieceType = BuildPieceType.Floor;
        if (hoeTip != null && firstTimeHoeEquipped)
        {
            ShowInformationTip(hoeTip);
            firstTimeHoeEquipped = false;
        }
    }

    public void FenceEquipped()
    {
        playerController.ToggleInventory();
        buildPlacer.enabled = true;
        buildPlacer.SetDestroyMode(false);
        buildPlacer.placementPieceType = BuildPieceType.Wall;
    }

    public int FillEquippedWateringCan(int availableWater)
    {
        if (equippedItem == null || !equippedItem.IsWateringCan)
            return 0;

        int transferredWater = equippedItem.FillWater(availableWater);
        if (transferredWater > 0)
        {
            RefreshEquippedItem();
        }

        return transferredWater;
    }

    public void DestroyEquipped()
    {
        playerController.ToggleInventory();
        buildPlacer.enabled = true;
        buildPlacer.SetDestroyMode(true);
    }

    private void RefreshEquippedItem()
    {
        if (EquipItemRoot == null || !EquipItemRoot.TryGetVisuals(out InventoryItemVisuals visuals)) return;

        visuals.Bind(equippedItem ?? emptyEquippedItem, this);
        OnEquippedItemChanged?.Invoke(equippedItem);
    }

    #endregion

    #region Time Management Methods

    public void ClockUpdate(GameTimestamp timestamp)
    {
        int hours = timestamp.hour;
        int minutes = timestamp.minute;
        string prefix = "AM";

        if (hours > 12)
        {
            prefix = "PM";
            hours -= 12;
        }

        TimePrefix.Text = prefix;
        TimeText.Text = hours.ToString("00") + ":" + minutes.ToString("00");
        DayText.Text = timestamp.day.ToString();
    }

    private void OnDisable() => TimeManager.Instance?.UnregisterTracker(this);

    #endregion

    #region Information Tip Methods

    public void ShowInformationTip(InformationTipSO tip)
    {
        if (tip == null)
            return;

        if (!shownInformtationTips.Add(tip))
            return;

        OnInformationTipRequested?.Invoke(tip);
    }

    #endregion
}