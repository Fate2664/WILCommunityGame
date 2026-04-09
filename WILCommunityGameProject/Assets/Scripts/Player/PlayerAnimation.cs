using System;
using UnityEngine;

namespace WILCommunityGame
{
    public class PlayerAnimation : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float locomotionBlendSpeed = 4f;

        private InputReader inputReader;
        private PlayerState playerState;
        
        private static int inputXHash =  Animator.StringToHash("MoveX");
        private static int inputYHash =  Animator.StringToHash("MoveY");
        private static int inputMagnitudeHash =  Animator.StringToHash("MoveMagnitude");
        
        private Vector3 currentBlendInput =  Vector3.zero;

        private void Awake()
        {
            inputReader = GetComponent<InputReader>();
            if (animator == null) return;
        }

        private void LateUpdate()
        {
            //UpdateAnimationState();
        }

        private void UpdateAnimationState()
        {
            bool isIdling = playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
            bool isSprinting = playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            
            Vector2 inputTarget = isSprinting ? inputReader.MovementInput * 1.5f : inputReader.MovementInput;
            currentBlendInput = Vector3.Lerp(currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);
            
            animator.SetFloat(inputXHash, currentBlendInput.x);
            animator.SetFloat(inputYHash, currentBlendInput.y);
            animator.SetFloat(inputMagnitudeHash, currentBlendInput.magnitude);
        }
    }
}
