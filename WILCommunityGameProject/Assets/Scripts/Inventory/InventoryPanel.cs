using Nova;
using UnityEngine;
using System.Collections.Generic;
using WILCommunityGame;


public class InventoryPanel : MonoBehaviour
{
    public ItemDatabase ItemDatabase = null;

    [Header("Upgrades")]
    public GridView Grid = null;
    public int Count = 24;

    [Header("Row Styling")] [SerializeField]
    private int padding = 10;

    [HideInInspector]
    private List<InventoryItem> Items = null;

    private void Start()
    {
        Items = ItemDatabase.GetEmptyItems(Count);
        InitGrid(Grid, Items);
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
    }

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
    private void BindItem(Data.OnBind<InventoryItem> evt, InventoryItemVisuals target, int index) => target.Bind(evt.UserData);
}
