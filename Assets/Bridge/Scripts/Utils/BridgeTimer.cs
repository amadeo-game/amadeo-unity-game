using UnityEngine;
using System;
using System.Collections;

namespace BridgePackage {
    public class BridgeTimer : MonoBehaviour {
        public static event Action OnTimerComplete;

        private static bool isRunning;

        public static IEnumerator StartTimer() {
            float timer = BridgeDataManager.TimeDuration;
            isRunning = true;
            Debug.Log("Timer started. Duration: " + timer + " seconds.");

            while (isRunning) {
                
                Debug.Log("Timer: " + timer + " TimeDelta " + Time.deltaTime);
                if (timer <= 0) {
                    OnTimerComplete?.Invoke();
                    ResetTimer();
                    
                }
                timer -= 1;
                yield return new WaitForSecondsRealtime(1);

            }
            yield break;
        }


        public static void ResetTimer() {
            isRunning = false;
        }
    }
}