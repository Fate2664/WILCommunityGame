using System;
using Nova;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using WILCommunityGame;

public class TestInteractable : MonoBehaviour, IInteractable
{
    public void Interact(PlayerController interactor)
    {
        Debug.Log("Interacted");
    }
}
