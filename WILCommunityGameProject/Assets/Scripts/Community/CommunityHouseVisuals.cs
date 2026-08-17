using System;
using DG.Tweening;
using Nova;
using UnityEngine;

namespace WILCommunityGame
{
    public class CommunityHouseVisuals : MonoBehaviour
    {
        [Header("Connections")] [SerializeField]
        private PlayerInteractionDetector playerInteractionDetector;

        [SerializeField] private UIBlock2D cropsBackground;
        [SerializeField] private UIBlock2D bowlBackground;
        [SerializeField] private UIBlock2D bowlPointer;
        [SerializeField] private UIBlock2D bowlBlocker1;
        [SerializeField] private UIBlock2D bowlBlocker2;
        [SerializeField] private UIBlock2D bowlBlocker3;

        [Header("Visuals")] [SerializeField] private float scaleDuration = 0.5f;
        [SerializeField] private Color defualtColor;
        [SerializeField] private Color redColor;
        [SerializeField] private Color greenColor;
        [SerializeField] private float redFlashDuration = .35f;

        [Header("Bowl Sprites")] [SerializeField]
        private Sprite emptyBowlSprite;

        [SerializeField] private Sprite neutalBowlSprite;
        [SerializeField] private Sprite fullBowlSprite;

        private Vector3 startScale;
        private CommunityHouse communityHouse;
        private Tween bowlColourTween;
        private Tween bowlPointerColourTween;
        private Tween bowlBlocker1ColourTween;
        private Tween bowlBlocker2ColourTween;
        private Tween bowlBlocker3ColourTween;

        private void Awake()
        {
            communityHouse = GetComponentInParent<CommunityHouse>();

            startScale = cropsBackground.transform.localScale;
            cropsBackground.transform.localScale = Vector3.zero;
        }

        private void FixedUpdate()
        {
            if (playerInteractionDetector.CurrentTarget != null &&
                playerInteractionDetector.CurrentTarget == communityHouse)
            {
                ShowIcons();
            }
            else
            {
                HideIcons();
            }
        }

        private void OnDisable()
        {
            bowlColourTween?.Kill();
            bowlPointerColourTween?.Kill();
            bowlBlocker1ColourTween?.Kill();
            bowlBlocker2ColourTween?.Kill();
            bowlBlocker3ColourTween?.Kill();
        }

        public void ShowIcons()
        {
            cropsBackground.transform.DOScale(startScale, scaleDuration).SetEase(Ease.OutCubic);
        }

        public void HideIcons()
        {
            cropsBackground.transform.DOScale(Vector3.zero, scaleDuration).SetEase(Ease.OutCubic);
        }

        public Sprite UpdateBowlImage(bool deliveredAnything, bool requestsComplete)
        {
            return requestsComplete ? fullBowlSprite : deliveredAnything ? neutalBowlSprite : emptyBowlSprite;
        }

        public void UpdateBowlBackground(bool deliveredAnything, bool requestsComplete)
        {
            bowlColourTween?.Kill();
            bowlPointerColourTween?.Kill();
            bowlBlocker1ColourTween?.Kill();
            bowlBlocker2ColourTween?.Kill();
            bowlBlocker3ColourTween?.Kill();

            if (requestsComplete)
            {
                bowlBackground.Color = greenColor;
                bowlPointer.Color = greenColor;
                bowlBlocker1.Color = greenColor;
                bowlBlocker2.Color = greenColor;
                bowlBlocker3.Color = greenColor;
                return;
            }

            if (deliveredAnything)
            {
                bowlBackground.Color = defualtColor;
                bowlPointer.Color = defualtColor;
                bowlBlocker1.Color = defualtColor;
                bowlBlocker2.Color = defualtColor;
                bowlBlocker3.Color = defualtColor;
                return;
            }

            bowlBackground.Color = defualtColor;
            bowlPointer.Color = defualtColor;
            bowlBlocker1.Color = defualtColor;
            bowlBlocker2.Color = defualtColor;
            bowlBlocker3.Color = defualtColor;
            
            bowlColourTween = DOTween
                .To(() => bowlBackground.Color, color => bowlBackground.Color = color, redColor, redFlashDuration)
                .SetLoops(-1, LoopType.Yoyo);
            bowlPointerColourTween = DOTween
                .To(() => bowlPointer.Color, color => bowlPointer.Color = color, redColor, redFlashDuration)
                .SetLoops(-1, LoopType.Yoyo);
            bowlBlocker1ColourTween = DOTween
                .To(() => bowlBlocker1.Color, color => bowlBlocker1.Color = color, redColor, redFlashDuration)
                .SetLoops(-1, LoopType.Yoyo);
            bowlBlocker2ColourTween = DOTween
                .To(() => bowlBlocker2.Color, color => bowlBlocker2.Color = color, redColor, redFlashDuration)
                .SetLoops(-1, LoopType.Yoyo);
            bowlBlocker3ColourTween = DOTween
                .To(() => bowlBlocker3.Color, color => bowlBlocker3.Color = color, redColor, redFlashDuration)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
}