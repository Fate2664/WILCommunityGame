using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace WILCommunityGame
{
    public class InputReader : MonoBehaviour, PlayerInputActions.IPlayerActions, PlayerInputActions.IUIActions
    {
        #region Class Variables

        [SerializeField] private bool holdToSprint = true;
        
        public PlayerInputActions InputActions { get; private set; }
        public Vector2 MovementInput {get ; private set;}
        public bool SprintToggledOn { get; private set; }
        public bool NextPressed { get; private set; }
        public bool InteractPressed { get; private set; }
        public bool InventoryTogglePressed { get; private set; }

        #endregion

        #region Startup & Update Methods

        private void OnEnable()
        {
            if (InputActions == null)
            {
                InputActions = new PlayerInputActions();
                InputActions.Player.SetCallbacks(this);
                InputActions.UI.SetCallbacks(this);
            }
            InputActions.Enable();
        }
        
        private void OnDisable()
        {
            if (InputActions != null)
            {
                InputActions.Disable();
            }
        }

        private void LateUpdate()
        {
            NextPressed = false;
            InteractPressed = false;
            InventoryTogglePressed = false;
        }
        #endregion

        #region Gameplay Inputs

        public void OnMove(InputAction.CallbackContext context)
        {
            MovementInput = context.ReadValue<Vector2>();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            return;
            // if (context.performed)
            // {
            //     SprintToggledOn = holdToSprint || !SprintToggledOn;
            // }
            // else if (context.canceled)
            // {
            //     SprintToggledOn = !holdToSprint && SprintToggledOn;
            // }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            InteractPressed = true;
        }

        public void OnToggleInventory(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            InventoryTogglePressed = true;
        }

        #endregion

        #region UI Inputs

        public void OnExit(InputAction.CallbackContext context)
        {
            return;
        }

        public void OnClick(InputAction.CallbackContext context)
        {
            return;
        }

        public void OnRightClick(InputAction.CallbackContext context)
        {
            return;
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            
            NextPressed = true;
        }

        public void OnNavigate(InputAction.CallbackContext context)
        {
            return;
        }

        public void OnApply(InputAction.CallbackContext context)
        {
        }

        #endregion

    }
}

