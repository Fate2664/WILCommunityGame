using UnityEngine;

namespace WILCommunityGame
{
    public enum PlayerMovementState
    {
        Idling = 0,
        Walking = 1,
        Sprinting = 2,
    }
    
    public class PlayerState : MonoBehaviour
    {
        //This allows us to see in editor but not edit it
        [field: SerializeField]
        public PlayerMovementState CurrentPlayerMovementState { get; private set; } = PlayerMovementState.Idling;
 
        public void SetPlayerMovementState(PlayerMovementState playerMovementState)
        {
            CurrentPlayerMovementState = playerMovementState;
        }
    }
}
