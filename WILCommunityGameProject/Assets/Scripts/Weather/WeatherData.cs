using UnityEngine;

namespace WILCommunityGame
{
    [CreateAssetMenu(menuName = "Weather/Weather Data")]
    public class WeatherData : ScriptableObject
    {
        public enum WeatherType
        {
            Sunny,
            Rain
        }
        
        public WeatherProbability[] WeatherProbabilities;
    }
}
