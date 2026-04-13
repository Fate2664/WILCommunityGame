using System;
using UnityEngine;

namespace WILCommunityGame
{
    public class LandManager : MonoBehaviour, IInteractable, ITimeTracker
    {
        private CropBehaviour cropBehaviour;
        private UIManager uiManager;
        private GameObject pendingSwapPrefab;

        private void Awake()
        {
            cropBehaviour ??=  GetComponent<CropBehaviour>();
            uiManager ??=  FindFirstObjectByType<UIManager>();
        }

        private void OnEnable() => TimeManager.Instance?.RegisterTracker(this);
        private void OnDisable() => TimeManager.Instance?.UnregisterTracker(this);

        private void LateUpdate()
        {
            if (pendingSwapPrefab == null) return;
            
            GameObject nextPlot = Instantiate(pendingSwapPrefab, transform.position, transform.rotation, transform.parent);
            
            nextPlot.transform.localScale = transform.localScale;
            nextPlot.GetComponent<CropBehaviour>().CopyStateFrom(cropBehaviour);
            nextPlot.GetComponent<LandManager>().uiManager = uiManager;
            
            Destroy(gameObject);
        }

        public void Interact(PlayerController interactor)
        {
            InventoryItem equipped = uiManager.EquippedItem;
            if (equipped.isEmpty) return;

            if (equipped.IsSeed && cropBehaviour.CanPlant(equipped.Seed))
            {
                uiManager.TryUseEquippedItem(1);
                return;
            }

            if (equipped.IsTool && equipped.item is ToolItemSO tool && tool.toolType == ToolType.WateringCan && cropBehaviour.CanWater())
            {
                pendingSwapPrefab = cropBehaviour.GetCurrentPlotPrefab();
            }
        }

        public void ClockUpdate(GameTimestamp timestamp)
        {
            if (cropBehaviour.Grow())
                pendingSwapPrefab = cropBehaviour.GetCurrentPlotPrefab();
        }
    }
}
