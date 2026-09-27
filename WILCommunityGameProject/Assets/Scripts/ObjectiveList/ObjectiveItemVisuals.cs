using Nova;
using UnityEngine;

namespace WILCommunityGame
{
    [System.Serializable]
    public class ObjectiveItemVisuals : ItemVisuals
    {
        public TextBlock objectiveText;
        public GameObject checkMark;
        public UIBlock2D checkBox;
        public Color checkBoxDefualtColor;
        public Color checkBoxCompletedColor = Color.green;

        public void Bind(ObjectiveItem item, bool completed)
        {
            objectiveText.Text = item.objectiveText;
            checkMark.SetActive(completed);
            checkBox.Color = completed ? checkBoxCompletedColor : checkBoxDefualtColor;
        }
    }
}
