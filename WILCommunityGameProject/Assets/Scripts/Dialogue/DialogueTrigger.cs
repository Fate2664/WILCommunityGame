using System;
using System.Collections.Generic;
using UnityEngine;
using WILCommunityGame;

[Serializable]
public class DialogueSeedReward
{
    public SeedItemSO Seed;
    [Min(1)] public int Amount = 1;
}

//This script manages player interaction with the dialogue trigger and what dialogues to show
public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueManager DialogueManager;
    [SerializeField] private List<DialogueBase> dialogueList;

    [Header("Rewards")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private List<DialogueSeedReward> seedRewards = new();

    private readonly HashSet<Collider> playerColliders = new();
    private readonly List<DialogueSeedReward> pendingRewards = new();
    private bool dialogueStarted;
    private bool dialogueCompleted;
    private bool givingRewards;

    private void OnEnable()
    {
        if (uiManager != null)
            uiManager.OnInventoryChanged += TryGiveRewards;

        TryGiveRewards();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || dialogueCompleted) return;

        playerColliders.Add(other);

        if (dialogueStarted)
        {
            DialogueManager.ResumeDialogue();
            return;
        }

        if (DialogueManager.HasStartedDialogue || dialogueList == null || dialogueList.Count == 0) return;

        dialogueStarted = true;
        DialogueManager.StartDialogueSequence(dialogueList, CompleteDialogue);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerColliders.Remove(other);

        if (playerColliders.Count == 0 && dialogueStarted && !dialogueCompleted)
            DialogueManager.PauseDialogue();
    }

    private void CompleteDialogue()
    {
        if (dialogueCompleted) return;

        dialogueCompleted = true;

        foreach (DialogueSeedReward reward in seedRewards)
        {
            if (reward == null || reward.Seed == null || reward.Amount <= 0)
                continue;

            pendingRewards.Add(new DialogueSeedReward { Seed = reward.Seed, Amount = reward.Amount });
        }

        TryGiveRewards();
    }

    private void TryGiveRewards()
    {
        if (givingRewards || pendingRewards.Count == 0 || uiManager == null) return;

        givingRewards = true;
        try
        {
            int rewardIndex = 0;
            while (rewardIndex < pendingRewards.Count)
            {
                DialogueSeedReward reward = pendingRewards[rewardIndex];
                reward.Amount -= uiManager.TryAddItem(reward.Seed, reward.Amount);

                if (reward.Amount == 0)
                    pendingRewards.RemoveAt(rewardIndex);
                else
                    rewardIndex++;
            }
        }
        finally
        {
            givingRewards = false;
        }
    }

    private void OnDisable()
    {
        if (uiManager != null)
            uiManager.OnInventoryChanged -= TryGiveRewards;

        playerColliders.Clear();

        if (dialogueStarted && !dialogueCompleted && DialogueManager != null)
            DialogueManager.PauseDialogue();
    }
}
