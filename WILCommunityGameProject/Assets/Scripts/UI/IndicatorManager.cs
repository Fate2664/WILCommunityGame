using System;
using DG.Tweening;
using Nova;
using UnityEngine;

public class IndicatorManager : MonoBehaviour
{
    [SerializeField] private float scaleDuration = 0.5f;
    [SerializeField] private float height = 1.0f;
    private UIBlock2D indicator;

    private void Awake()
    {
        indicator =  GetComponent<UIBlock2D>();
        if (indicator == null) return;
    }

    void Start()
    {
        indicator.transform.localScale = Vector3.zero;
        indicator.transform.DOLocalMoveY(height, 1f).SetLoops(-1,  LoopType.Yoyo).From(0.9f).SetEase(Ease.InOutQuad);
    }

    public void ShowIndictor()
    {
        indicator.transform.DOScale(Vector3.one, scaleDuration).SetEase(Ease.OutCubic);
    }

    public void HideIndictor()
    {
        indicator.transform.DOScale(Vector3.zero, scaleDuration).SetEase(Ease.OutCubic);
    }

    public void Destroy()
    {
        DOTween.Kill(this);
    }
}
