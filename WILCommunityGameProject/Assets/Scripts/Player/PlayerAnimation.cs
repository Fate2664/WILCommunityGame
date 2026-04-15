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
        
        private Vector3 currentBlendInput =  Vector3.zero;

        private void Awake()
        {
            inputReader = GetComponent<InputReader>();
            playerState = GetComponent<PlayerState>();
            if (animator == null) return;
        }

        private void LateUpdate()
        {
            UpdateAnimationState();
        }

        private void UpdateAnimationState()
        {
            bool isIdling = playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
            bool isWalking = !isIdling && playerState.CurrentPlayerMovementState == PlayerMovementState.Walking;
            
            animator.SetBool("IsWalking", isWalking);
        }
    }
}
