using System;
using System.Collections.Generic;
using Nova;
using UnityEngine;
using Random = System.Random;

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
        [SerializeField] private ListView cropRequestList;
        [SerializeField] private UIBlock2D bowlIcon;

        [Header("Available Crops")] [SerializeField]
        private ProduceItemSO[] availableCrops;

        [Header("Daily Request Settings")] [SerializeField]
        private int minCropTypesPerHouse = 1;

        [SerializeField] private int maxCropTypesPerHouse = 5;
        [SerializeField] private int maxAmountPerCrop = 30;

        private readonly List<CropRequest> requests = new();
        private CommunityHouseVisuals visuals;
        private int requestDay;
        public CommunityHouseSatisfaction Satisfaction { get; private set; } = CommunityHouseSatisfaction.Empty;
        public event Action<CommunityHouse, CommunityHouseSatisfaction> OnSatisfactionChanged;


        private void Start()
        {
            visuals = GetComponentInChildren<CommunityHouseVisuals>();

            if (cropRequestList != null)
            {
                cropRequestList.AddDataBinder<CropRequest, CropIconVisuals>(BindCropIcon);
            }

            requestDay = TimeManager.Instance.CurrentGameTimeStamp.day;
            GenerateDailyRequests();

            TimeManager.Instance.RegisterTracker(this);
        }

        private void BindCropIcon(Data.OnBind<CropRequest> evt, CropIconVisuals target, int index)
        {
            target.Bind(evt.UserData);
        }

        private void OnDestroy()
        {
            TimeManager.Instance.UnregisterTracker(this);
        }

        public void Interact(PlayerController interactor)
        {
            foreach (var request in requests)
            {
                if (request == null || request.IsComplete)
                    continue;

                int delivered = uiManager.RemoveProduce(request.Produce.produceType, request.Remaining);
                request.Delivered += delivered;
                communityUIManager.AddDelivered(request.Produce.produceType, delivered);
            }

            RefreshVisuals();
        }

        public void ClockUpdate(GameTimestamp timestamp)
        {
            if (timestamp.day == requestDay)
                return;

            requestDay = timestamp.day;
            GenerateDailyRequests();
        }

        private void GenerateDailyRequests()
        {
            requests.Clear();

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
        }
    }
}