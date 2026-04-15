using System;
using UnityEngine;

namespace WILCommunityGame
{
    public class IconMaterialProperty : MonoBehaviour
    {
        [SerializeField] private Material waterIconMAT;
        [SerializeField] private Material seedIconMAT;
        [SerializeField] private Material produceIconMAT;
        
        private SpriteRenderer _spriteRenderer;
        private Sprite icon;
        
        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();    
            icon = _spriteRenderer.sprite;
        }

        private void Update()
        {
                
        }
    }
}
