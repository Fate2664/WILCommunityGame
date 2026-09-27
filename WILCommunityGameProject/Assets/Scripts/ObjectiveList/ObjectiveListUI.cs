using System;
using System.Collections.Generic;
using DG.Tweening;
using Nova;
using UnityEngine;
using Object = System.Object;

namespace WILCommunityGame
{
    public class ObjectiveListUI : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private ListView objectiveList;

        [SerializeField] private TextBlock currentObjectiveText;

        [Header("Objective")] [SerializeField] private Objective startingObjective;

        [Header("Slide Animation")] [SerializeField]
        private UIBlock panelRoot;

        [SerializeField] private float offScreenX = 1140f;
        [SerializeField] private float onScreenX = 777.5258f;
        [SerializeField] private float slideDuration = 0.35f;

        private Sequence slideSequence;
        private bool isTransitioning;

        public Objective CurrentObjective { get; private set; }
        public bool IsComplete => displayedItems.Count > 0 && completedItems.Count == displayedItems.Count;
        public event Action<Objective> OnObjectiveCompleted;

        private readonly List<ObjectiveItem> displayedItems = new();
        private readonly HashSet<ObjectiveItem> completedItems = new();

        private bool initialized = false;

        private void Start()
        {
            if (!initialized)
                SetObjective(startingObjective);
        }

        private void OnEnable()
        {
            if (initialized)
                objectiveList.Refresh();
            
            slideSequence?.Play();

            if (initialized)
                TryAdvanceObjective();
        }

        public void SetObjective(Objective objective)
        {
            if (!initialized)
            {
                objectiveList.AddDataBinder<ObjectiveItem, ObjectiveItemVisuals>(BindObjectiveItem);
                initialized = true;
            }

            CurrentObjective = objective;
            displayedItems.Clear();
            completedItems.Clear();

            if (objective != null)
            {
                foreach (var item in objective.objectiveItems)
                {
                    if (item != null && !displayedItems.Contains(item))
                        displayedItems.Add(item);
                }
            }

            currentObjectiveText.Text = objective != null ? objective.objectiveName : "No current objective";
            objectiveList.SetDataSource(displayedItems);
        }

        public void CompleteItem(ObjectiveItem item)
        {
            if (!initialized)
                SetObjective(startingObjective);

            if (item == null || !displayedItems.Contains(item)) return;

            //Completing the same item again has no effect
            if (!completedItems.Add(item)) return;

            objectiveList.Refresh();

            if (IsComplete)
            {
                Objective completedObjective = CurrentObjective;
                OnObjectiveCompleted?.Invoke(CurrentObjective);
                if (CurrentObjective == completedObjective)
                    TryAdvanceObjective();
            }
        }

        private void BindObjectiveItem(Data.OnBind<ObjectiveItem> evt, ObjectiveItemVisuals target, int index)
        {
            ObjectiveItem item = evt.UserData;
            target.Bind(item, completedItems.Contains(item));
        }

        private void TryAdvanceObjective()
        {
            if (!isActiveAndEnabled || isTransitioning || !IsComplete || CurrentObjective == null ||
                CurrentObjective.nextObjective == null)
                return;

            Objective nextObjective = CurrentObjective.nextObjective;
            isTransitioning = true;

            slideSequence = DOTween.Sequence().SetUpdate(true).Append(CreateSlide(offScreenX, Ease.InCubic))
                .AppendCallback(() =>
                {
                    SetObjective(nextObjective);
                    SetPanelX(offScreenX);
                })
                .Append(CreateSlide(onScreenX, Ease.OutCubic))
                .OnComplete(() =>
                {
                    slideSequence = null;
                    isTransitioning = false;

                    TryAdvanceObjective();
                });
        }

        private Tweener CreateSlide(float targetX, Ease ease)
        {
            return DOTween.To(() =>
                        panelRoot.transform.localPosition.x,
                    SetPanelX,
                    targetX,
                    slideDuration)
                .SetEase(ease);
        }

        private void SetPanelX(float x)
        {
            Vector3 position = panelRoot.transform.localPosition;
            position.x = x;

            panelRoot.TrySetLocalPosition(position);
        }

        private void OnDisable()
        {
            slideSequence?.Pause();
        }

        private void OnDestroy()
        {
            slideSequence?.Kill();
            slideSequence = null;
            
            if (initialized && objectiveList != null)
            {
                objectiveList.RemoveDataBinder<ObjectiveItem, ObjectiveItemVisuals>(BindObjectiveItem);
            }
        }
    }
}