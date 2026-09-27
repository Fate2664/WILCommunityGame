using System.Collections.Generic;
using UnityEngine;

namespace WILCommunityGame
{
    [CreateAssetMenu(menuName = "ObjectiveList/Objective")]
    public class Objective : ScriptableObject
    {
        public string objectiveName;
        public List<ObjectiveItem> objectiveItems = new();
        public Objective nextObjective;
    }
}