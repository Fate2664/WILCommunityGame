using System;
using Nova;
using UnityEngine;

namespace WILCommunityGame
{
    public class WaterTankCapacityBar : MonoBehaviour
    {
        [SerializeField] private RainTank RainTank;
        [SerializeField] private UIBlock2D Background;
        [SerializeField] private UIBlock2D Fillbar;

        private void OnEnable()
        {
            if (RainTank != null)
                RainTank.OnWaterAmountChanged += UpdateBar;
        }

        private void OnDisable()
        {
            if (RainTank != null)
                RainTank.OnWaterAmountChanged -= UpdateBar;
        }

        private void Start()
        {
            if (RainTank != null)
                UpdateBar(RainTank.WaterAmount, RainTank.Capacity);
        }

        private void UpdateBar(int raintTankWaterAmount, int raintTankCapacity)
        {
            float percentage = Mathf.Clamp01((float)raintTankWaterAmount / raintTankCapacity);
            Fillbar.AutoSize.X = AutoSize.None;
            Fillbar.Size.X.Percent = percentage;
        }
    }
}
