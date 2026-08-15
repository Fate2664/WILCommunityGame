using System;
using UnityEngine;

namespace WILCommunityGame
{
    public class RainTank : MonoBehaviour, IInteractable
    {
        [Header("Water Storage")] 
        [SerializeField, Min(1)] private int capacity = 100;  
        [SerializeField, Min(0)] private int currentWaterAmount;  
        
        [Header("Connections")]
        [SerializeField]private UIManager uiManager;
        
        public event Action<int, int> OnWaterAmountChanged;
        public int WaterAmount => currentWaterAmount;
        public int Capacity => capacity;

        private void OnDisable()
        {
            if (WeatherManager.Instance != null)
                WeatherManager.Instance.OnWeatherChange -= HandleWeatherChanged;
        }

        private void Start()
        {
            SetWaterAmount(capacity);
            
            if (WeatherManager.Instance != null)
                WeatherManager.Instance.OnWeatherChange += HandleWeatherChanged;
        }

        private void HandleWeatherChanged(WeatherData.WeatherType weather)
        {
            if (weather == WeatherData.WeatherType.Rain)
                SetWaterAmount(capacity);
        }

        public void Interact(PlayerController interactor)
        {
            if (WaterAmount <= 0) return;
            
            int transferredWater = uiManager.FillEquippedWateringCan(currentWaterAmount);
            SetWaterAmount(currentWaterAmount - transferredWater);
        }

        private void SetWaterAmount(int amount)
        {
            currentWaterAmount = Mathf.Clamp(amount, 0, capacity);
            OnWaterAmountChanged?.Invoke(currentWaterAmount, capacity);
        }

    }
}
