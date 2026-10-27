using System;
using UnityEngine;

namespace WILCommunityGame
{
    public class LandManager : MonoBehaviour, IInteractable, ITimeTracker
    {
        [SerializeField] private GameObject emptyPlotPrefab;

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
        private TimeManager timeManager;

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
            RegisterClock();
            TryAutomaticWatering();
            RefreshIndicator();
        }

        private void OnEnable() => RegisterClock();

        private void RegisterClock()
        {
            if (timeManager != null || TimeManager.Instance == null)
                return;

            timeManager = TimeManager.Instance;
            timeManager.RegisterTracker(this);
        }

        private void OnDisable()
        {
            timeManager?.UnregisterTracker(this);
            timeManager = null;
        }

        private void LateUpdate()
        {
            if (pendingSwapPrefab == null) return;

            GameObject nextPlot =
                Instantiate(pendingSwapPrefab, transform.position, transform.rotation, transform.parent);
            if (pendingSwapPrefab != emptyPlotPrefab)
            {
                nextPlot.GetComponent<CropBehaviour>().CopyStateFrom(cropBehaviour);
                nextPlot.GetComponent<LandManager>().firstWatering = firstWatering;
            }

            PlotSocket socket = GetComponentInChildren<PlotSocket>();
            PlotSocket nextSocket = nextPlot.GetComponentInChildren<PlotSocket>();
            if (socket != null && socket.Occupant != null && nextSocket != null)
            {
                Sprinkler sprinkler = socket.Occupant.GetComponent<Sprinkler>();
                if (sprinkler != null)
                    sprinkler.AttachToPlot(nextSocket);
            }
            
            Destroy(gameObject);
        }

        public void Interact(PlayerController interactor)
        {
            if (cropBehaviour.IsDead)
            {
                if (emptyPlotPrefab != null)
                    pendingSwapPrefab = emptyPlotPrefab;

                return;
            }

            InventoryItem equipped = uiManager.EquippedItem;

            if (equipped != null && equipped.IsFertilizer)
            {
                ToolItemSO fertilizer = equipped.Tool;

                if (cropBehaviour.CanFertilize && uiManager.TryUseEquippedItem(1))
                    cropBehaviour.ApplyFertilizer(fertilizer);

                return;
            }

            if (equipped != null && equipped.IsWateringCan && cropBehaviour.NeedsWater)
            {
                if (equipped.TryUseWater() && WaterPlot())
                {
                    if (wateringTip != null && firstWatering)
                    {
                        uiManager.ShowInformationTip(wateringTip);
                    }
                    objectiveListUI?.CompleteItem(waterCropObjective);
                    AudioManager.Instance.Play("Watering");
                    firstWatering = false;
                }

                return;
            }

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

            if (equipped == null || equipped.isEmpty) return;

            if (equipped.IsSeed && cropBehaviour.CanPlant(equipped.Seed))
            {
                SeedItemSO plantedSeed = equipped.Seed;

                if (uiManager.TryUseEquippedItem(1))
                {
                    uiManager.ShowInformationTip(plantedSeed.informationTip);

                    objectiveListUI?.CompleteItem(plantSeedObjective);
                    AudioManager.Instance.Play("Planting");
                    TryAutomaticWatering();
                    RefreshIndicator();
                }

                return;
            }
        }

        public bool WaterPlot()
        {
            CropBehaviour.CropStage previous = cropBehaviour.Stage;
            if (!cropBehaviour.CanWater()) return false;

            if (cropBehaviour.Stage != previous)
                pendingSwapPrefab = cropBehaviour.GetCurrentPlotPrefab();

            RefreshIndicator();
            return true;
        }

        private void TryAutomaticWatering()
        {
            if (cropBehaviour.NeedsWater && Sprinkler.IsPlotCovered(this))
                WaterPlot();
        }

        public void ClockUpdate(GameTimestamp timestamp)
        {
            TryAutomaticWatering();
            bool neededWater = cropBehaviour.NeedsWater;
            bool stageChanged = cropBehaviour.Grow();

            if (stageChanged)
                pendingSwapPrefab = cropBehaviour.GetCurrentPlotPrefab();

            TryAutomaticWatering();

            if (stageChanged || neededWater != cropBehaviour.NeedsWater)
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
