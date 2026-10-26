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
            Harvestable
        }

        [SerializeField] private GameObject wateredPrefab;
        [SerializeField] private GameObject sproutPrefab;
        [SerializeField] private CropStage stage = CropStage.Dry;
        [SerializeField] private SeedItemSO seedItem;

        private float growthMinutes;
        private int maxGrowthMinutes;
        private int fertilizerMinutesRemaining;
        private float fertilizerGrowthMultiplier = 1f;
        private GameObject fertilizerGlow;
        
        //State checks
        public CropStage Stage => stage;
        public SeedItemSO SeedItem => seedItem;
        public bool IsPlanted => seedItem != null;
        public bool NeedsSeed => !IsPlanted;
        public bool NeedsWater => IsPlanted && stage == CropStage.Dry;
        public bool IsHarvestable => IsPlanted && stage == CropStage.Harvestable;
        public bool IsFertilized => fertilizerMinutesRemaining > 0;
        public bool CanFertilize => IsPlanted && !IsFertilized;
        public Sprite HarvestIcon => IsHarvestable ? seedItem?.produceItem?.itemDesc?.Icon : null;
        
        
        public bool CanPlant(SeedItemSO seed)
        {
            if (IsPlanted) return false;
            
            seedItem = seed;
            growthMinutes = 0;
            maxGrowthMinutes = GameTimestamp.HoursToMinutes(GameTimestamp.DaysToHours(seed.daysToGrow));
            stage = CropStage.Dry;
            return true;
        }

        public bool CanWater()
        {
            if (!IsPlanted || stage != CropStage.Dry) return false;
            stage = CropStage.Watered;
            return true;
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
            float growthAmount = IsFertilized ? fertilizerGrowthMultiplier : 1f;
            UpdateFertilizer();

            if (!IsPlanted || stage == CropStage.Dry || stage == CropStage.Harvestable) return false;
            
            CropStage previous = stage;
            growthMinutes += growthAmount;

            if (growthMinutes >= maxGrowthMinutes && stage == CropStage.Seedling)
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
                CropStage.Sprout => sproutPrefab,
                CropStage.Seedling => seedItem.seedlingPrefab,
                CropStage.Harvestable => seedItem.harvestablePrefab,
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

        public void ResetToSprout()
        {
            stage = CropStage.Sprout;
            growthMinutes = maxGrowthMinutes / 3;
        }
        
    }
}
