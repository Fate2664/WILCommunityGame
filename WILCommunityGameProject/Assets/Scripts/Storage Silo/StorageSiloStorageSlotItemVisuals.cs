using DG.Tweening;
using System;
using Nova;
using UnityEngine;

namespace WILCommunityGame
{
    [Serializable]
    public class StorageSiloStorageSlotItemVisuals : ItemVisuals
    {
        public UIBlock2D background;
        public UIBlock2D Icon;
        public TextBlock CountText;

        public Color DefaultColor = new (0.97f, 0.82f, 0.65f);
        public Color HoverColor = new (0.97f, 0.82f, 0.65f);
        public Color SelectedColor = new (0.65f, 0.85f, 0.45f);
        
        public float hoverScale = 1.05f;
        public float pressedScale = 0.98f;
        public float animationDuration = 0.15f;
        
        private Vector3 defaultScale;
        private Color defaultTextColor;
        private bool defaultBodyEnabled;
        private bool isHovered;
        private bool initialized;
        private bool isSelected;

        public void Bind(InventoryItem crop, bool selected)
        {
            if (!initialized)
            {
                CacheDefaults();
                RegisterGestureHandlers();
                initialized = true;
            }

            // GridView reuses visuals, including when an occupied slot becomes empty.
            isSelected = selected;
            isHovered = false;
            background.transform.DOKill();
            background.transform.localScale = defaultScale;
            background.BodyEnabled = defaultBodyEnabled;

            bool hasItem = crop != null && !crop.isEmpty && crop.count > 0;
            if (Icon != null)
            {
                Icon.gameObject.SetActive(hasItem);
                if (hasItem)
                    Icon.SetImage(crop.item.itemDesc.Icon);
            }

            if (CountText != null)
            {
                CountText.gameObject.SetActive(hasItem);
                CountText.Text = hasItem ? crop.count.ToString() : string.Empty;
                CountText.Color = defaultTextColor;
            }

            RefreshBackgroundColor();
        }

        private void RefreshBackgroundColor()
        {
            background.Color = isSelected ? SelectedColor : isHovered ? HoverColor : DefaultColor;
        }

        private void CacheDefaults()
        {
            defaultScale = background.transform.localScale;
            defaultBodyEnabled = background.BodyEnabled;

            if (CountText != null)
            {
                defaultTextColor = CountText.Color;
            }
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

            if (CountText != null)
            {
                CountText.Color = defaultTextColor;
            }
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
