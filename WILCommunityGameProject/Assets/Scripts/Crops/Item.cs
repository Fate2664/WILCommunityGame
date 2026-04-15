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
        [TextArea(3, 10)]
        public string ToolTip;
        public Sprite Icon;
    }

    [System.Serializable]
    public class InventoryItem
    {
        public InventoryItemData item;
        public const int maxCount = 99;
        [HideInInspector]
        public int count;

        public event Action OnCountDecreased;
        public bool isEmpty => item == null;
        public bool IsTool => item is ToolItemSO;
        public bool IsSeed => item is SeedItemSO;
        public bool IsProduce => item is ProduceItemSO;
        ToolItemSO Tool => item as ToolItemSO;
        public SeedItemSO Seed => item as SeedItemSO;
        public ProduceItemSO Produce => item as ProduceItemSO;

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
    }

    public abstract class InventoryItemData : ScriptableObject
    {
        public ItemDescription itemDesc;
    }
    
}
