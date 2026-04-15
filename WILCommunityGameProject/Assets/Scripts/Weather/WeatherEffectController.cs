using System;
using UnityEngine;

namespace WILCommunityGame
{
    public class WeatherEffectController : MonoBehaviour
    {
        [SerializeField] private GameObject rainParticles;

        private ParticleSystem rainParticleSystem;
        private bool rainParticlesActive;

        private void Awake()
        {
            if (rainParticles != null)
            {
                rainParticleSystem = rainParticles.GetComponent<ParticleSystem>();
                rainParticlesActive = rainParticles.activeSelf;
            }
        }

        public void LoadParticles()
        {
            if (rainParticles == null) return;

            var shouldRain = WeatherManager.Instance.IsRaining;
            if (rainParticlesActive == shouldRain) return;

            if (shouldRain)
            {
                rainParticles.SetActive(true);
                rainParticleSystem.Play();               
            }
            else
            {
                rainParticleSystem.Stop();
                rainParticles.SetActive(false);
            }

            rainParticlesActive = shouldRain;
        }
    }
}
