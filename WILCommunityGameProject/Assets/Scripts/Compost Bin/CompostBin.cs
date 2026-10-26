using System;
using UnityEngine;

namespace WILCommunityGame
{
    public class CompostBin : MonoBehaviour, IInteractable, IBuildPreview, ITimeTracker
    {
        [Header("Animation")] [SerializeField] private Animator animator;

        [Header("Recipe")] [SerializeField] private ToolItemSO fertilizer;
        [SerializeField, Range(1, InventoryItem.maxCount)] private int cropWasteRequired = 10;
        [SerializeField, Range(1, InventoryItem.maxCount)] private int naturalWasteRequired = 1;
        [SerializeField, Min(1)] private int compostDays = 1;

        public const int CropWasteSlot = 0;
        public const int NaturalWasteSlot = 1;
        public const int OutputSlot = 2;
        public const int SlotCount = 3;

        public event Action OnContentsChanged;
        public bool IsComposting { get; private set; }
        public float Progress => !slots[OutputSlot].isEmpty ? 1f : IsComposting ?
            Mathf.Clamp01((float)elapsedMinutes / durationMinutes) : 0f;

        private readonly InventoryItem[] slots = { new(), new(), new() };
        private CompostBinUI compostBinUI;
        private IndicatorManager indicatorManager;
        private PlayerInteractionDetector interactionDetector;
        private TimeManager timeManager;
        private bool indicatorVisible;
        private bool isPreview;
        private bool transferInProgress;
        private int startedAtMinute;
        private int elapsedMinutes;
        private int durationMinutes;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>(true);

            indicatorManager = GetComponentInChildren<IndicatorManager>(true);
            interactionDetector = FindFirstObjectByType<PlayerInteractionDetector>();
        }

        private void Start() => RegisterClock();

        private void OnEnable()
        {
            RegisterClock();
            UpdateAnimationState();
        }

        private void UpdateAnimationState()
        {
            if (animator != null)
                animator.SetBool("IsComposting", IsComposting);
        }

        private void RegisterClock()
        {
            if (isPreview || timeManager != null || TimeManager.Instance == null)
                return;

            timeManager = TimeManager.Instance;
            timeManager.RegisterTracker(this);

            if (IsComposting && timeManager.CurrentGameTimeStamp != null)
                ClockUpdate(timeManager.CurrentGameTimeStamp);
        }

        public InventoryItem GetSlotSnapshot(int index)
        {
            if (index < 0 || index >= SlotCount)
                return new InventoryItem();

            return new InventoryItem { item = slots[index].item, count = slots[index].count };
        }

        public int DepositFrom(UIManager inventory, CompostItemSO compost, int targetIndex, int requestedAmount)
        {
            if (isPreview || !isActiveAndEnabled || transferInProgress || IsComposting ||
                inventory == null || compost == null || !compost.IsIngredient || requestedAmount <= 0 ||
                targetIndex < CropWasteSlot || targetIndex > NaturalWasteSlot || !slots[OutputSlot].isEmpty)
                return 0;

            CompostItem requiredType = targetIndex == CropWasteSlot ? CompostItem.CropWaste : CompostItem.NaturalWaste;
            InventoryItem target = slots[targetIndex];

            if (compost.compostItem != requiredType || (!target.isEmpty && target.item != compost))
                return 0;

            int space = InventoryItem.maxCount - target.count;
            int amount = Mathf.Min(requestedAmount, Mathf.Min(space, inventory.GetCompostCount(compost)));

            if (amount <= 0)
                return 0;

            transferInProgress = true;

            try
            {
                int removed = inventory.RemoveCompost(compost, amount);
                if (removed <= 0)
                    return 0;

                target.item = compost;
                target.count += removed;
                TryStartComposting();
                OnContentsChanged?.Invoke();
                return removed;
            }
            finally
            {
                transferInProgress = false;
            }
        }

        public int WithdrawTo(UIManager inventory, InventoryItemData item, int sourceIndex, int requestedAmount)
        {
            if (isPreview || !isActiveAndEnabled || transferInProgress || IsComposting ||
                inventory == null || item == null || requestedAmount <= 0 || sourceIndex < 0 || sourceIndex >= SlotCount)
                return 0;

            InventoryItem source = slots[sourceIndex];
            if (source.item != item || source.count <= 0)
                return 0;

            transferInProgress = true;

            try
            {
                int added = inventory.TryAddItem(item, Mathf.Min(requestedAmount, source.count));
                if (added <= 0)
                    return 0;

                source.count -= added;
                if (source.count == 0)
                    slots[sourceIndex] = new InventoryItem();

                if (sourceIndex == OutputSlot)
                    TryStartComposting();

                OnContentsChanged?.Invoke();
                return added;
            }
            finally
            {
                transferInProgress = false;
            }
        }

        private void TryStartComposting()
        {
            RegisterClock();

            if (IsComposting || timeManager == null || timeManager.CurrentGameTimeStamp == null ||
                fertilizer == null || fertilizer.toolType != ToolType.Fertilizer ||
                !slots[OutputSlot].isEmpty || slots[CropWasteSlot].count < cropWasteRequired ||
                slots[NaturalWasteSlot].count < naturalWasteRequired)
                return;

            startedAtMinute = GetTotalMinutes(timeManager.CurrentGameTimeStamp);
            elapsedMinutes = 0;
            durationMinutes = GameTimestamp.HoursToMinutes(GameTimestamp.DaysToHours(Mathf.Max(1, compostDays)));
            IsComposting = true;
            UpdateAnimationState();
        }

        public void ClockUpdate(GameTimestamp timestamp)
        {
            if (isPreview || !isActiveAndEnabled || !IsComposting)
                return;

            elapsedMinutes = Mathf.Max(0, GetTotalMinutes(timestamp) - startedAtMinute);

            if (elapsedMinutes < durationMinutes)
                return;

            ConsumeIngredient(CropWasteSlot, cropWasteRequired);
            ConsumeIngredient(NaturalWasteSlot, naturalWasteRequired);
            slots[OutputSlot] = new InventoryItem { item = fertilizer, count = 1 };
            IsComposting = false;
            UpdateAnimationState();
            OnContentsChanged?.Invoke();
        }

        private void ConsumeIngredient(int index, int amount)
        {
            slots[index].DecreaseCount(amount);

            if (slots[index].count == 0)
                slots[index] = new InventoryItem();
        }

        private static int GetTotalMinutes(GameTimestamp timestamp)
        {
            return GameTimestamp.HoursToMinutes(GameTimestamp.DaysToHours(timestamp.day) + timestamp.hour) + timestamp.minute;
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

            compostBinUI = FindFirstObjectByType<CompostBinUI>(FindObjectsInactive.Include);
            compostBinUI?.ToggleCompostUI(this, interactor);
        }

        public void PrepareAsPreview()
        {
            isPreview = true;
            indicatorManager?.gameObject.SetActive(false);
            enabled = false;
        }

        private void OnDisable()
        {
            timeManager?.UnregisterTracker(this);
            timeManager = null;
            indicatorVisible = false;
            indicatorManager?.HideIndictor();

            if (compostBinUI != null && compostBinUI.ActiveBin == this)
                compostBinUI.CloseCompostUI();
        }
    }
}
