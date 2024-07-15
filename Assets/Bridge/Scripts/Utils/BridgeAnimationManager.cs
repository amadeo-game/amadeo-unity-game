using System.Collections;
using UnityEngine;

namespace BridgePackage
{
    public class BridgeAnimationManager : MonoBehaviour
    {
        [SerializeField] private AnimationCurve riseAndFallCurve;
        [SerializeField] private float riseAnimDuration = 2f;
        [SerializeField] private float fallAnimDuration = 2f;
        [SerializeField] private float successAnimDuration = 2f;
        private BridgeStateMachine stateMachine;

        private void Awake()
        {
            stateMachine = GetComponent<BridgeStateMachine>();
        }

        public void AnimateBuildUpBridge(GameObject[] bridgeUnits, int heightOffset)
        {
            StartCoroutine(AnimateUnitsWithOffset(bridgeUnits, heightOffset, riseAnimDuration, () => stateMachine.ChangeState(BridgeStates.BridgeReady)));
        }

        public void AnimateBridgeFallDown(GameObject[] bridgeUnits, int heightOffset)
        {
            StartCoroutine(AnimateUnitsWithOffset(bridgeUnits, -heightOffset, fallAnimDuration, () => stateMachine.ChangeState(BridgeStates.GameFailed)));
        }

        public void AnimateSuccess(GameObject[] playerUnits, int[] unitHeights)
        {
            StartCoroutine(AnimatePlayerUnitsSuccess(playerUnits, unitHeights, successAnimDuration, () => stateMachine.ChangeState(BridgeStates.GameWon)));
        }

        private IEnumerator AnimateUnitsWithOffset(GameObject[] bridgeUnits, int heightOffset, float duration, System.Action onComplete)
        {
            var unitsLength = bridgeUnits.Length;
            for (var i = 0; i < unitsLength - 1; i++)
            {
                var unit = bridgeUnits[i];
                if (unit == null) continue;
                var positionY = unit.transform.position.y;
                StartCoroutine(AnimateUnitToDestination(unit, positionY + heightOffset, duration));
                yield return new WaitForSeconds(duration / unitsLength);
            }

            var lastUnit = bridgeUnits[unitsLength - 1];
            if (lastUnit != null)
            {
                var lPositionY = lastUnit.transform.position.y;
                yield return StartCoroutine(AnimateUnitToDestination(lastUnit, lPositionY + heightOffset, duration));
                yield return new WaitForSeconds(duration);
            }

            onComplete?.Invoke();
        }

        private IEnumerator AnimateUnitToDestination(GameObject unit, float destinationY, float duration)
        {
            if (unit == null) yield break;

            Vector3 startPos = unit.transform.position;
            Vector3 endPos = new Vector3(startPos.x, destinationY, startPos.z);
            for (float t = 0; t < 1; t += Time.deltaTime / duration)
            {
                if (unit == null) yield break;
                unit.transform.position = Vector3.Lerp(startPos, endPos, riseAndFallCurve.Evaluate(t));
                yield return null;
            }
            unit.transform.position = endPos;
        }

        private IEnumerator AnimatePlayerUnitsSuccess(GameObject[] playerUnits, int[] unitHeights, float duration, System.Action onComplete)
        {
            var unitsLength = playerUnits.Length;
            for (var i = 0; i < unitsLength; i++)
            {
                var unit = playerUnits[i];
                if (unit == null) continue;
                var positionY = unit.transform.position.y;
                StartCoroutine(AnimateUnitToDestination(unit, positionY + unitHeights[i], duration));
                yield return new WaitForSeconds(duration / unitsLength);
            }

            onComplete?.Invoke();
        }
    }
}
