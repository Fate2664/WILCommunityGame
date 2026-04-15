using System;
using UnityEngine;

namespace WILCommunityGame
{
    public class HouseSleep : MonoBehaviour, IInteractable
    {
        [SerializeField] private PlayerInteractionDetector playerInteractionDetector;
        [SerializeField] private int sleepHours = 12;

        private IndicatorManager indicatorManager;

        private void Awake()
        {
            indicatorManager = GetComponentInChildren<IndicatorManager>();
        }

        private void FixedUpdate()
        {
            if (indicatorManager == null) return;

            if (playerInteractionDetector.CurrentTarget != null &&
                playerInteractionDetector.CurrentIteractableObject != null &&
                playerInteractionDetector.CurrentIteractableObject.CompareTag("House"))
            {
                indicatorManager.ShowIndictor();
            }
            else
            {
                indicatorManager.HideIndictor();
            }
        }

        public void Interact(PlayerController interactor)
        {
            TimeManager.Instance.Sleep(sleepHours);
        }
    }
}