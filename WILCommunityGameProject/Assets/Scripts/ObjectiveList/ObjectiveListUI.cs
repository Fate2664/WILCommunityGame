using System;
using System.Collections.Generic;
using Nova;
using UnityEngine;

namespace WILCommunityGame
{
    public class ObjectiveListUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ListView objectiveList;
        [SerializeField] private TextBlock currentObjectiveText;
        
        [Header("Objective")]
        [SerializeField] private Objective startingObjective;
        
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
                OnObjectiveCompleted?.Invoke(CurrentObjective);
            }
        }

        private void BindObjectiveItem(Data.OnBind<ObjectiveItem> evt, ObjectiveItemVisuals target, int index)
        {
            ObjectiveItem item = evt.UserData;
            target.Bind(item, completedItems.Contains(item));
        }

        private void OnDestroy()
        {
            if (initialized && objectiveList != null)
            {
                objectiveList.RemoveDataBinder<ObjectiveItem, ObjectiveItemVisuals>(BindObjectiveItem);
            }
        }
    }
}