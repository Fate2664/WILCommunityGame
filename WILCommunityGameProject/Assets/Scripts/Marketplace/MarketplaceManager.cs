using System;
using Unity.VisualScripting;
using UnityEngine;

namespace WILCommunityGame
{
    public class MarketplaceManager : MonoBehaviour, IInteractable
    {
        [SerializeField] private PlayerInteractionDetector playerInteractionDetector;
        [SerializeField] private MarketplaceUI marketplaceUI;
        private IndicatorManager indicatorManager;

        private void Awake()
        {
            indicatorManager = GetComponentInChildren<IndicatorManager>();
        }

        private void FixedUpdate()
        {
            if (playerInteractionDetector.CurrentTarget != null &&
                playerInteractionDetector.CurrentIteractableObject != null &&
                playerInteractionDetector.CurrentIteractableObject.CompareTag("Marketplace"))
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
            marketplaceUI.ToggleMarketplaceUI();
        }
    }
}
