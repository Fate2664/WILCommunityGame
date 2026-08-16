using UnityEngine;

namespace WILCommunityGame
{
    [CreateAssetMenu(menuName = "Information Tip")]
    public sealed class InformationTipSO : ScriptableObject
    {
        [TextArea(3, 10)]
        public string text;
        public string Text => text;
    }
}
