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
            
            settingsUI.transform.DOKill();
            settingsUI.transform.DOScale(1f, .5f).SetEase(Ease.OutBack);
        }

        public void HideSettings()
        {
            if (settingsUI == null) return;
            
            settingsUI.transform.DOKill();
            settingsUI.transform.DOScale(0f, .3f).SetEase(Ease.OutQuad);
        }
    }
    
}
