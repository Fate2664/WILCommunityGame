using System;
using System.Collections.Generic;
using System.Numerics;
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


   private void OnTriggerEnter(Collider other)
   {
      if (!other.TryGetComponent<IInteractable>(out var interactable)) return;
      IndicatorManager indicator = other.GetComponentInChildren<IndicatorManager>();
      if (indicator == null) return;
      
      currentTarget = interactable;
      indicator.ShowIndictor();
   }
   
   private void OnTriggerExit(Collider other)
   {
      if (!other.TryGetComponent(out TestInteractable interactable)) return;
      IndicatorManager indicator = other.GetComponentInChildren<IndicatorManager>();
      currentTarget = null;
      indicator.HideIndictor();
   }

   private void Update()
   {
      if (input.InteractPressed)
      {
         if (currentTarget != null)
         {
            currentTarget.Interact(player);
         }
      }
   }
   
}
