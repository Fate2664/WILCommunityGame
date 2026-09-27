using System;
using DG.Tweening;
using UnityEngine;

namespace WILCommunityGame
{
    public class TutorialBarrier : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private ObjectiveListUI objectiveListUI;

        [SerializeField] private Objective tutorialObjective;
        [SerializeField] private Transform popup;

        [Header("Animation")] [SerializeField] private float scaleDuration;

        private Vector3 originalScale;
        private Tween scaleTween;
        private bool tutorialCompleted;

        private void Awake()
        {
            originalScale = popup.localScale;
            popup.localScale = Vector3.zero;
        }

        private void OnEnable()
        {
            objectiveListUI.OnObjectiveCompleted += HandleObjectiveCompleted;
            CheckTutorialCompletion();
        }

        private void Start()
        {
            CheckTutorialCompletion();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (tutorialCompleted || !other.CompareTag("Player"))
                return;

            AnimatePopup(originalScale);
        }

        private void OnTriggerExit(Collider other)
        {
            if (tutorialCompleted || !other.CompareTag("Player"))
                return;
            
            AnimatePopup(Vector3.zero);
        }

        private void AnimatePopup(Vector3 targetScale)
        {
            scaleTween?.Kill();

            scaleTween = popup.DOScale(targetScale, scaleDuration).SetEase(Ease.OutCubic)
                .OnKill(() => scaleTween = null);
        }

        private void HandleObjectiveCompleted(Objective objective)
        {
            if (tutorialObjective != null && objective == tutorialObjective)
                DisableBarrier();
        }

        private void CheckTutorialCompletion()
        {
            if (tutorialCompleted || (tutorialObjective != null &&
                                      objectiveListUI.CurrentObjective == tutorialObjective &&
                                      objectiveListUI.IsComplete))
            {
                DisableBarrier();
            }
        }

        private void DisableBarrier()
        {
            tutorialCompleted = true;
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (objectiveListUI != null)
            {
                objectiveListUI.OnObjectiveCompleted -= HandleObjectiveCompleted;
            }

            scaleTween?.Kill();
            scaleTween = null;

            if (popup != null)
                popup.localScale = Vector3.zero;
        }
    }
}