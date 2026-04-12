using UnityEngine;

namespace WILCommunityGame
{
    [System.Serializable]
    public class ItemDescription
    {
        public string Name;
        [TextArea(3, 10)]
        public string ToolTip;
        public Texture2D Icon;
        

    }

    [System.Serializable]
    public class InventoryItem
    {
        public Item item;
        public int count;

        public bool isEmpty => item == null;
    }
    
    [CreateAssetMenu(menuName = "Inventory/Item")]
    public class Item : ScriptableObject
    {
        public ItemDescription itemDesc;
        public const int maxCount = 99;
    }

}
