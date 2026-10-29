using System;
using System.Collections.Generic;
using System.Numerics;
using Nova;
using UnityEngine;
using UnityEngine.Events;
using WILCommunityGame;
using Vector3 = UnityEngine.Vector3;

public class PlayerInteractionDetector : MonoBehaviour
{
    private InputReader input;
    private PlayerController player;
    private IInteractable currentTarget;
    private Collider currentIteractableObject;
    public IInteractable CurrentTarget => currentTarget;
    public Collider CurrentIteractableObject => currentIteractableObject;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        input = GetComponent<InputReader>();
    }

    private void Update()
    {
        if (GameUIManager.GameplayInputBlocked) return;

        if (input.InteractPressed)
        {
            DialogueManager dialogueManager = DialogueManager.ActiveDialogue;
            if (dialogueManager != null && dialogueManager.HasStartedDialogue && !dialogueManager.IsPaused)
            {
                dialogueManager.DisplayNextDialogueText();
                return;
            }

            if (currentTarget != null)
            {
                currentTarget.Interact(player);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IInteractable>(out IInteractable interactable))
        {
            currentTarget = interactable;
            currentIteractableObject = other;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IInteractable>(out IInteractable interactable) && interactable == currentTarget)
            currentTarget = null;
        currentIteractableObject = null;
    }
}
