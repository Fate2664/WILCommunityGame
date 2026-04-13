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

        private int growthMinutes;
        private int maxGrowthMinutes;
        public CropStage Stage => stage;
        public bool IsPlanted => seedItem != null;
        
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

        public bool Grow()
        {
            if (!IsPlanted || stage == CropStage.Dry || stage == CropStage.Harvestable) return false;
            
            CropStage previous = stage;
            growthMinutes++;

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
        }
        
    }
}
