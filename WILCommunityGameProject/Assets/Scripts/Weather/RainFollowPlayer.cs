using System;
using UnityEngine;

namespace WILCommunityGame
{
    public class RainFollowPlayer : MonoBehaviour
    {
        [SerializeField] private GameObject player;
        
        private void FixedUpdate()
        {
            transform.localPosition = new Vector3(player.transform.position.x, transform.localPosition.y, player.transform.position.z);
        }
    }
}
