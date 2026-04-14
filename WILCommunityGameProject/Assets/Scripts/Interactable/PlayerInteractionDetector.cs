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
   
   private void Awake()
   {
      player = GetComponent<PlayerController>();
      input = GetComponent<InputReader>();
   }
   
   private void Update()
   {
      if (input.InteractPressed)
      {
         if (currentTarget != null || currentTarget != null);
            currentTarget.Interact(player);
            //TODO: When player interacts and the plot is harvestable -> give produce to player
            //TODO: Then reset the plot back to a sprout
      }
   }

   private void OnTriggerEnter(Collider other)
   {
      if (other.TryGetComponent<IInteractable>(out IInteractable interactable))
         currentTarget = interactable;
   }
   
   private void OnTriggerExit(Collider other)
   {
      if (other.TryGetComponent<IInteractable>(out IInteractable interactable) &&  interactable == currentTarget)
         currentTarget = null;
   }

   
}
