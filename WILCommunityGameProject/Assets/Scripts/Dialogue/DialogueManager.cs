using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script manages how all dialogue will be displayed 
public class DialogueManager : MonoBehaviour
{
    [SerializeField] private DialogueVisuals dialogueVisuals;   //Assign the dialogue visuals

    //Create a new Queue for the dialogues and the sentences they hold
    private  Queue<DialogueBase> dialogueQueue = new();
    private  Queue<string> sentences = new();
    private bool hasStartedDialogue = false;
    private Action onDialogueComplete;
    private Action<int> onDialogueIndexComplete;
    private int currentDialogueIndex;

    public static DialogueManager ActiveDialogue { get; private set; }
    public bool IsPaused { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetActiveDialogue()
    {
        ActiveDialogue = null;
    }

    private void Awake()
    {
        dialogueVisuals.Initialize();
    }

    public bool HasStartedDialogue => hasStartedDialogue;

    public void StartDialogue(DialogueBase dialogue, Action onComplete = null)
    {
        if (hasStartedDialogue || dialogue == null) return;

        dialogueQueue.Clear();
        onDialogueComplete = onComplete;
        onDialogueIndexComplete = null;
        currentDialogueIndex = 0;
        dialogueQueue.Enqueue(dialogue);
        BeginQueuedDialogues();
    }

    //Queue each dialogue in the list to be shown
    public void StartDialogueSequence(List<DialogueBase> dialogues, Action onComplete = null, Action<int> onIndexComplete = null)
    {
        if (hasStartedDialogue || dialogues == null || dialogues.Count == 0) return;

        dialogueQueue.Clear();
        onDialogueComplete = onComplete;
        onDialogueIndexComplete = onIndexComplete;
        currentDialogueIndex = 0;
        
        foreach (DialogueBase dialogue in dialogues)
        {
            if (dialogue != null)
                dialogueQueue.Enqueue(dialogue);
        }
        BeginQueuedDialogues();
    }

    private void BeginQueuedDialogues()
    {
        StopAllCoroutines();
        sentences.Clear();

        if (dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        hasStartedDialogue = true;
        ResumeDialogue();
        StartNextDialogueBase();
    }

    //Move to the next dialogue base in the list
    private void StartNextDialogueBase()
    {
        if (dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        sentences.Clear();

        DialogueBase dialogue = dialogueQueue.Dequeue();
        dialogueVisuals.NameText.Text = dialogue.DialogueName;

        foreach (string textBlock in dialogue.DialogueText)
        {
            sentences.Enqueue(textBlock);
        }

        DisplayNextDialogueText();
    }

    //Display next block of text in that dialogue base
    public void DisplayNextDialogueText()
    {
        if (!hasStartedDialogue || IsPaused) return;

        StopAllCoroutines();
        if (sentences.Count == 0)
        {
            onDialogueIndexComplete?.Invoke(currentDialogueIndex);
            currentDialogueIndex++;
            
            StartNextDialogueBase();
            return;
        }

        string textToDisplay = (string)sentences.Dequeue();
        dialogueVisuals.DialogueText.Text = textToDisplay;
        StartCoroutine(ShowDialogueText(textToDisplay));
    }

    //Show dialogue text letter by letter in the dialogue box
    private IEnumerator ShowDialogueText(string textToDisplay)
    {
        dialogueVisuals.DialogueText.Text = "";
        foreach (char letter in textToDisplay.ToCharArray())
        {
            dialogueVisuals.DialogueText.Text += letter;
            yield return new WaitForSeconds(0.05f);
        }

    }

    public void PauseDialogue()
    {
        if (!hasStartedDialogue) return;

        IsPaused = true;
        if (ActiveDialogue == this)
            ActiveDialogue = null;

        dialogueVisuals.Hide();
    }

    public void ResumeDialogue()
    {
        if (!hasStartedDialogue) return;

        if (ActiveDialogue != null && ActiveDialogue != this)
            ActiveDialogue.PauseDialogue();

        ActiveDialogue = this;
        IsPaused = false;
        dialogueVisuals.Show();
    }

    private void OnDisable()
    {
        if (ActiveDialogue == this)
            ActiveDialogue = null;

        StopAllCoroutines();
        IsPaused = true;
        dialogueVisuals.Reset();
    }

    private void EndDialogue()
    {
        if (ActiveDialogue == this)
            ActiveDialogue = null;

        hasStartedDialogue = false;
        IsPaused = false;
        StopAllCoroutines();
        dialogueVisuals.Hide();
        Action callback = onDialogueComplete;
        onDialogueComplete = null;
        onDialogueIndexComplete = null;
        callback?.Invoke();
    }
}
