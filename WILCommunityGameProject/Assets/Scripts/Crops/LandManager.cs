using System;
using System.Collections.Generic;
using UnityEngine;

namespace WILCommunityGame
{
    public class LandManager : MonoBehaviour, IInteractable, ITimeTracker, IBuildPreview
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
        private static readonly HashSet<LandManager> activePlots = new();
        private Collider plotCollider;
        private bool isPreview;

        private bool firstWatering = true;

        private void Awake()
        {
            cropBehaviour ??= GetComponent<CropBehaviour>();
            plotCollider = GetComponent<Collider>();
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

        private void OnEnable()
        {
            if (isPreview) return;
            activePlots.Add(this);
            RegisterClock();
        }

        public void PrepareAsPreview()
        {
            isPreview = true;
            enabled = false;
        }

        public static bool OverlapsPlot(Vector3 position, float halfExtent, float plotSpacing)
        {
            foreach (LandManager plot in activePlots)
            {
                if (plot == null) continue;

                // Reserve the ground footprint through every stage, even when the
                // collider becomes a narrow tree trunk or the plot is above the terrain.
                Vector3 center = plot.transform.position;
                float extentX = plotSpacing * 0.5f;
                float extentZ = extentX;
                if (plot.plotCollider != null && plot.plotCollider.enabled)
                {
                    Bounds bounds = plot.plotCollider.bounds;
                    center = bounds.center;
                    extentX = Mathf.Max(extentX, bounds.extents.x);
                    extentZ = Mathf.Max(extentZ, bounds.extents.z);
                }

                if (Mathf.Abs(position.x - center.x) < halfExtent + extentX &&
                    Mathf.Abs(position.z - center.z) < halfExtent + extentZ)
                    return true;
            }

            return false;
        }

        private void RegisterClock()
        {
            if (isPreview || timeManager != null || TimeManager.Instance == null)
                return;

            timeManager = TimeManager.Instance;
            timeManager.RegisterTracker(this);
        }

        private void OnDisable()
        {
            activePlots.Remove(this);
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
                SeedItemSO harvestedSeed = cropBehaviour.SeedItem;
                uiManager.AddItemToInventory(harvestedSeed.produceItem, harvestedSeed.harvestAmount);

                // Roll once per harvest for five bonus crop waste items (25% chance).
                if (harvestedSeed.cropWasteItem != null && UnityEngine.Random.Range(0, 4) == 0)
                    uiManager.AddItemToInventory(harvestedSeed.cropWasteItem, 5);

                objectiveListUI?.CompleteItem(harvestCropObjective);
                cropBehaviour.ResetAfterHarvest();
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
            if (isPreview) return false;

            CropBehaviour.CropStage previous = cropBehaviour.Stage;
            if (!cropBehaviour.CanWater()) return false;

            if (cropBehaviour.Stage != previous)
                pendingSwapPrefab = cropBehaviour.GetCurrentPlotPrefab();

            RefreshIndicator();
            return true;
        }

        private void TryAutomaticWatering()
        {
            if (isPreview || !cropBehaviour.NeedsWater) return;

            bool isRaining = WeatherManager.Instance != null && WeatherManager.Instance.IsRaining;
            if (isRaining || Sprinkler.IsPlotCovered(this))
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
