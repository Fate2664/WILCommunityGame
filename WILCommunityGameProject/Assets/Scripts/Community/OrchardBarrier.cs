using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WILCommunityGame
{
    public class OrchardBarrier : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private CommunityUIManager communityUIManager;

        [SerializeField] private Transform popup;

        [Header("Animation")] [SerializeField] private float scaleDuration;

        private Vector3 originalScale;
        private Tween scaleTween;
        private bool orchardUnlocked;

        private void Awake()
        {
            originalScale = popup.localScale;
            popup.localScale = Vector3.zero;
        }

        private void OnEnable()
        {
            communityUIManager.OnStreakStageChanged += HandleStreakStageChanged;
            CheckOrchardUnlock();
        }

        private void Start()
        {
            CheckOrchardUnlock();
        }


        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
                DisableBarrier();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (orchardUnlocked || !other.CompareTag("Player"))
                return;

            AnimatePopup(originalScale);
        }

        private void OnTriggerExit(Collider other)
        {
            if (orchardUnlocked || !other.CompareTag("Player"))
                return;

            AnimatePopup(Vector3.zero);
        }

        private void AnimatePopup(Vector3 targetScale)
        {
            scaleTween?.Kill();

            scaleTween = popup.DOScale(targetScale, scaleDuration).SetEase(Ease.OutCubic)
                .OnKill(() => scaleTween = null);
        }

        private void HandleStreakStageChanged(int stage)
        {
            if (stage >= 3)
                DisableBarrier();
        }

        private void CheckOrchardUnlock()
        {
            if (orchardUnlocked || communityUIManager.StreakStage >= 3)
                DisableBarrier();
        }

        private void DisableBarrier()
        {
            orchardUnlocked = true;
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (communityUIManager != null)
            {
                communityUIManager.OnStreakStageChanged -= HandleStreakStageChanged;
            }

            scaleTween?.Kill();
            scaleTween = null;

            if (popup != null)
                popup.localScale = Vector3.zero;
        }
    }
}
