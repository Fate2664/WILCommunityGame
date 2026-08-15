using System;
using Nova;
using UnityEngine;

namespace WILCommunityGame
{
    public class WaterTankCapacityBar : MonoBehaviour
    {
        [SerializeField] private RainTank raintTank;
        [SerializeField] private UIBlock2D Background;
        [SerializeField] private UIBlock2D Fillbar;

        private void OnEnable()
        {
            if (raintTank != null)
                raintTank.OnWaterAmountChanged += UpdateBar;
        }

        private void OnDisable()
        {
            if (raintTank != null)
                raintTank.OnWaterAmountChanged -= UpdateBar;
        }

        private void Start()
        {
            if (raintTank != null)
                UpdateBar(raintTank.WaterAmount, raintTank.Capacity);
        }

        private void UpdateBar(int raintTankWaterAmount, int raintTankCapacity)
        {
            float percentage = Mathf.Clamp01((float)raintTankWaterAmount / raintTankCapacity);
            Fillbar.AutoSize.X = AutoSize.None;
            Fillbar.Size.X.Percent = percentage;
        }
    }
}
