using System;
using UnityEngine;

[Serializable]
//This scriptable object holds all the base data for a given dialogue
[CreateAssetMenu(menuName = "Dialogue/DialogueBase")]
public class DialogueBase : ScriptableObject
{
    public string DialogueName;
    [TextArea(3,10)]
    public string[] DialogueText;
}
