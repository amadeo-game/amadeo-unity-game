using UnityEngine;
using System;

namespace BridgePackage
{
    public class BridgeTimer : MonoBehaviour
    {
        public event Action OnTimerComplete;

        [SerializeField]
        private float gameTime = 60f; // Default game time in seconds

        private float timer;
        private bool isRunning;

        public void StartTimer(float duration)
        {
            gameTime = duration;
            timer = gameTime;
            isRunning = true;
        }

        private void Update()
        {
            if (isRunning)
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    isRunning = false;
                    OnTimerComplete?.Invoke();
                }
            }
        }

        public void ResetTimer()
        {
            isRunning = false;
            timer = gameTime;
        }
    }
}