using System;
using DG.Tweening;
using Nova;
using UnityEngine;

[Serializable]
//This script manages the viusals and animations behind the dialogue block 
public class DialogueVisuals
{
    public UIBlock2D Background;
    public TextBlock DialogueText;
    public TextBlock NameText;
    public float PopinDuration = 0.35f;
    private Vector3 originalScale;
    private Tween scaleTween;

    public void Initialize()
    {
        originalScale = Background.transform.localScale;
        Reset();
    }

    public void Show()
    {
        Background.Visible = true;
        AnimatePopup(originalScale);
    }

    public void Hide()
    {
        AnimatePopup(Vector3.zero);
    }

    private void AnimatePopup(Vector3 targetScale)
    {
        scaleTween?.Kill();
        scaleTween = Background.transform.DOScale(targetScale, PopinDuration).SetEase(Ease.OutCubic)
            .OnKill(() => scaleTween = null);
    }

    public void Reset()
    {
        scaleTween?.Kill();
        scaleTween = null;
        Background.transform.localScale = Vector3.zero;
        Background.Visible = false;
    }
}
