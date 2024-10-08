using System;
using System.Collections;
using UnityEngine;

namespace BridgePackage {
    public class BridgeAnimationManager : MonoBehaviour {
        [SerializeField] private AnimationCurve riseAndFallCurve;
        [SerializeField] private float riseAnimDuration = 2f;
        [SerializeField] private float fallAnimDuration = 2f;
        [SerializeField] private float successAnimDuration = 2f;
        private BridgeStateMachine stateMachine;
        
        private Coroutine buildUpBridgeCoroutine;
        private Coroutine bridgeFallDownCoroutine;
        private Coroutine successCoroutine;
        
        private void Awake() {
            stateMachine = GetComponent<BridgeStateMachine>();
        }

        private void OnEnable() {
            BridgeEvents.AnimatingBuildingState += AnimateBuildUpBridge;
            BridgeEvents.AnimatingBridgeCollapsingState += AnimateBridgeFallDown;
            BridgeEvents.AnimatingBridgeCompletingState += AnimateSuccess;
        }

        private void OnDisable() {
            BridgeEvents.AnimatingBuildingState -= AnimateBuildUpBridge;
            BridgeEvents.AnimatingBridgeCollapsingState -= AnimateBridgeFallDown;
            BridgeEvents.AnimatingBridgeCompletingState -= AnimateSuccess;
        }

        public void AnimateBuildUpBridge() {
            GameObject[] bridgeUnits = Bridge.TotalBridgeUnits;
            int heightOffset = Bridge.BridgeRiseDownOffset;
            // check if the coroutine is running
            if (buildUpBridgeCoroutine != null) {
                StopCoroutine(buildUpBridgeCoroutine);
            }
            buildUpBridgeCoroutine = StartCoroutine(AnimateUnitsWithOffset(bridgeUnits, heightOffset, riseAnimDuration,
                BridgeEvents.FinishedAnimatingBuildingState));
        }

        public void AnimateBridgeFallDown() {
            GameObject[] bridgeUnits = Bridge.TotalBridgeUnits;
            int heightOffset = Bridge.BridgeRiseDownOffset;
            // check if the coroutine is running
            if (bridgeFallDownCoroutine != null) {
                StopCoroutine(bridgeFallDownCoroutine);
            }
            bridgeFallDownCoroutine = StartCoroutine(AnimateUnitsWithOffset(bridgeUnits, -heightOffset, fallAnimDuration,
                BridgeEvents.FinishedAnimatingBridgeCollapsingState));
        }

        public void AnimateSuccess() {
            GameObject[] playerUnits = Bridge.PlayerUnits;
            int[] unitHeights = Bridge.PlayerUnitsHeights;
            // check if the coroutine is running
            if (successCoroutine != null) {
                StopCoroutine(successCoroutine);
            }
            successCoroutine = StartCoroutine(AnimatePlayerUnitsSuccess(playerUnits, unitHeights, successAnimDuration,
                BridgeEvents.FinishedAnimatingBridgeCompletingState));
        }

        private IEnumerator AnimateUnitsWithOffset(GameObject[] bridgeUnits, int heightOffset, float duration,
            Action onComplete) {
            var unitsLength = bridgeUnits.Length;
            for (var i = 0; i < unitsLength - 1; i++) {
                var unit = bridgeUnits[i];
                if (unit == null) continue;
                var positionY = unit.transform.position.y;
                StartCoroutine(AnimateUnitToDestination(unit, positionY + heightOffset, duration));
                yield return new WaitForSeconds(duration / unitsLength);
            }

            var lastUnit = bridgeUnits[unitsLength - 1];
            if (lastUnit != null) {
                var lPositionY = lastUnit.transform.position.y;
                yield return StartCoroutine(AnimateUnitToDestination(lastUnit, lPositionY + heightOffset, duration));
                yield return new WaitForSeconds(duration);
            }

            onComplete?.Invoke();
        }

        private IEnumerator AnimateUnitToDestination(GameObject unit, float destinationY, float duration) {
            if (unit == null) yield break;

            Vector3 startPos = unit.transform.position;
            Vector3 endPos = new Vector3(startPos.x, destinationY, startPos.z);
            for (float t = 0; t < 1; t += Time.deltaTime / duration) {
                if (unit == null) yield break;
                unit.transform.position = Vector3.Lerp(startPos, endPos, riseAndFallCurve.Evaluate(t));
                yield return null;
            }
            if (unit == null) yield break;
            unit.transform.position = endPos;
            yield return null;
        }

        private IEnumerator AnimatePlayerUnitsSuccess(GameObject[] playerUnits, int[] unitHeights, float duration,
            System.Action onComplete) {
            var unitsLength = playerUnits.Length;
            for (var i = 0; i < unitsLength; i++) {
                var unit = playerUnits[i];
                if (unit == null) continue;
                var positionY = unit.transform.position.y;

                StartCoroutine(AnimateUnitToDestination(unit, unitHeights[i], duration));
                yield return new WaitForSeconds(duration / unitsLength);
            }

            onComplete?.Invoke();
        }
    }
}