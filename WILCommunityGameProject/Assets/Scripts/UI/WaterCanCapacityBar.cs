using System;
using DG.Tweening;
using Nova;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

namespace WILCommunityGame
{
    public class WaterCanCapacityBar : MonoBehaviour
    {
        [SerializeField] private UIManager uiManager;
        [SerializeField] private Transform barTransform;
        [SerializeField] private UIBlock2D FillBar;
        [SerializeField] private float enabledXValue;
        [SerializeField] private float slideDuration = 0.35f;

        private InventoryItem wateringCan;
        private float offScreenX;
        private Tween slideTween;

        private void Awake()
        {
            offScreenX = barTransform.localPosition.x;
        }


        private void OnEnable()
        {
            uiManager.OnEquippedItemChanged += HandleEquippedWateringCan;
            HandleEquippedWateringCan(uiManager.EquippedItem);
        }

        private void OnDisable()
        {
            uiManager.OnEquippedItemChanged -= HandleEquippedWateringCan;
            UnsubFromWateringCan();
            slideTween?.Kill();
        }

        private void UnsubFromWateringCan()
        {
            if (wateringCan != null)
                wateringCan.OnWaterAmountChanged -= UpdateBar;
            
            wateringCan = null;
        }

        private void HandleEquippedWateringCan(InventoryItem item)
        {
            if (item != null && item == wateringCan)
            {
                UpdateBar(item.WaterAmount, item.WaterCapacity);
                return;
            }
            
            UnsubFromWateringCan();
            slideTween?.Kill();

            if (item == null || !item.IsWateringCan)
            {
                slideTween = barTransform.DOLocalMoveX(offScreenX, slideDuration).SetEase(Ease.OutCubic);
                return;
            }
            
            wateringCan = item;
            wateringCan.OnWaterAmountChanged += UpdateBar;
            
            UpdateBar(wateringCan.WaterAmount, wateringCan.WaterCapacity);
            barTransform.localPosition = new Vector3(offScreenX, barTransform.localPosition.y, barTransform.localPosition.z);
            slideTween?.Kill();
            slideTween = barTransform.DOLocalMoveX(enabledXValue, slideDuration).SetEase(Ease.OutCubic);
        }

        private void UpdateBar(int waterAmount, int capacity)
        {
            float percentage = capacity > 0 ? Mathf.Clamp01((float)waterAmount / capacity) : 0f;

            FillBar.AutoSize.Y = AutoSize.None;
            FillBar.Size.Y.Percent = percentage;
        }
    }
}
