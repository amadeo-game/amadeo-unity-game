using UnityEngine;
using System;
using System.Collections;

namespace BridgePackage {
    public class BridgeTimer : MonoBehaviour {
        public static event Action OnTimerComplete;
        private static float timer;
        private static bool resumePausedTimer;
        private static bool isRunning;
        
        

        public static IEnumerator StartTimer() {
            if (!resumePausedTimer) {
                timer = BridgeDataManager.TimeDuration;
            }
            resumePausedTimer = false;
            isRunning = true;

            while (isRunning) {
                if (timer <= 0) {
                    OnTimerComplete?.Invoke();
                    ResetTimer();
                    
                }
                BridgeEvents.OnTimeDurationChanged?.Invoke(--timer);
                yield return new WaitForSecondsRealtime(1);

            }
            yield break;
        }

        public static void PauseTimer() {
            isRunning = false;
            resumePausedTimer = true;
        }
        
        public static void ResetTimer() {
            isRunning = false;
            timer = BridgeDataManager.TimeDuration;
        }
    }
}