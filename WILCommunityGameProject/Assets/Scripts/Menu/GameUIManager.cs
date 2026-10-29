using DG.Tweening;
using Nova;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace WILCommunityGame
{
    [DefaultExecutionOrder(-100)]
    public class GameUIManager : MonoBehaviour
    {
        [SerializeField] private UIBlock2D pauseMenuUI;
        [SerializeField] private UIBlock2D pauseDimmer;
        
        public bool IsPaused { get; private set; }
        public static bool GameplayInputBlocked => Time.timeScale == 0f || Time.frameCount == resumeFrame;

        private static int resumeFrame = -1;
        private float previousTimeScale;
        private CursorLockMode previousCursorLockState;
        private bool previousCursorVisible;

        private void Awake()
        {
            resumeFrame = -1;

            if (pauseMenuUI != null)
                pauseMenuUI.gameObject.SetActive(false);

            if (pauseDimmer != null)
            {
                Interactable blocker = pauseDimmer.GetComponent<Interactable>();
                if (blocker == null)
                    blocker = pauseDimmer.gameObject.AddComponent<Interactable>();

                blocker.ObstructDrags = true;
                blocker.Navigable = false;
                pauseDimmer.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                TogglePauseMenu();
        }

        public void LoadGame()
        {
            PrepareSceneChange();
            SceneManager.LoadScene("GameScene");
        }

        public void LoadMainMenu()
        {
            PrepareSceneChange();
            SceneManager.LoadScene("MenuScene");
        }
        
        public void ShowPauseMenu()
        {
            if (pauseMenuUI == null || IsPaused) return;

            previousTimeScale = Time.timeScale;
            previousCursorLockState = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            IsPaused = true;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (pauseDimmer != null)
            {
                pauseDimmer.BodyEnabled = true;
                pauseDimmer.gameObject.SetActive(true);
            }

            pauseMenuUI.gameObject.SetActive(true);
            pauseMenuUI.transform.DOKill();
            pauseMenuUI.transform.localScale = Vector3.zero;
            pauseMenuUI.transform.DOScale(1f, .5f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        public void HidePauseMenu()
        {
            if (!IsPaused) return;

            RestoreGameplay();
            HidePauseVisuals();
        }

        public void TogglePauseMenu()
        {
            if (IsPaused)
                HidePauseMenu();
            else
                ShowPauseMenu();
        }

        public void QuitGame()
        {
            RestoreGameplay();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void RestoreGameplay()
        {
            if (!IsPaused) return;

            IsPaused = false;
            resumeFrame = Time.frameCount;
            Time.timeScale = previousTimeScale;
            Cursor.lockState = previousCursorLockState;
            Cursor.visible = previousCursorVisible;
        }

        private void PrepareSceneChange()
        {
            RestoreGameplay();
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void HidePauseVisuals()
        {
            if (pauseMenuUI != null)
            {
                pauseMenuUI.transform.DOKill();
                pauseMenuUI.gameObject.SetActive(false);
            }

            if (pauseDimmer != null)
                pauseDimmer.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            RestoreGameplay();
            HidePauseVisuals();
        }
    }
}
