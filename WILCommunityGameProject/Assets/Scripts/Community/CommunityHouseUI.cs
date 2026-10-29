using System.Collections.Generic;
using DG.Tweening;
using Nova;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WILCommunityGame
{
    public class CommunityHouseUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UIManager inventory;
        [SerializeField] private BuildPlacer buildPlacer;

        [Header("House Status")]
        [SerializeField] private UIBlock2D statusBlock;
        [SerializeField] private UIBlock2D bowlIcon;
        [SerializeField] private TextBlock statusText;
        [SerializeField] private Color emptyColor;
        [SerializeField] private Color neutralColor;
        [SerializeField] private Color fullColor;

        [Header("Items")]
        [SerializeField] private Transform cropsRoot;
        [SerializeField] private Transform giveRoot;
        [SerializeField] private Transform receiveRoot;
        [SerializeField] private ItemView cropItemPrefab;
        [SerializeField] private ItemView tradeItemPrefab;

        [Header("Controls")]
        [SerializeField] private GameObject tradeContentBackground;
        [SerializeField] private Interactable feedButton;
        [SerializeField] private Interactable tradeButton;
        [SerializeField] private TextBlock feedButtonText;
        [SerializeField] private TextBlock tradeButtonText;
        [SerializeField] private TextBlock tradeStatusText;

        public bool IsOpen { get; private set; }
        public CommunityHouse ActiveHouse { get; private set; }

        private List<ItemView> cropViews;
        private List<ItemView> giveViews;
        private List<ItemView> receiveViews;
        private PlayerController player;
        private PlayerInteractionDetector interactionDetector;
        private bool refreshPending;
        private bool inputReady;

        private bool CanInteract => IsOpen && inputReady && isActiveAndEnabled && !GameUIManager.GameplayInputBlocked &&
                                    ActiveHouse != null && ActiveHouse.isActiveAndEnabled &&
                                    ActiveHouse.FeedingEnabled && player != null && !player.IsInventoryOpen &&
                                    interactionDetector != null &&
                                    ReferenceEquals(interactionDetector.CurrentTarget, ActiveHouse);

        private void Awake()
        {
            cropViews = GetItemViews(cropsRoot);
            giveViews = GetItemViews(giveRoot);
            receiveViews = GetItemViews(receiveRoot);
            transform.localScale = Vector3.zero;
        }

        private List<ItemView> GetItemViews(Transform root)
        {
            List<ItemView> views = new();

            foreach (Transform child in root)
            {
                if (child.TryGetComponent(out ItemView view))
                    views.Add(view);
            }

            return views;
        }

        public void ToggleHouseUI(CommunityHouse house, PlayerController interactor)
        {
            if (house == null || interactor == null)
                return;

            if (cropItemPrefab == null || tradeItemPrefab == null)
            {
                Debug.LogError("Assign the crop and trade item prefabs on CommunityHouseUI.", this);
                return;
            }

            if (IsOpen && ActiveHouse == house)
            {
                CloseHouseUI();
                return;
            }

            if (interactor.IsInventoryOpen)
                interactor.ToggleInventory();

            buildPlacer.enabled = false;
            inventory.UnEquipItem();
            Unsubscribe();

            ActiveHouse = house;
            player = interactor;
            interactionDetector = player.GetComponent<PlayerInteractionDetector>();
            inventory.OnInventoryChanged += RequestRefresh;
            ActiveHouse.OnHouseChanged += RequestRefresh;
            IsOpen = true;
            inputReady = false;

            RefreshHouse();
            transform.DOKill();
            transform.DOScale(1f, 0.35f).SetEase(Ease.OutCubic).SetUpdate(true)
                .OnComplete(() => inputReady = true);
        }

        public void CloseHouseUI()
        {
            Unsubscribe();
            IsOpen = false;
            inputReady = false;
            refreshPending = false;
            ActiveHouse = null;
            player = null;
            interactionDetector = null;

            transform.DOKill();
            transform.DOScale(0f, 0.35f).SetEase(Ease.OutCubic).SetUpdate(true);
        }

        public void FeedHouse()
        {
            if (!CanInteract)
                return;

            ActiveHouse.Feed();
            RefreshHouse();
        }

        public void Trade()
        {
            if (!CanInteract)
                return;

            ActiveHouse.TryTrade();
            RefreshHouse();
        }

        private void RequestRefresh()
        {
            refreshPending = true;
        }

        private void RefreshHouse()
        {
            CommunityHouseSatisfaction satisfaction = ActiveHouse.Satisfaction;
            bool fullyFed = satisfaction == CommunityHouseSatisfaction.Full;
            tradeContentBackground.SetActive(fullyFed);
            tradeButton.gameObject.SetActive(fullyFed);

            statusText.Text = satisfaction == CommunityHouseSatisfaction.Full ? "Fully Fed!" :
                satisfaction == CommunityHouseSatisfaction.Neutral ? "Partly Fed" : "Hungry";
            statusBlock.Color = satisfaction == CommunityHouseSatisfaction.Full ? fullColor :
                satisfaction == CommunityHouseSatisfaction.Neutral ? neutralColor : emptyColor;
            bowlIcon.SetImage(ActiveHouse.BowlSprite);

            SetViewCount(cropViews, ActiveHouse.Requests.Count, cropsRoot, cropItemPrefab);

            for (int index = 0; index < ActiveHouse.Requests.Count; index++)
            {
                if (cropViews[index].TryGetVisuals(out CropIconVisuals visuals))
                    visuals.Bind(ActiveHouse.Requests[index]);

            }

            CommunityTradeSO trade = ActiveHouse.DailyTrade;
            BindTradeItems(giveViews, trade != null && trade.IsValid ? trade.Give : null, giveRoot);
            BindTradeItems(receiveViews, trade != null && trade.IsValid ? trade.Receive : null, receiveRoot);

            bool canFeed = false;
            List<InventoryItem> produce = inventory.GetProduceItems();

            foreach (CropRequest request in ActiveHouse.Requests)
            {
                if (!request.IsComplete && produce.Exists(stack => stack.Produce.produceType == request.Produce.produceType))
                    canFeed = true;
            }

            feedButton.enabled = canFeed;
            feedButtonText.Text = satisfaction == CommunityHouseSatisfaction.Full ? "Fully Fed!" :
                canFeed ? "Feed House" : "Need crops";
            bool canTrade = ActiveHouse.CanTrade(out string reason);
            tradeButton.enabled = canTrade;
            tradeButtonText.Text = ActiveHouse.HasTradedToday ? "Traded Today" : "Trade";

            if (tradeStatusText != null)
                tradeStatusText.Text = canTrade ? "Today's Trade Offer" : reason;
            else if (!canTrade)
                tradeButtonText.Text = reason;

            refreshPending = false;
        }

        private void BindTradeItems(List<ItemView> views, TradeItem[] items, Transform root)
        {
            int count = items != null ? items.Length : 0;
            SetViewCount(views, count, root, tradeItemPrefab);

            for (int index = 0; index < count; index++)
            {
                if (views[index].TryGetVisuals(out TradeItemVisuals visuals))
                    visuals.Bind(items[index]);
            }
        }

        private void SetViewCount(List<ItemView> views, int count, Transform root, ItemView prefab)
        {
            while (views.Count < count)
                views.Add(Instantiate(views.Count > 0 ? views[0] : prefab, root));

            for (int index = 0; index < views.Count; index++)
                views[index].gameObject.SetActive(index < count);
        }

        private void Update()
        {
            if (GameUIManager.GameplayInputBlocked) return;

            if (!IsOpen)
                return;

            if (ActiveHouse == null || !ActiveHouse.isActiveAndEnabled || !ActiveHouse.FeedingEnabled ||
                player == null || player.IsInventoryOpen || interactionDetector == null ||
                !ReferenceEquals(interactionDetector.CurrentTarget, ActiveHouse) ||
                (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame))
            {
                CloseHouseUI();
            }
        }

        private void LateUpdate()
        {
            if (IsOpen && refreshPending)
                RefreshHouse();
        }

        private void Unsubscribe()
        {
            if (inventory != null)
                inventory.OnInventoryChanged -= RequestRefresh;

            if (ActiveHouse != null)
                ActiveHouse.OnHouseChanged -= RequestRefresh;
        }

        private void OnDisable()
        {
            CloseHouseUI();
            transform.DOKill();
            transform.localScale = Vector3.zero;
        }
    }
}
