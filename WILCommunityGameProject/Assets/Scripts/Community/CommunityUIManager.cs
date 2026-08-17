using System;
using System.Collections.Generic;
using Nova;
using UnityEngine;

namespace WILCommunityGame
{
    public class CommunityUIManager : MonoBehaviour, ITimeTracker
    {
        [Header("Community Level")]
        [SerializeField] private TextBlock levelText;
        
        
        [Header("Community Happiness")] 
        [SerializeField] private CommunityHouse[] communityHouses;
        [SerializeField] private UIBlock2D[] happinessBarSegments;
        [SerializeField] private Color emptySegmentColor;
        [SerializeField] private Color firstThirdColor;
        [SerializeField] private Color secondThirdColor;
        [SerializeField] private Color thirdThirdColor;
        
        [Header("Crop Amounts")]
        [SerializeField] private TextBlock tomatoCropAmount;
        [SerializeField] private TextBlock potatoCropAmount;
        [SerializeField] private TextBlock cornCropAmount;
        [SerializeField] private TextBlock cabbageCropAmount;
        [SerializeField] private TextBlock carrotCropAmount;

        private int tomatosDelivered;
        private int potatosDelivered;
        private int cornDelivered;
        private int cabbgeDelivered;
        private int carrotsDelivered;
        private int currentDay;
        private readonly Dictionary<CommunityHouse, int> houseHappiness = new();
        private int levelNumber = 0;
        private bool happinessBarFull;

        private void Start()
        {
            currentDay = TimeManager.Instance.CurrentGameTimeStamp.day;
            levelText.Text = levelNumber.ToString();
            
            ResetDeliveryAmounts();

            foreach (CommunityHouse house in communityHouses)
            {
                RegisterHouse(house);
            }

            RefreshHappinessBar();
            TimeManager.Instance.RegisterTracker(this);
        }

        private void OnDestroy()
        {
            foreach (CommunityHouse house in communityHouses)
            {
                house.OnSatisfactionChanged -= HandleHouseSatisfactionChanged;
            }
            
            TimeManager.Instance.UnregisterTracker(this);
        }

        public void AddDelivered(ProduceType type, int amount)
        {
            switch (type)
            {
                case ProduceType.Tomato:
                    tomatosDelivered += amount;
                    break;
                case ProduceType.Potato:
                    potatosDelivered += amount;
                    break;
                case ProduceType.Corn:
                    cornDelivered += amount;
                    break;
                case ProduceType.Cabbage:
                    cabbgeDelivered += amount;
                    break;
                case ProduceType.Carrot:
                    carrotsDelivered += amount;
                    break;
            }

            RefreshAmounts();
        }
        
        private void RegisterHouse(CommunityHouse house)
        {
            if (houseHappiness.ContainsKey(house))
                return;
            
            houseHappiness.Add(house, GetHappinessScore(house.Satisfaction));
            house.OnSatisfactionChanged += HandleHouseSatisfactionChanged;
        }

        private void HandleHouseSatisfactionChanged(CommunityHouse house, CommunityHouseSatisfaction satisfaction)
        {
            houseHappiness[house] = GetHappinessScore(satisfaction);
            RefreshHappinessBar();
        }

        private void RefreshHappinessBar()
        {
            int filledSegments = 0;
            
            foreach (int score in houseHappiness.Values)
                filledSegments += score;
            
            filledSegments = Mathf.Clamp(filledSegments, 0, happinessBarSegments.Length);

            for (int i = 0; i < happinessBarSegments.Length; i++)
            {
                if (happinessBarSegments[i] != null)
                {
                    happinessBarSegments[i].Color = i < filledSegments ? GetFilledSegmentColor(i) : emptySegmentColor;
                }
            }
            
            happinessBarFull = filledSegments == happinessBarSegments.Length;
            if (happinessBarFull)
            {
                levelNumber++;
                levelText.Text = levelNumber.ToString();
            }
        }

        private Color GetFilledSegmentColor(int segmentIndex)
        {
            int firstThird = Mathf.CeilToInt(happinessBarSegments.Length / 3f);
            int secondThird = Mathf.CeilToInt(happinessBarSegments.Length * 2f / 3f);

            if (segmentIndex < firstThird)
                return firstThirdColor;
            
            if (segmentIndex < secondThird)
                return secondThirdColor;

            return thirdThirdColor;
        }

        private int GetHappinessScore(CommunityHouseSatisfaction satisfaction)
        {
            return satisfaction switch
            {
                CommunityHouseSatisfaction.Neutral => 2,
                CommunityHouseSatisfaction.Full => 3,
                _ => 0
            };
        }

        public void ClockUpdate(GameTimestamp timestamp)
        {
            if (timestamp.day == currentDay)
                return;
            
            currentDay = timestamp.day;
            ResetDeliveryAmounts();
        }

        private void ResetDeliveryAmounts()
        {
            tomatosDelivered = 0;
            potatosDelivered = 0;
            cornDelivered = 0;
            cabbgeDelivered = 0;
            carrotsDelivered = 0;

            RefreshAmounts();
        }

        private void RefreshAmounts()
        {
            tomatoCropAmount.Text = tomatosDelivered.ToString();
            potatoCropAmount.Text = potatosDelivered.ToString();
            cornCropAmount.Text = cornDelivered.ToString();
            cabbageCropAmount.Text = cabbgeDelivered.ToString();
            carrotCropAmount.Text = carrotsDelivered.ToString();
        }
    }
}
