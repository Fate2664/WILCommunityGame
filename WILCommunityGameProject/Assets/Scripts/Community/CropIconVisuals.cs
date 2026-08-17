using Nova;
using UnityEngine;

namespace WILCommunityGame
{
    public class CropIconVisuals : ItemVisuals
    {
        public UIBlock2D Icon;
        public TextBlock CountText;

        public void Bind(CropRequest request)
        {
            Icon.SetImage(request.Produce.itemDesc.Icon);
            CountText.Text = $"{request.Delivered}/{request.Requested}";
        }
    }
}
