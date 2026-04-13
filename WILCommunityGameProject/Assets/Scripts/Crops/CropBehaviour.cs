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
        
        
        private GameObject seedlingPrefab; 
        private GameObject harvestablePrefab;
        public CropStage stage = CropStage.Dry;
        
        private SeedItemSO seedItem;
        private int growth;
        private int maxGrowth;

        public void Plant(SeedItemSO seedItem)
        {
            this.seedItem = seedItem;
            sproutPrefab = Instantiate(seedItem.seedlingPrefab, transform);
            harvestablePrefab = Instantiate(seedItem.harvestablePrefab, transform);
            int hoursToGrow = GameTimestamp.DaysToHours(seedItem.daysToGrow);
            maxGrowth = GameTimestamp.HoursToMinutes(hoursToGrow);
            
            SwitchStage(CropStage.Watered);
        }

        public void Grow()
        {
            growth++;

            if (growth >= maxGrowth / 3 && stage == CropStage.Watered)
            {
                SwitchStage(CropStage.Sprout);
            }
            
            if (growth >= maxGrowth / 2 && stage == CropStage.Sprout)
            {
                SwitchStage(CropStage.Seedling);    
            }
            
            if (growth >= maxGrowth && stage == CropStage.Seedling)
            {
                SwitchStage(CropStage.Harvestable);
            }
        }

        private void SwitchStage(CropStage nextStage)
        {
            sproutPrefab.gameObject.SetActive(false);
            seedlingPrefab.gameObject.SetActive(false);
            harvestablePrefab.gameObject.SetActive(false);

            switch (nextStage)
            {
                case CropStage.Sprout:
                    sproutPrefab.gameObject.SetActive(true);
                    break;
                case CropStage.Seedling:
                    seedlingPrefab.gameObject.SetActive(true);
                    break;
                case CropStage.Harvestable:
                    harvestablePrefab.gameObject.SetActive(true);
                    break;
            }
            stage = nextStage;
        }
        
    }
}
