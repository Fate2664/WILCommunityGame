using System;
using System.Collections.Generic;
using Nova;
using UnityEngine;

namespace WILCommunityGame
{
    [Serializable]
    public class CommunityStreakStage
    {
        public UIBlock2D Background;
        public UIBlock2D Connector;
        public Texture2D Icon;
        [TextArea] public string Description;
        public ObjectiveItem Objective;
    }

    public class CommunityUIManager : MonoBehaviour, ITimeTracker
    {
        [Header("Community Level")] [SerializeField]
        private TextBlock levelText;


        [Header("Community Happiness")] [SerializeField]
        private CommunityHouse[] communityHouses;

        [SerializeField] private UIBlock2D[] happinessBarSegments;
        [SerializeField] private Color emptySegmentColor;
        [SerializeField] private Color firstThirdColor;
        [SerializeField] private Color secondThirdColor;
        [SerializeField] private Color thirdThirdColor;

        [Header("Community Food Streak")]
        [SerializeField] private TextBlock streakNumber;
        [SerializeField] private CommunityStreakStage[] streakStages;
        [SerializeField] private Color lockedStageColor;
        [SerializeField] private Color unlockedStageColor;

        [Header("Objectives")]
        [SerializeField] private ObjectiveListUI objectiveListUI;
        
        [Header("Player Currency")]
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private TextBlock currencyText;
        
        public bool IsHappinessGreen { get; private set; }
        public event Action OnHappinessGreenChanged;
        public int FoodStreak { get; private set; }
        public int StreakStage { get; private set; }
        public IReadOnlyList<CommunityStreakStage> StreakStages => streakStages;
        public CommunityTradeQuality TradeQuality => StreakStage >= 2
            ? CommunityTradeQuality.Improved : CommunityTradeQuality.Normal;
        public event Action<int> OnStreakStageChanged;

        private int lastFullyFedDay = -1;
        private int currentDay;
        private readonly Dictionary<CommunityHouse, int> houseHappiness = new();
        private int levelNumber = 0;
        private bool happinessBarFull;

        private void Start()
        {
            currentDay = TimeManager.Instance.CurrentGameTimeStamp.day;
            levelText.Text = levelNumber.ToString();

            RefreshStreak();

            foreach (CommunityHouse house in communityHouses)
            {
                RegisterHouse(house);
            }

            RefreshHappinessBar();
            TimeManager.Instance.RegisterTracker(this);
        }
        
        private void OnEnable()
        {
            if (objectiveListUI != null)
            {
                objectiveListUI.OnObjectiveChanged += HandleObjectiveChanged;
                CompleteStreakObjectives();
            }

            if (playerStats == null || currencyText == null)
                return;

            playerStats.OnCurrencyChanged += RefreshCurrency;
            RefreshCurrency(playerStats.Currency);
        }

        private void OnDestroy()
        {
            foreach (CommunityHouse house in houseHappiness.Keys)
            {
                if (house != null)
                {
                    house.OnSatisfactionChanged -= HandleHouseSatisfactionChanged;
                }
            }

            if (TimeManager.Instance != null)
                TimeManager.Instance.UnregisterTracker(this);
        }
        
        private void RefreshCurrency(int amount)
        {
            currencyText.Text = $"R{amount}";
        }

        public void RegisterHouse(CommunityHouse house)
        {
            if (house == null || !house.FeedingEnabled || houseHappiness.ContainsKey(house))
                return;

            houseHappiness.Add(house, GetHappinessScore(house.Satisfaction));
            house.OnSatisfactionChanged += HandleHouseSatisfactionChanged;

            RefreshHappinessBar();
        }

        private void HandleHouseSatisfactionChanged(CommunityHouse house, CommunityHouseSatisfaction satisfaction)
        {
            ClockUpdate(TimeManager.Instance.CurrentGameTimeStamp);
            houseHappiness[house] = GetHappinessScore(satisfaction);
            RefreshHappinessBar();
            CheckFoodStreak();
        }

        private void CheckFoodStreak()
        {
            if (lastFullyFedDay == currentDay || houseHappiness.Count == 0)
                return;

            foreach (CommunityHouse house in houseHappiness.Keys)
            {
                if (house == null || house.RequestDay != currentDay ||
                    house.Satisfaction != CommunityHouseSatisfaction.Full)
                    return;
            }

            lastFullyFedDay = currentDay;
            FoodStreak++;

            int newStage = Mathf.Min(FoodStreak, 3);
            bool stageIncreased = newStage > StreakStage;

            if (stageIncreased)
            {
                StreakStage = newStage;

                if (StreakStage == 1)
                    playerStats.AddCurrency(1000);
            }

            RefreshStreak();

            if (stageIncreased)
                OnStreakStageChanged?.Invoke(StreakStage);
        }

        private void RefreshHappinessBar()
        {
            int totalScore = 0;

            foreach (int score in houseHappiness.Values)
                totalScore += score;

            int maxScore = houseHappiness.Count * GetHappinessScore(CommunityHouseSatisfaction.Full);
            int segmentCount = happinessBarSegments.Length;
            int filledSegments = maxScore > 0 ? (int)((long)totalScore * segmentCount / maxScore) : 0;

            for (int i = 0; i < segmentCount; i++)
            {
                if (happinessBarSegments[i] != null)
                {
                    happinessBarSegments[i].Color = i < filledSegments
                        ? GetFilledSegmentColor(i)
                        : emptySegmentColor;
                }
            }

            bool isFullNow = segmentCount > 0 && maxScore > 0 && totalScore == maxScore;

            if (isFullNow && !happinessBarFull)
            {
                levelNumber++;
                levelText.Text = levelNumber.ToString();
            }

            happinessBarFull = isFullNow;

            int greenStartIndex = Mathf.CeilToInt(segmentCount * 2f / 3f);
            bool isGreenNow = segmentCount > 0 &&
                              filledSegments > greenStartIndex;

            if (IsHappinessGreen != isGreenNow)
            {
                IsHappinessGreen = isGreenNow;
                OnHappinessGreenChanged?.Invoke();
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

            RefreshStreak();
        }

        private void RefreshStreak()
        {
            streakNumber.Text = FoodStreak.ToString();

            for (int index = 0; index < streakStages.Length; index++)
            {
                CommunityStreakStage stage = streakStages[index];
                Color color = index < StreakStage ? unlockedStageColor : lockedStageColor;

                if (stage.Background != null)
                    stage.Background.Color = color;

                if (stage.Connector != null)
                    stage.Connector.Color = color;
            }

            CompleteStreakObjectives();
        }

        private void HandleObjectiveChanged(Objective objective)
        {
            // A milestone can be reached before its objective list becomes active.
            CompleteStreakObjectives();
        }

        private void CompleteStreakObjectives()
        {
            if (objectiveListUI == null || streakStages == null) return;

            for (int index = 0; index < Mathf.Min(StreakStage, streakStages.Length); index++)
                objectiveListUI.CompleteItem(streakStages[index].Objective);
        }
        
        private void OnDisable()
        {
            if (objectiveListUI != null)
                objectiveListUI.OnObjectiveChanged -= HandleObjectiveChanged;

            if (playerStats != null)
                playerStats.OnCurrencyChanged -= RefreshCurrency;
        }
    }
}
