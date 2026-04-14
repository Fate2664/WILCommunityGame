using System;
using UnityEngine;

namespace WILCommunityGame
{
    public class LandManager : MonoBehaviour, IInteractable, ITimeTracker
    {
        [Header("Icons")] [SerializeField] private Sprite seedIcon;
        [SerializeField] private Sprite waterIcon;
        
        private CropBehaviour cropBehaviour;
        private UIManager uiManager;
        private IndicatorManager indicatorManager;
        private GameObject pendingSwapPrefab;

        private void Awake()
        {
            cropBehaviour ??= GetComponent<CropBehaviour>();
            uiManager ??= FindFirstObjectByType<UIManager>();
            indicatorManager ??= GetComponentInChildren<IndicatorManager>();
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
            InventoryItem equipped = uiManager.EquippedItem;
            if (equipped.isEmpty) return;

            if (equipped.IsSeed && cropBehaviour.CanPlant(equipped.Seed))
            {
                uiManager.TryUseEquippedItem(1);
                RefreshIndicator();
                return;
            }

            if (equipped.IsTool && equipped.item is ToolItemSO tool && tool.toolType == ToolType.WateringCan &&
                cropBehaviour.CanWater())
            {
                pendingSwapPrefab = cropBehaviour.GetCurrentPlotPrefab();
                RefreshIndicator();
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
            
            if (cropBehaviour.NeedsSeed)
                iconToShow = seedIcon;
            else if (cropBehaviour.NeedsWater)
                iconToShow = waterIcon;
            else if (cropBehaviour.IsHarvestable)
                iconToShow = cropBehaviour.HarvestIcon;
            
            indicatorManager.icon = iconToShow;
            
            if (iconToShow != null)
                indicatorManager.ShowIndictor();
            else 
                indicatorManager.HideIndictor();
        }
    }
}