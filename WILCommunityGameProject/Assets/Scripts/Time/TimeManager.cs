using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WILCommunityGame
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance {get ; private set;}
        
        [SerializeField] private GameTimestamp timestamp;
        [SerializeField] private float timeScale = 1.0f;
        public Transform sunTransform;
        
        private List<ITimeTracker> listeners = new ();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        private void Start()
        {
            timestamp = new GameTimestamp(8, 0, 0);
            StartCoroutine(UpdateTime());
        }

        IEnumerator UpdateTime()
        {
            while (true)
            {
                yield return new WaitForSeconds(1/timeScale);
                Tick();
            }
        }
        
        public void Tick()
        {
            timestamp.UpdateClock();

            foreach (ITimeTracker listner in listeners)
            {
                listner.ClockUpdate(timestamp);
            }
            
           UpdateSunAngle();
        }

        private void UpdateSunAngle()
        {
            //Move sun angle
            int timeInMinutes = GameTimestamp.HoursToMinutes(timestamp.hour) + timestamp.minute;
            float sunAngle = .25f * timeInMinutes - 90;
            sunTransform.localEulerAngles = new Vector3(sunAngle, 0, 0);
        }

        public void RegisterTracker(ITimeTracker listener)
        {
            listeners.Add(listener);
        }

        public void UnregisterTracker(ITimeTracker listener)
        {
            listeners.Remove(listener);
        }
    }
}
