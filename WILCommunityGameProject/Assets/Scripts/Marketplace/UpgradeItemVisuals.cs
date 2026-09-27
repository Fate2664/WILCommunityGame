using System;
using DG.Tweening;
using Nova;
using UnityEngine;

namespace WILCommunityGame
{
    [Serializable]
    public class UpgradeItemVisuals : ItemVisuals
    {
        public UIBlock2D Icon;
        public TextBlock TitleText;
        public TextBlock DescriptionText;
        public TextBlock CostText;

        
        public UIBlock2D BuyBackground;
        public TextBlock BuyText;
        public Interactable BuyInteractable;
        public Color HoverButtonColor = new (0.97f, 0.82f, 0.65f);

        public Color AvailableColor = new(0.5f, 0.76f, 0.26f);
        public Color DisabledColor = Color.gray;
        
        public float hoverScale = 1.05f;
        public float pressedScale = 0.98f;
        public float animationDuration = 0.15f;
        
        private Vector3 defaultScale;
        private Color defaultBackgroundColor;
        private Color defaultTextColor;
        private bool defaultBodyEnabled;
        private bool isHovered;
        private bool initialized;
        private bool canBuy;
        
        private void CacheDefaults()
        {
            defaultScale = BuyBackground.transform.localScale;
            defaultBackgroundColor = BuyBackground.Color;
            defaultBodyEnabled = BuyBackground.BodyEnabled;
        }

        public void Bind(MarketplaceUpgradeItem upgrade, UpgradePurchaseState state)
        {
            if (!initialized)
            {
                CacheDefaults();
                RegisterGestureHandlers();
                initialized = true;
            }
            
            Icon.SetImage(upgrade.icon);
            TitleText.Text = upgrade.displayName;
            DescriptionText.Text = upgrade.description;
            CostText.Text = upgrade.price.ToString();

            canBuy = state == UpgradePurchaseState.Available;
            BuyInteractable.enabled = canBuy;

            BuyText.Text = state switch
            {
                UpgradePurchaseState.Available => "Buy",
                UpgradePurchaseState.Owned => "Owned",
                UpgradePurchaseState.Locked => "Locked",
                UpgradePurchaseState.InsufficientFunds => "Need coins"
            };
            
            RefreshBackgroundColor();
        }
        
        private void RegisterGestureHandlers()
        {
            BuyBackground.AddGestureHandler<Gesture.OnHover>(HandleHover);
            BuyBackground.AddGestureHandler<Gesture.OnUnhover>(HandleUnhover);
            BuyBackground.AddGestureHandler<Gesture.OnPress>(HandlePress);
            BuyBackground.AddGestureHandler<Gesture.OnRelease>(HandleRelease);
            BuyBackground.AddGestureHandler<Gesture.OnCancel>(HandleCancel);
        }
        
        private void RefreshBackgroundColor()
        {
            BuyBackground.Color = !canBuy ? DisabledColor : isHovered ?  HoverButtonColor : AvailableColor;
        }
        
        private void HandleHover(Gesture.OnHover evt)
        {
            isHovered = true;
            BuyBackground.BodyEnabled = true;
            RefreshBackgroundColor();

            //AudioManager.Instance.Play("HoverSound");
            AnimateScale(defaultScale * hoverScale, Ease.OutBack);
        }

        private void HandleUnhover(Gesture.OnUnhover evt)
        {
            isHovered = false;
            BuyBackground.BodyEnabled = defaultBodyEnabled;
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
            BuyBackground.transform.DOKill();
            BuyBackground.transform
                .DOScale(targetScale, animationDuration)
                .SetEase(ease)
                .SetUpdate(true);
        }
    }
}