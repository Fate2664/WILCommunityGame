using System;
using Nova;
using UnityEngine;

namespace WILCommunityGame
{
    [Serializable]
    public class MarketplaceCropItemVisuals : ItemVisuals
    {
        public UIBlock2D Background;
        public UIBlock2D Icon;
        public TextBlock CountText;

        public Color DefaultColor = new (0.97f, 0.82f, 0.65f);
        public Color SelectedColor = new (0.65f, 0.85f, 0.45f);

        public void Bind(InventoryItem crop, bool selected)
        {
            Icon.SetImage(crop.item.itemDesc.Icon);
            CountText.Text = crop.count.ToString();
            Background.Color = selected ? SelectedColor : DefaultColor;
        }
    }
}