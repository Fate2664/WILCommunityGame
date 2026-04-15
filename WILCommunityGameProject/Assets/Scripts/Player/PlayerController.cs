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

        [Header("Movement Settings")] 
        [SerializeField] private float moveSpeed = 6.0f;
        [SerializeField] private float rotationSpeed = 720f;
        [SerializeField] private float playerHeight = 1f;
        [SerializeField] private float playerRadius = .5f;
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
            UpdateMovementState();
            HandleHorizontalMovement();
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

        private void HandleHorizontalMovement()
        {
            Vector2 inputVector = input.MovementInput.normalized;
            Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);

            float moveDistance = moveSpeed * Time.deltaTime;

            bool canMove = CanMoveInDirection(moveDir, moveDistance);

            if (!canMove)
            {
                Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
                canMove = moveDir.x != 0 && CanMoveInDirection(moveDirX, moveDistance);

                if (canMove)
                {
                    moveDir = moveDirX;
                }
                else
                {
                    Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                    canMove = moveDir.z != 0 && CanMoveInDirection(moveDirZ, moveDistance);
                    if (canMove)
                    {
                        moveDir = moveDirZ;
                    }
                }
            }

            if (canMove)
            {
                transform.position += moveDir * moveDistance;
            }
            transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotationSpeed);
        }

        private bool CanMoveInDirection(Vector3 moveDir, float moveDistance)
        {
            return !Physics.CapsuleCast(
                transform.position,
                transform.position + Vector3.up * playerHeight,
                playerRadius,
                moveDir,
                moveDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
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
                uiManager?.RefreshInventory();

                if (buildPlacer.enabled)
                {
                    buildPlacer.enabled = false;
                    uiManager?.UnEquipItem();
                }
            }
        }

        #region State Checks

        public bool IsMovingHorizontally()
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            return horizontalVelocity.magnitude > movingThreshold;
        }

        #endregion
    }
}
