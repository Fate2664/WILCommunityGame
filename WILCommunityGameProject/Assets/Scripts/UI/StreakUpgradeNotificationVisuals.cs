using DG.Tweening;
using Nova;
using UnityEngine;

namespace WILCommunityGame
{
    public class StreakUpgradeNotificationVisuals : MonoBehaviour
    {
        [SerializeField] private CommunityUIManager communityUIManager;
        [SerializeField] private Transform rootTransform;
        [SerializeField] private UIBlock2D stageIcon;
        [SerializeField] private TextBlock contextText;
        [SerializeField] private float slideDuration = 0.35f;
        [SerializeField] private float showDuration = 10.0f;
        [SerializeField] private float onScreenY = 413.0f;

        private float offScreenY;
        private Sequence slideTween;

        private void Awake()
        {
            offScreenY = rootTransform.localPosition.y;
        }

        private void OnEnable()
        {
            communityUIManager.OnStreakStageChanged += ShowUpgrade;
        }

        private void OnDisable()
        {
            communityUIManager.OnStreakStageChanged -= ShowUpgrade;
            slideTween?.Kill();

            Vector3 position = rootTransform.localPosition;
            position.y = offScreenY;
            rootTransform.localPosition = position;
        }

        private void ShowUpgrade(int stage)
        {
            CommunityStreakStage streakStage = communityUIManager.StreakStages[stage - 1];
            stageIcon.SetImage(streakStage.Icon);
            contextText.Text = streakStage.Description;

            slideTween?.Kill();

            slideTween = DOTween.Sequence()
                .Append(rootTransform.DOLocalMoveY(onScreenY, slideDuration).SetEase(Ease.OutCubic))
                .AppendInterval(showDuration)
                .Append(rootTransform.DOLocalMoveY(offScreenY, slideDuration).SetEase(Ease.OutCubic));
        }
    }
}
