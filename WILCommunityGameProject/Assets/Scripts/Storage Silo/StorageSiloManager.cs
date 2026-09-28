using System;
using System.Collections.Generic;
using UnityEngine;

namespace WILCommunityGame
{
    public class StorageSiloManager : MonoBehaviour, IInteractable, IBuildPreview
    {
        private StorageSiloUI storageSiloUI;
        private IndicatorManager indicatorManager;
        private PlayerInteractionDetector interactionDetector;
        private bool indicatorVisible;
        private bool isPreview;
        public const int SlotCount = 9;

        public event Action OnStorageChanged;

        private readonly InventoryItem[] slots = new InventoryItem[SlotCount];
        private bool transferInProgress;

        private void Awake()
        {
            EnsureSlots();
            indicatorManager = GetComponentInChildren<IndicatorManager>(true);
            interactionDetector = FindFirstObjectByType<PlayerInteractionDetector>();
        }

        public void Initialize(StorageSiloUI ui)
        {
            storageSiloUI = ui;
        }

        private void EnsureSlots()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] ??= new InventoryItem();
            }
        }

        public List<InventoryItem> GetContentsSnapshot()
        {
            EnsureSlots();

            List<InventoryItem> result = new(SlotCount);

            foreach (InventoryItem slot in slots)
            {
                result.Add(new InventoryItem
                {
                    item = slot.item,
                    count = slot.count
                });
            }

            return result;
        }

        public int DepositFrom(UIManager inventory, ProduceItemSO produce, int targetIndex, int requestedAmount)
        {
            if (isPreview || !isActiveAndEnabled || transferInProgress || inventory == null || produce == null ||
                requestedAmount <= 0 || targetIndex < 0 || targetIndex >= SlotCount)
                return 0;

            EnsureSlots();

            InventoryItem target = slots[targetIndex];

            if (!target.isEmpty && target.item != produce)
                return 0;

            int space = InventoryItem.maxCount - target.count;

            int amount = Mathf.Min(requestedAmount, Mathf.Min(space, inventory.GetProduceCount(produce)));

            if (amount <= 0)
                return 0;

            transferInProgress = true;

            try
            {
                int removed = inventory.RemoveProduce(produce, amount);

                if (removed <= 0)
                    return 0;

                target.item = produce;
                target.count += removed;

                OnStorageChanged?.Invoke();
                return removed;
            }
            finally
            {
                transferInProgress = false;
            }
        }

        public int WithdrawTo(UIManager inventory, ProduceItemSO produce, int sourceIndex, int requestedAmount)
        {
            if (isPreview || !isActiveAndEnabled || transferInProgress || inventory == null || produce == null ||
                requestedAmount <= 0 || sourceIndex < 0 || sourceIndex >= SlotCount)
                return 0;

            EnsureSlots();
            InventoryItem source = slots[sourceIndex];

            // The slot may have changed since the drag began.
            if (source.item != produce || source.count <= 0)
                return 0;

            int amount = Mathf.Min(requestedAmount, source.count);
            transferInProgress = true;

            try
            {
                int added = inventory.TryAddItem(produce, amount);
                if (added <= 0)
                    return 0;

                // Leave anything that did not fit in the silo.
                source.count -= added;
                if (source.count == 0)
                    slots[sourceIndex] = new InventoryItem();

                OnStorageChanged?.Invoke();
                return added;
            }
            finally
            {
                transferInProgress = false;
            }
        }

        public void PrepareAsPreview()
        {
            isPreview = true;
            indicatorManager?.gameObject.SetActive(false);
            enabled = false;
        }

        private void FixedUpdate()
        {
            bool shouldShow = !isPreview && interactionDetector != null &&
                              ReferenceEquals(interactionDetector.CurrentTarget, this);

            if (shouldShow == indicatorVisible)
                return;

            indicatorVisible = shouldShow;

            if (indicatorManager == null)
                return;

            if (shouldShow)
                indicatorManager.ShowIndictor();
            else
                indicatorManager.HideIndictor();
        }

        public void Interact(PlayerController interactor)
        {
            if (isPreview || !isActiveAndEnabled)
                return;

            storageSiloUI = FindFirstObjectByType<StorageSiloUI>(FindObjectsInactive.Include);
            storageSiloUI?.ToggleSiloUI(this, interactor);
        }
        
        private void OnDisable()
        {
            if (storageSiloUI != null && storageSiloUI.ActiveSilo == this)
            {
                storageSiloUI.CloseSiloUI();
            }
        }
    }
}
