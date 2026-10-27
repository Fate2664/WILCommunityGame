using System;
using System.Collections.Generic;
using Nova;
using UnityEngine;

namespace WILCommunityGame
{
    [Serializable]
    public class CropRequest
    {
        public ProduceItemSO Produce;
        public int Requested;
        public int Delivered;

        public int Remaining => Mathf.Max(0, Requested - Delivered);
        public bool IsComplete => Delivered >= Requested;
    }

    public enum CommunityHouseSatisfaction
    {
        Empty,
        Neutral,
        Full
    }

    public class CommunityHouse : MonoBehaviour, IInteractable, ITimeTracker
    {
        [Header("Connections")] [SerializeField]
        private UIManager uiManager;

        [SerializeField] private CommunityUIManager communityUIManager;
        [SerializeField] private CommunityHouseUI communityHouseUI;
        [SerializeField] private ListView cropRequestList;
        [SerializeField] private UIBlock2D bowlIcon;

        [Header("Available Crops")] [SerializeField]
        private ProduceItemSO[] availableCrops;

        [Header("Daily Trades")]
        [SerializeField] private CommunityTradeSO[] availableTrades;

        [Header("Objectives")]
        [SerializeField] private ObjectiveListUI objectiveListUI;
        [SerializeField] private ObjectiveItem fullyFeedCommunityHouseObjective;

        [Header("Daily Request Settings")] 
        [SerializeField] private int minCropTypesPerHouse = 1;
        [SerializeField] private int maxCropTypesPerHouse = 5;
        [SerializeField] private int maxAmountPerCrop = 30;
        
        [Header("Availability")]
        [SerializeField] private bool feedingEnabled = true;
        [SerializeField] private GameObject feedingUIRoot;

        private readonly List<CropRequest> requests = new();
        private CommunityHouseVisuals visuals;
        public bool FeedingEnabled => feedingEnabled;
        public int RequestDay => requestDay;
        public IReadOnlyList<CropRequest> Requests => requests;
        public CommunityTradeSO DailyTrade { get; private set; }
        public bool HasTradedToday { get; private set; }
        public Sprite BowlSprite => bowlIcon.Sprite;
        public event Action OnHouseChanged;
        private bool hasStarted;
        private TimeManager registeredClock;
        private int requestDay;
        public CommunityHouseSatisfaction Satisfaction { get; private set; } = CommunityHouseSatisfaction.Empty;
        public event Action<CommunityHouse, CommunityHouseSatisfaction> OnSatisfactionChanged;


        private void Start()
        {
            visuals = GetComponentInChildren<CommunityHouseVisuals>(true);

            if (cropRequestList != null)
            {
                cropRequestList.AddDataBinder<CropRequest, CropIconVisuals>(BindCropIcon);
            }

            hasStarted = true;
            feedingUIRoot.SetActive(feedingEnabled);

            if (feedingEnabled)
                BeginFeeding();
        }

        private void BindCropIcon(Data.OnBind<CropRequest> evt, CropIconVisuals target, int index)
        {
            target.Bind(evt.UserData);
        }

        public void EnableFeeding()
        {
            if (feedingEnabled)
                return;
            
            feedingEnabled = true;

            if (hasStarted)
                BeginFeeding();
        }

        private void BeginFeeding()
        {
            feedingUIRoot.SetActive(true);

            registeredClock = TimeManager.Instance;
            requestDay = registeredClock.CurrentGameTimeStamp.day;
            communityUIManager.OnStreakStageChanged += HandleStreakStageChanged;
            
            GenerateDailyRequests();
            registeredClock.RegisterTracker(this);
            communityUIManager.RegisterHouse(this);
        }

        private void OnDestroy()
        {
            if (communityUIManager != null)
                communityUIManager.OnStreakStageChanged -= HandleStreakStageChanged;

            if (registeredClock != null)
                registeredClock.UnregisterTracker(this);

            if (hasStarted && cropRequestList != null)
            {
                cropRequestList.RemoveDataBinder<CropRequest, CropIconVisuals>(
                    BindCropIcon);
            }
        }

        public void Interact(PlayerController interactor)
        {
            if (!feedingEnabled || !hasStarted || !isActiveAndEnabled)
                return;

            communityHouseUI?.ToggleHouseUI(this, interactor);
        }

        public void Feed()
        {
            if (!feedingEnabled || !hasStarted || !isActiveAndEnabled)
                return;

            foreach (var request in requests)
            {
                if (request == null || request.IsComplete)
                    continue;

                int delivered = uiManager.RemoveProduce(request.Produce.produceType, request.Remaining);
                request.Delivered += delivered;
            }

            RefreshVisuals();
        }

        public bool CanTrade(out string reason)
        {
            reason = "Fully feed this house first";

            if (!feedingEnabled || !hasStarted || !isActiveAndEnabled ||
                Satisfaction != CommunityHouseSatisfaction.Full)
                return false;

            reason = "Traded today";
            if (HasTradedToday)
                return false;

            reason = "No trade available";
            if (DailyTrade == null || !DailyTrade.IsValid)
                return false;

            return uiManager.CanExchangeItems(DailyTrade, out reason);
        }

        public bool TryTrade()
        {
            if (!CanTrade(out _))
                return false;

            HasTradedToday = true;

            if (!uiManager.TryExchangeItems(DailyTrade))
            {
                HasTradedToday = false;
                return false;
            }

            OnHouseChanged?.Invoke();
            return true;
        }

        public void ClockUpdate(GameTimestamp timestamp)
        {
            if (!feedingEnabled || !hasStarted || !isActiveAndEnabled)
                return;

            if (timestamp.day == requestDay)
                return;

            requestDay = timestamp.day;
            GenerateDailyRequests();
        }

        private void HandleStreakStageChanged(int stage)
        {
            if (stage != 2 || HasTradedToday)
                return;

            GenerateDailyTrade();
            OnHouseChanged?.Invoke();
        }

        private void GenerateDailyTrade()
        {
            DailyTrade = null;

            if (availableTrades == null)
                return;

            List<CommunityTradeSO> trades = GetTrades(communityUIManager.TradeQuality);

            if (trades.Count == 0 && communityUIManager.TradeQuality == CommunityTradeQuality.Improved)
                trades = GetTrades(CommunityTradeQuality.Normal);

            if (trades.Count > 0)
                DailyTrade = trades[UnityEngine.Random.Range(0, trades.Count)];
        }

        private List<CommunityTradeSO> GetTrades(CommunityTradeQuality quality)
        {
            List<CommunityTradeSO> trades = new();

            foreach (CommunityTradeSO trade in availableTrades)
            {
                if (trade != null && trade.IsValid && trade.Quality == quality)
                    trades.Add(trade);
            }

            return trades;
        }

        private void GenerateDailyRequests()
        {
            requests.Clear();
            HasTradedToday = false;
            GenerateDailyTrade();

            List<ProduceItemSO> uniqueCrops = GetUniqueCrops();

            if (uniqueCrops.Count == 0)
            {
                RefreshVisuals();
                return;
            }

            Shuffle(uniqueCrops);

            int minTypes = Mathf.Clamp(minCropTypesPerHouse, 1, uniqueCrops.Count);
            int maxTypes = Mathf.Clamp(maxCropTypesPerHouse, minTypes, uniqueCrops.Count);
            int numberOfCropTypes = UnityEngine.Random.Range(minTypes, maxTypes + 1);

            for (int i = 0; i < numberOfCropTypes; i++)
            {
                requests.Add(new CropRequest
                {
                    Produce = uniqueCrops[i],
                    Requested = UnityEngine.Random.Range(1, maxAmountPerCrop + 1),
                    Delivered = 0
                });
            }

            RefreshVisuals();
        }

        private void RefreshVisuals()
        {
            cropRequestList?.SetDataSource(requests);

            bool hasDeliveredAnything = requests.Exists(request => request.Delivered > 0);
            bool allRequestsComplete = requests.Count > 0 && requests.TrueForAll(request => request.IsComplete);

            CommunityHouseSatisfaction satisfaction = allRequestsComplete ? CommunityHouseSatisfaction.Full :
                hasDeliveredAnything ? CommunityHouseSatisfaction.Neutral : CommunityHouseSatisfaction.Empty;
            
            SetSatisfaction(satisfaction);

            bowlIcon.SetImage(visuals.UpdateBowlImage(hasDeliveredAnything, allRequestsComplete));
            visuals.UpdateBowlBackground(hasDeliveredAnything, allRequestsComplete);
            OnHouseChanged?.Invoke();
        }

        private void Shuffle(List<ProduceItemSO> crops)
        {
            for (int i = crops.Count - 1; i >= 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                (crops[i], crops[randomIndex]) = (crops[randomIndex], crops[i]);
            }
        }

        private List<ProduceItemSO> GetUniqueCrops()
        {
            List<ProduceItemSO> uniqueCrops = new();
            HashSet<ProduceType> usedTypes = new();

            foreach (ProduceItemSO crop in availableCrops)
            {
                if (crop != null && usedTypes.Add(crop.produceType))
                {
                    uniqueCrops.Add(crop);
                }
            }

            return uniqueCrops;
        }

        private void SetSatisfaction(CommunityHouseSatisfaction newSatisfaction)
        {
            if (Satisfaction == newSatisfaction)
                return;
            
            Satisfaction = newSatisfaction;
            OnSatisfactionChanged?.Invoke(this, newSatisfaction);
            
            if (newSatisfaction == CommunityHouseSatisfaction.Full)
            {
                objectiveListUI?.CompleteItem(fullyFeedCommunityHouseObjective);
            }
        }
    }
}
