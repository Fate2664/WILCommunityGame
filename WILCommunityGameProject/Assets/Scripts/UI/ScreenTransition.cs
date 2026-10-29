using System;
using System.Collections;
using DG.Tweening;
using Nova;
using UnityEngine;

namespace WILCommunityGame
{
    [RequireComponent(typeof(UIBlock2D))]
    public class ScreenTransition : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float transitionDuration = 0.5f;
        [SerializeField, Min(0f)] private float holdDuration = 1f;

        private const float CoveredWidth = 1.25f;
        private UIBlock2D screen;
        private Sequence transitionSequence;

        private void Awake()
        {
            screen = GetComponent<UIBlock2D>();
            screen.AutoSize.X = AutoSize.None;
            SetWidth(CoveredWidth);
        }

        private IEnumerator Start()
        {
            transitionSequence = DOTween.Sequence()
                .SetUpdate(true)
                .Append(CreateWidthTween(0f))
                .OnComplete(() => transitionSequence = null)
                .Pause();

            yield return null;

            transitionSequence?.Play();
        }

        public void Play(Action onCovered)
        {
            if (transitionSequence != null)
                return;

            transitionSequence = DOTween.Sequence()
                .SetUpdate(true)
                .Append(CreateWidthTween(CoveredWidth))
                .AppendCallback(() => onCovered?.Invoke())
                .AppendInterval(holdDuration)
                .Append(CreateWidthTween(0f))
                .OnComplete(() => transitionSequence = null);
        }

        private Tweener CreateWidthTween(float width)
        {
            return DOTween.To(() => screen.Size.X.Percent, SetWidth, width, transitionDuration)
                .SetEase(Ease.InOutCubic);
        }

        private void SetWidth(float width)
        {
            screen.Size.X.Percent = width;
        }

        private void OnDisable()
        {
            transitionSequence?.Kill();
            transitionSequence = null;
            SetWidth(0f);
        }
    }
}
