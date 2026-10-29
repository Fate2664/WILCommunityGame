using UnityEngine;

namespace WILCommunityGame
{
    public class HideAfterFirstSleep : MonoBehaviour
    {
        private TimeManager timeManager;

        private void OnEnable()
        {
            SubscribeToSleep();
        }

        private void Start()
        {
            SubscribeToSleep();
        }

        private void SubscribeToSleep()
        {
            if (timeManager != null)
                return;

            timeManager = TimeManager.Instance;
            if (timeManager == null)
                return;

            if (timeManager.HasSlept)
                Hide();
            else
                timeManager.OnFirstSleep += Hide;
        }

        private void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (timeManager != null)
                timeManager.OnFirstSleep -= Hide;

            timeManager = null;
        }
    }
}
