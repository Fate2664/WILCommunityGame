using DG.Tweening;
using Nova;
using UnityEngine;

namespace WILCommunityGame
{   
    [System.Serializable]
    public class UpgradeCategoryItemVisuals : ItemVisuals
    {
        public UIBlock2D background;
        public UIBlock2D Icon;
        public TextBlock NameText;

        public Color DefaultColor = new(0.97f, 0.82f, 0.65f);
        public Color HoverColor = new (0.97f, 0.82f, 0.65f);
        public Color SelectedColor = Color.white;
        public Color SelectedTextColor = Color.white;
        public Color DefaultTextColor = Color.white;
        
        public float hoverScale = 1.05f;
        public float pressedScale = 0.98f;
        public float animationDuration = 0.15f;
        
        private Vector3 defaultScale;
        private Color defaultBackgroundColor;
        private bool defaultBodyEnabled;
        private bool isHovered;
        private bool initialized;
        private bool isSelected;

        public void Bind(MarketplaceUpgradeCategory category, bool selected)
        {
            if (!initialized)
            {
                CacheDefaults();
                RegisterGestureHandlers();
                initialized = true;
            }
            
            isSelected = selected;
            
            Icon.SetImage(category.icon);
            NameText.Text = category.displayName;
            NameText.Color = selected ? SelectedTextColor : DefaultTextColor;
            
            RefreshBackgroundColor();
        }
        
         private void RefreshBackgroundColor()
        {
            background.Color = isSelected ? SelectedColor : isHovered ? HoverColor : DefaultColor;
        }

        private void CacheDefaults()
        {
            defaultScale = background.transform.localScale;
            defaultBackgroundColor = background.Color;
            defaultBodyEnabled = background.BodyEnabled;
        }

        private void RegisterGestureHandlers()
        {
            background.AddGestureHandler<Gesture.OnHover>(HandleHover);
            background.AddGestureHandler<Gesture.OnUnhover>(HandleUnhover);
            background.AddGestureHandler<Gesture.OnPress>(HandlePress);
            background.AddGestureHandler<Gesture.OnRelease>(HandleRelease);
            background.AddGestureHandler<Gesture.OnCancel>(HandleCancel);
        }

        private void HandleHover(Gesture.OnHover evt)
        {
            isHovered = true;
            background.BodyEnabled = true;
            RefreshBackgroundColor();

            //AudioManager.Instance.Play("HoverSound");
            AnimateScale(defaultScale * hoverScale, Ease.OutBack);
        }

        private void HandleUnhover(Gesture.OnUnhover evt)
        {
            isHovered = false;
            background.BodyEnabled = defaultBodyEnabled;
            RefreshBackgroundColor();

            AnimateScale(defaultScale, Ease.OutQuad);
        }

        private void HandlePress(Gesture.OnPress evt)
        {
            //AudioManager.Instance.Play("ClickSound");
            AnimateScale(defaultScale * pressedScale, Ease.OutQuad);
        }

        private void HandleRelease(Gesture.OnRelease evt)
        {
            AnimateScale(defaultScale * (isHovered ? hoverScale : 1f), Ease.OutBack);
        }

        private void HandleCancel(Gesture.OnCancel evt)
        {
            AnimateScale(defaultScale * (isHovered ? hoverScale : 1f), Ease.OutQuad);
        }

        private void AnimateScale(Vector3 targetScale, Ease ease)
        {
            background.transform.DOKill();
            background.transform
                .DOScale(targetScale, animationDuration)
                .SetEase(ease)
                .SetUpdate(true);
        }
    }
}