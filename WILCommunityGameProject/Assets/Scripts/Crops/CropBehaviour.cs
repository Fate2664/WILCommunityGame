using Unity.VisualScripting;
using UnityEngine;

namespace WILCommunityGame
{
    public class CropBehaviour : MonoBehaviour
    {
        public enum CropStage
        {
            Dry,
            Watered,
            Sprout,
            Seedling,
            Harvestable,
            Dead,
            Regrowing
        }

        [SerializeField] private GameObject wateredPrefab;
        [SerializeField] private GameObject sproutPrefab;
        [SerializeField] private CropStage stage = CropStage.Dry;
        [SerializeField] private SeedItemSO seedItem;

        private float growthMinutes;
        private int maxGrowthMinutes;
        private int waterMinutesRemaining;
        private int dryMinutes;
        private int fertilizerMinutesRemaining;
        private float fertilizerGrowthMultiplier = 1f;
        private GameObject fertilizerGlow;
        
        //State checks
        public CropStage Stage => stage;
        public SeedItemSO SeedItem => seedItem;
        public bool IsPlanted => seedItem != null;
        public bool NeedsSeed => !IsPlanted;
        public bool NeedsWater => IsPlanted && !IsDead && waterMinutesRemaining <= 0 &&
                                  (!seedItem.onlyWaterOnce || stage == CropStage.Dry);
        public bool IsHarvestable => IsPlanted && stage == CropStage.Harvestable;
        public bool IsDead => stage == CropStage.Dead;
        public bool IsFertilized => fertilizerMinutesRemaining > 0;
        public bool CanFertilize => IsPlanted && !IsDead && !IsFertilized;
        public Sprite HarvestIcon => IsHarvestable ? seedItem?.produceItem?.itemDesc?.Icon : null;
        private void Awake()
        {
            // Inspector-assigned seeds bypass CanPlant, but need the same growth duration.
            InitializeGrowthDuration();
        }

        private void InitializeGrowthDuration()
        {
            maxGrowthMinutes = seedItem == null ? 0 :
                GameTimestamp.HoursToMinutes(GameTimestamp.DaysToHours(Mathf.Max(1, seedItem.daysToGrow)));
        }

        public bool CanPlant(SeedItemSO seed)
        {
            if (IsPlanted || seed == null) return false;
            
            seedItem = seed;
            growthMinutes = 0;
            InitializeGrowthDuration();
            waterMinutesRemaining = 0;
            dryMinutes = 0;
            stage = CropStage.Dry;
            return true;
        }

        public bool CanWater()
        {
            if (!NeedsWater) return false;

            waterMinutesRemaining = GameTimestamp.HoursToMinutes(GameTimestamp.DaysToHours(2));
            dryMinutes = 0;

            if (stage == CropStage.Dry)
                stage = CropStage.Watered;

            return true;
        }

        private void UpdateWater()
        {
            if (seedItem.onlyWaterOnce && stage != CropStage.Dry)
                return;

            if (waterMinutesRemaining > 0)
            {
                waterMinutesRemaining--;
                return;
            }

            dryMinutes++;

            if (dryMinutes >= GameTimestamp.HoursToMinutes(GameTimestamp.DaysToHours(1)))
            {
                stage = CropStage.Dead;
                ClearFertilizer();
            }
        }

        public bool ApplyFertilizer(ToolItemSO fertilizer)
        {
            if (!CanFertilize || fertilizer == null || fertilizer.toolType != ToolType.Fertilizer)
                return false;

            fertilizerMinutesRemaining = GameTimestamp.HoursToMinutes(GameTimestamp.DaysToHours(Mathf.Max(1, fertilizer.fertilizerDays)));
            fertilizerGrowthMultiplier = Mathf.Max(1f, fertilizer.fertilizerGrowthMultiplier);

            if (fertilizer.fertilizerGlowPrefab != null)
            {
                fertilizerGlow = Instantiate(fertilizer.fertilizerGlowPrefab, transform.position, transform.rotation, transform);
            }

            return true;
        }

        private void UpdateFertilizer()
        {
            if (!IsFertilized)
                return;

            fertilizerMinutesRemaining--;

            if (IsFertilized)
                return;

            ClearFertilizer();
        }

        private void ClearFertilizer()
        {
            fertilizerMinutesRemaining = 0;
            fertilizerGrowthMultiplier = 1f;

            if (fertilizerGlow != null)
            {
                fertilizerGlow.SetActive(false);
                Destroy(fertilizerGlow);
                fertilizerGlow = null;
            }
        }

        public bool Grow()
        {
            if (!IsPlanted || IsDead) return false;

            CropStage previous = stage;
            UpdateWater();

            float growthAmount = IsFertilized ? fertilizerGrowthMultiplier : 1f;
            UpdateFertilizer();

            if (IsDead || stage == CropStage.Dry || stage == CropStage.Harvestable ||
                (NeedsWater && (stage == CropStage.Seedling || stage == CropStage.Regrowing)))
                return stage != previous;
            
            growthMinutes += growthAmount;

            if (growthMinutes >= maxGrowthMinutes &&
                (stage == CropStage.Seedling || stage == CropStage.Regrowing) && !NeedsWater)
                stage = CropStage.Harvestable;
            else if (growthMinutes >= maxGrowthMinutes / 2 && stage == CropStage.Sprout)
                stage = CropStage.Seedling;
            else if (growthMinutes >= maxGrowthMinutes / 3 && stage == CropStage.Watered)
                stage = CropStage.Sprout;
            
            return stage != previous;
        }

        public GameObject GetCurrentPlotPrefab()
        {
            if (!IsPlanted) return null;

            return stage switch
            {
                CropStage.Watered => wateredPrefab,
                CropStage.Sprout => seedItem.sproutPrefab != null ? seedItem.sproutPrefab : sproutPrefab,
                CropStage.Seedling => seedItem.seedlingPrefab,
                CropStage.Harvestable => seedItem.harvestablePrefab,
                CropStage.Regrowing => seedItem.regrowingPrefab,
                CropStage.Dead => seedItem.deadPrefab,
                _ => null
            };
        }

        public void CopyStateFrom(CropBehaviour other)
        {
            wateredPrefab = other.wateredPrefab;
            sproutPrefab = other.sproutPrefab;
            seedItem = other.seedItem;
            growthMinutes = other.growthMinutes;
            maxGrowthMinutes = other.maxGrowthMinutes;
            waterMinutesRemaining = other.waterMinutesRemaining;
            dryMinutes = other.dryMinutes;
            stage = other.stage;
            fertilizerMinutesRemaining = other.fertilizerMinutesRemaining;
            fertilizerGrowthMultiplier = other.fertilizerGrowthMultiplier;
            fertilizerGlow = other.fertilizerGlow;

            if (fertilizerGlow != null)
            {
                fertilizerGlow.transform.SetParent(transform, false);
                other.fertilizerGlow = null;
            }
        }

        public void ResetAfterHarvest()
        {
            stage = seedItem.regrowingPrefab != null ? CropStage.Regrowing : CropStage.Sprout;
            growthMinutes = maxGrowthMinutes / 3;
        }
        
    }
}
