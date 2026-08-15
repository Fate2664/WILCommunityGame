using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace WILCommunityGame
{
    public enum ToolType
    {
        Hoe,
        WateringCan
    }

    public enum SeedType
    {
        Potato,
        Carrot,
        Cabbage,
        Tomato,
        Corn
    }

    public enum ProduceType
    {
        Potato,
        Carrot,
        Cabbage,
        Tomato,
        Corn
    }

    [System.Serializable]
    public class ItemDescription
    {
        public string Name;
        [TextArea(3, 10)] public string ToolTip;
        public Sprite Icon;
    }

    [System.Serializable]
    public class InventoryItem
    {
        public InventoryItemData item;
        public const int maxCount = 99;
        [HideInInspector] public int count;
        private int waterAmount;

        public event Action OnCountDecreased;
        public bool isEmpty => item == null;
        public bool IsTool => item is ToolItemSO;
        public bool IsSeed => item is SeedItemSO;
        public bool IsProduce => item is ProduceItemSO;
        ToolItemSO Tool => item as ToolItemSO;
        public SeedItemSO Seed => item as SeedItemSO;
        public ProduceItemSO Produce => item as ProduceItemSO;
        public bool IsWateringCan => item is ToolItemSO tool && tool.toolType == ToolType.WateringCan;
        public int WaterCapacity => item is ToolItemSO tool && tool.toolType == ToolType.WateringCan ? tool.waterCapacity : 0;
        public int WaterAmount => waterAmount;

        public void IncreaseCount(int amount)
        {
            count = Mathf.Min(count + amount, maxCount);
        }

        public void DecreaseCount(int amount)
        {
            int previousCount = count;
            count = Mathf.Max(0, count - amount);

            if (count < previousCount)
            {
                OnCountDecreased?.Invoke();
            }
        }

        public int FillWater(int availableWater)
        {
            if (!IsWateringCan || availableWater <= 0)
                return 0;

            int addedWater = Mathf.Min(availableWater, WaterCapacity - waterAmount);
            waterAmount += addedWater;
            return addedWater;
        }

        public bool TryUseWater(int amount = 1)
        {
            if (!IsWateringCan || waterAmount < amount) 
                return false;
            
            waterAmount -= amount;
            return true;
        }
    }

    public abstract class InventoryItemData : ScriptableObject
    {
        public ItemDescription itemDesc;
    }
}