using Nova;
using UnityEngine;
using System.Collections.Generic;
using WILCommunityGame;


public class UIManager : MonoBehaviour, ITimeTracker
{
    public ItemDatabase ItemDatabase = null;
    public ItemView[] EquipItemRoots = null;
    public ItemView closeButtonRoot = null;

    [Header("Grid Layout")]
    public GridView Grid = null;
    public int Count = 24;

    [Header("Row Styling")] [SerializeField]
    private int padding = 10;
    
    [Header("Date & Time")]
    [SerializeField] private TextBlock TimeText = null;
    [SerializeField] private TextBlock TimePrefix = null;
    [SerializeField] private TextBlock DayText = null;

    [HideInInspector]
    private List<InventoryItem> Items = null;
    private readonly InventoryItem emptyEquippedItem = new InventoryItem();
    private InventoryItem equippedItem = null;

    private void Start()
    {
        Items = ItemDatabase.GetEmptyItems(Count);
        InitGrid(Grid, Items);
        RegisterStandaloneGestureHandlers();
        RefreshEquippedItem();
        TimeManager.Instance.RegisterTracker(this);
    }

    public void AddItemToInventory(Item item, int count = 1)
    {
        if (item == null) return;
        
        var existing = Items.Find(x => !x.isEmpty && x.item == item);
        if (existing != null && existing.count + count != Item.maxCount)
        {
            existing.count += count; 
        }
        else
        {
            int emptyIndex = Items.FindIndex(x => x.isEmpty);
            if (emptyIndex != -1)
            {
                Items[emptyIndex] = new InventoryItem
                {
                    item = item,
                    count = count
                };
            } 
        }

        if (Grid.gameObject.activeInHierarchy)
        {
            Grid.Refresh();
        }

        if (equippedItem != null && equippedItem.item == item)
        {
            RefreshEquippedItem();
        }
    }


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
        gridslice.AutoLayout.AutoSpace = true;
        gridslice.Layout.Padding.Value = padding;
    }
    
    private void BindItem(Data.OnBind<InventoryItem> evt, InventoryItemVisuals target, int index) => target.Bind(evt.UserData, this);

    private void RegisterStandaloneGestureHandlers()
    {
        if (EquipItemRoots != null)
        {
            foreach (var equipItemRoot in EquipItemRoots)
            {
                equipItemRoot.UIBlock.AddGestureHandler<Gesture.OnHover, InventoryItemVisuals>(InventoryItemVisuals.HandleHover);
                equipItemRoot.UIBlock.AddGestureHandler<Gesture.OnUnhover, InventoryItemVisuals>(InventoryItemVisuals.HandleUnhover);
                equipItemRoot.UIBlock.AddGestureHandler<Gesture.OnPress, InventoryItemVisuals>(InventoryItemVisuals.HandlePress);
                equipItemRoot.UIBlock.AddGestureHandler<Gesture.OnRelease, InventoryItemVisuals>(InventoryItemVisuals.HandleRelease);
            }
        }

        if (closeButtonRoot != null)
        {
            closeButtonRoot.UIBlock.AddGestureHandler<Gesture.OnHover, InventoryButtonVisuals>(InventoryButtonVisuals.HandleHover);
            closeButtonRoot.UIBlock.AddGestureHandler<Gesture.OnUnhover, InventoryButtonVisuals>(InventoryButtonVisuals.HandleUnhover);
            closeButtonRoot.UIBlock.AddGestureHandler<Gesture.OnPress, InventoryButtonVisuals>(InventoryButtonVisuals.HandlePress);
            closeButtonRoot.UIBlock.AddGestureHandler<Gesture.OnRelease, InventoryButtonVisuals>(InventoryButtonVisuals.HandleRelease);
        }
    }

    #endregion

    #region Equip Item Methods

    public void EquipItem(InventoryItem item)
    {
        equippedItem = item != null && !item.isEmpty ? item : null;
        RefreshEquippedItem();
    }
    
    private void RefreshEquippedItem()
    {
        if (EquipItemRoots == null || !EquipItemRoots[0].TryGetVisuals(out InventoryItemVisuals visuals)) return;
        
        visuals.Bind(equippedItem ?? emptyEquippedItem);
    }

    #endregion

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
        TimeText.Text = hours.ToString("00") +  ":" + minutes.ToString("00");
    }
}
