using System;
using UnityEngine;

namespace WILCommunityGame
{
    public class LandManager : MonoBehaviour, IInteractable, ITimeTracker
    {
        [Header("Icons")] 
        [SerializeField] private Sprite seedIcon;
        [SerializeField] private Sprite waterIcon;

        [Header("Information Tips")] 
        [SerializeField] private InformationTipSO wateringTip;
        
        [Header("Objectives")]
        [SerializeField] private ObjectiveItem plantSeedObjective;
        [SerializeField] private ObjectiveItem waterCropObjective;
        [SerializeField] private ObjectiveItem harvestCropObjective;
        
        private CropBehaviour cropBehaviour;
        private UIManager uiManager;
        private IndicatorManager indicatorManager;
        private GameObject pendingSwapPrefab;
        private ObjectiveListUI objectiveListUI;

        private bool firstWatering = true;

        private void Awake()
        {
            cropBehaviour ??= GetComponent<CropBehaviour>();
            uiManager ??= FindFirstObjectByType<UIManager>();
            indicatorManager ??= GetComponentInChildren<IndicatorManager>();
            objectiveListUI ??= FindFirstObjectByType<ObjectiveListUI>();
        }

        private void Start()
        {
            RefreshIndicator();
        }

        private void OnEnable() => TimeManager.Instance?.RegisterTracker(this);
        private void OnDisable() => TimeManager.Instance?.UnregisterTracker(this);

        private void LateUpdate()
        {
            if (pendingSwapPrefab == null) return;

            GameObject nextPlot =
                Instantiate(pendingSwapPrefab, transform.position, transform.rotation, transform.parent);
            nextPlot.GetComponent<CropBehaviour>().CopyStateFrom(cropBehaviour);
            
            Destroy(gameObject);
        }

        public void Interact(PlayerController interactor)
        {
            if (cropBehaviour.IsHarvestable && cropBehaviour.SeedItem.produceItem != null)
            {
                //Add produce to inventory
                uiManager.AddItemToInventory(cropBehaviour.SeedItem.produceItem, cropBehaviour.SeedItem.harvestAmount);
                objectiveListUI?.CompleteItem(harvestCropObjective);
                //reset back to sprout
                cropBehaviour.ResetToSprout();
                pendingSwapPrefab = cropBehaviour.GetCurrentPlotPrefab();
                RefreshIndicator();
                return;
            }

            InventoryItem equipped = uiManager.EquippedItem;
            if (equipped == null || equipped.isEmpty) return;

            if (equipped.IsSeed && cropBehaviour.CanPlant(equipped.Seed))
            {
                SeedItemSO plantedSeed = equipped.Seed;

                if (uiManager.TryUseEquippedItem(1))
                {
                    uiManager.ShowInformationTip(plantedSeed.informationTip);

                    objectiveListUI?.CompleteItem(plantSeedObjective);
                    AudioManager.Instance.Play("Planting");
                    RefreshIndicator();
                }

                return;
            }

            if (equipped.IsWateringCan && cropBehaviour.NeedsWater && equipped.TryUseWater() && cropBehaviour.CanWater())
            {
                pendingSwapPrefab = cropBehaviour.GetCurrentPlotPrefab();
                
                if (wateringTip != null && firstWatering)
                {
                    uiManager.ShowInformationTip(wateringTip);
                }
                objectiveListUI?.CompleteItem(waterCropObjective);
                AudioManager.Instance.Play("Watering");
                RefreshIndicator();
                firstWatering = false;
            }
        }

        public void ClockUpdate(GameTimestamp timestamp)
        {
            if (!cropBehaviour.Grow()) return;

            pendingSwapPrefab = cropBehaviour.GetCurrentPlotPrefab();
            RefreshIndicator();
        }

        private void RefreshIndicator()
        {
            if (indicatorManager == null) return;
            Sprite iconToShow = null;
            Material iconToShowMAT = null;

            if (cropBehaviour.NeedsSeed)
            {
                iconToShow = seedIcon;
            }
            else if (cropBehaviour.NeedsWater)
            {
                iconToShow = waterIcon;
            }
            else if (cropBehaviour.IsHarvestable)
            {
                iconToShow = cropBehaviour.HarvestIcon;
            }
            
            indicatorManager.icon = iconToShow;
            
            if (iconToShow != null)
                indicatorManager.ShowIndictor();
            else 
                indicatorManager.HideIndictor();
        }
    }
}
