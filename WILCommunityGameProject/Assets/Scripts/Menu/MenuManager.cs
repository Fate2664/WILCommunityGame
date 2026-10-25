using DG.Tweening;
using Nova;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace WILCommunityGame
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private UIBlock2D settingsUI;
        [SerializeField] private Interactable startButton;
        [SerializeField] private Interactable settingsButton;
        [SerializeField] private UIBlock2D settingsDarker;
        [SerializeField] private Interactable exitButton;
        
        
        public void LoadGame()
        {
            SceneManager.LoadScene("GameScene");
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MenuScene");
        }

        public void ShowSettings()
        {
            if (settingsUI == null) return;

            startButton.enabled = false;
            settingsButton.enabled = false;
            exitButton.enabled = false;
            settingsDarker.gameObject.SetActive(true);
            
            settingsUI.transform.DOKill();
            settingsUI.transform.DOScale(1f, .5f).SetEase(Ease.OutBack);
        }

        public void HideSettings()
        {
            if (settingsUI == null) return;
            
            startButton.enabled = true;
            settingsButton.enabled = true;
            exitButton.enabled = true;
            settingsDarker.gameObject.SetActive(false);
            
            settingsUI.transform.DOKill();
            settingsUI.transform.DOScale(0f, .3f).SetEase(Ease.OutQuad);
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
    
}
