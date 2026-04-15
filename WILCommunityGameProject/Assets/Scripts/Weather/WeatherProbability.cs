using UnityEngine;

namespace WILCommunityGame
{
    [System.Serializable]
    public struct WeatherProbability
    {
        public WeatherData.WeatherType WeatherType;
        [Range(0f, 1f)]
        public float Probability;
    }
}
