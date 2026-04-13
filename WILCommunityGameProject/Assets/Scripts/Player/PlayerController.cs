using System;
using Nova;
using UnityEngine;

namespace WILCommunityGame
{
    public class PlayerController : MonoBehaviour
    {
        #region Class Varaibles

        [Header("References")] [SerializeField]
        private Transform playerRoot;
        [SerializeField] private BuildPlacer buildPlacer;
        [SerializeField] private UIManager uiManager;

        [SerializeField] private UIBlock2D InventoryRoot;

        [Header("Movement Settings")] [SerializeField]
        private float moveSpeed = 6.0f;

        [SerializeField] private float walkAcceleration = 20f;
        [Space(10)] [SerializeField] private float runSpeed = 12f;
        [SerializeField] private float runAcceleration = 30f;
        [Space(10)] [SerializeField] private float rotationSpeed = 720f;
        [HideInInspector]
        public bool IsInventoryOpen => InventoryRoot.gameObject.activeSelf;
        
        private InputReader input;
        private PlayerState playerState;
        private Rigidbody rb;
        private float movingThreshold = 0.01f;

        #endregion

        #region Startup Methods

        private void Awake()
        {
            input = GetComponent<InputReader>();
            playerState = GetComponent<PlayerState>();
            rb = GetComponent<Rigidbody>();

            if (playerRoot == null)
            {
                playerRoot = transform;
            }
        }

        #endregion

        #region Update Logic

        private void Update()
        {
            if (input.InventoryTogglePressed)
            {
                ToggleInventory();
            }
        }

        private void FixedUpdate()
        {
            Vector3 moveDirection = GetMoveDirection();
            UpdateMovementState();
            HandleHorizontalMovement(moveDirection);
            RotateTowardsMovement(moveDirection);
        }

        private void UpdateMovementState()
        {
            bool hasMoveInput = input.MovementInput.sqrMagnitude > 0f;
            bool isMovingHorizontally = IsMovingHorizontally();
            bool isSprinting = hasMoveInput && isMovingHorizontally && input.SprintToggledOn;

            PlayerMovementState horizontalState = isSprinting
                ? PlayerMovementState.Sprinting
                : hasMoveInput || isMovingHorizontally
                    ? PlayerMovementState.Walking
                    : PlayerMovementState.Idling;

            playerState.SetPlayerMovementState(horizontalState);
        }

        #endregion

        #region Movement and Rotation Logic

        private Vector3 GetMoveDirection()
        {
            Vector2 movementInput = input.MovementInput;
            return new Vector3(movementInput.x, 0f, movementInput.y);
        }

        private void HandleHorizontalMovement(Vector3 moveDirection)
        {
            bool isSprinting = playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            Vector3 clampedMoveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

            float horizontalAcceleration = isSprinting ? runAcceleration : walkAcceleration;
            float speed = isSprinting ? runSpeed : moveSpeed;

            Vector3 velocity = rb.linearVelocity;
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
            Vector3 targetVelocity = clampedMoveDirection * speed;

            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                targetVelocity,
                horizontalAcceleration * Time.fixedDeltaTime);

            velocity.x = horizontalVelocity.x;
            velocity.z = horizontalVelocity.z;
            rb.linearVelocity = velocity;
        }

        private void RotateTowardsMovement(Vector3 moveDirection)
        {
            Vector3 flatMoveDirection = Vector3.ClampMagnitude(moveDirection, 1f);
            if (flatMoveDirection.sqrMagnitude <= 0f) return;

            Quaternion targetRotation = Quaternion.LookRotation(flatMoveDirection, Vector3.up);
            playerRoot.rotation =
                Quaternion.RotateTowards(playerRoot.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        #endregion

        public void ToggleInventory()
        {
            if (InventoryRoot.gameObject.activeSelf)
            {
                InventoryRoot.gameObject.SetActive(false);
            }
            else
            {
                InventoryRoot.gameObject.SetActive(true);

                if (buildPlacer.enabled)
                {
                    buildPlacer.enabled = false;
                    uiManager?.UnEquipItem();
                }
            }
        }

        #region State Checks

        private bool IsMovingHorizontally()
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            return horizontalVelocity.magnitude > movingThreshold;
        }

        #endregion
    }
}
