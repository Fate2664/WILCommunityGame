using System;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using Random = System.Random;

namespace WILCommunityGame
{
    public class WeatherManager : MonoBehaviour, ITimeTracker
    {
        public static WeatherManager Instance { get; private set; }

        [SerializeField] private WeatherData weatherData;
        [SerializeField] private Light sun; 
        [SerializeField] private float rainSunIntensity;
        public bool setRain = false;
        
        public WeatherData.WeatherType WeatherToday { get; private set; }
        public WeatherData.WeatherType WeatherTomorrow { get; private set; }
        public bool IsRaining => WeatherToday == WeatherData.WeatherType.Rain;
        private bool weatherSet = false;
        private const float defaultSunIntensity = 2.0f;
        private WeatherEffectController weatherEffectController;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);    
            }
            else
            {
                Instance = this;
            }
            weatherEffectController = GetComponent<WeatherEffectController>();
        }

        private void OnEnable() => TimeManager.Instance?.RegisterTracker(this);
        private void OnDisable() => TimeManager.Instance?.UnregisterTracker(this);

        private void Update()
        {
            if (setRain)
            {
                WeatherToday = WeatherData.WeatherType.Rain;
                ApplyWeatherLight();
            }
            else
            {
                WeatherToday = WeatherData.WeatherType.Sunny;
                ApplyWeatherLight();
            }

            if (weatherEffectController != null)
                weatherEffectController.LoadParticles();
        }

        public WeatherData.WeatherType ComputeWeather()
        {
            if (weatherData == null)
                return WeatherData.WeatherType.Sunny;

            float roll = UnityEngine.Random.Range(0f, 1f);
            float total = 0f;

            foreach (WeatherProbability weatherProbability in weatherData.WeatherProbabilities)
            {
                total += weatherProbability.Probability;
                if (roll <= total)
                {
                    return weatherProbability.WeatherType;
                }
            }
            
            return WeatherData.WeatherType.Sunny;
        }

        private void ApplyWeatherLight()
        {
            if (sun == null) return;
            
            sun.intensity = WeatherToday == WeatherData.WeatherType.Rain ? rainSunIntensity : defaultSunIntensity; 
        }
        
        public void ClockUpdate(GameTimestamp timestamp)
        {
            if (timestamp.hour == 6 && timestamp.minute == 0)
            {
                if (!weatherSet)
                {
                    WeatherToday = ComputeWeather();
                }
                else
                {
                    WeatherToday = WeatherTomorrow;
                }

                WeatherTomorrow = ComputeWeather();
                weatherSet = true;
                ApplyWeatherLight();
            }
        }
    }
}
