using System;
using DG.Tweening;
using Nova;
using UnityEngine;

namespace WILCommunityGame
{
    public class InformationTipVisuals : MonoBehaviour
    {
        [SerializeField] private UIManager uiManager;
        [SerializeField] private Transform rootTransform;
        [SerializeField] private TextBlock infoText;
        [SerializeField] private float slideDuration = 0.35f;
        [SerializeField] private float showDuration = 10.0f;

        private float offScreenY;
        private float onScreenY = 450.0f;
        private Sequence slideTween;

        private void Awake()
        {
            offScreenY = rootTransform.localPosition.y;
        }

        private void OnEnable()
        {
            uiManager.OnInformationTipRequested += ShowTip;
        }

        private void OnDisable()
        {
            uiManager.OnInformationTipRequested -= ShowTip;
            slideTween?.Kill();
        }

        private void ShowTip(InformationTipSO tip)
        {
            infoText.Text = tip.Text;

            slideTween?.Kill();

            slideTween = DOTween.Sequence()
                .Append(rootTransform.DOLocalMoveY(onScreenY, slideDuration).SetEase(Ease.OutCubic))
                .AppendInterval(showDuration)
                .Append(rootTransform.DOLocalMoveY(offScreenY, slideDuration).SetEase(Ease.OutCubic));
        }
    }
}