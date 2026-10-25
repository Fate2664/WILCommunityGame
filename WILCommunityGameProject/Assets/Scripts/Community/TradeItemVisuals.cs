using System;
using Nova;

namespace WILCommunityGame
{
    [Serializable]
    public class TradeItemVisuals : ItemVisuals
    {
        public UIBlock2D Icon;
        public TextBlock CountText;

        public void Bind(TradeItem item)
        {
            Icon.SetImage(item.Item.itemDesc.Icon);
            CountText.Text = item.Amount.ToString();
        }
    }
}
