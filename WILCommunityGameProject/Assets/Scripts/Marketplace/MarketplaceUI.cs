using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

namespace WILCommunityGame
{
    public class MarketplaceUI : MonoBehaviour
    {
        private bool uiOpen = false;
        
        public void ToggleMarketplaceUI()
        {
            if (uiOpen)
            {
                transform.DOScale(0f, 0.35f).SetEase(Ease.OutCubic);
                uiOpen = false;
            }
            else
            {
                transform.DOScale(1f, 0.35f).SetEase(Ease.OutCubic);
                uiOpen = true;
            }
        }
    }
}
