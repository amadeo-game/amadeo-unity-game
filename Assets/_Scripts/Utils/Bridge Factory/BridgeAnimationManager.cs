using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class BridgeAnimationManager : MonoBehaviour {
    [SerializeField] private AnimationCurve riseAndFallCurve;
    [SerializeField] private AnimationCurve shakeIntensityCurve; // Curve for controlling shake intensity
    
    [SerializeField] private float shakeDuration = 0.1f; // Duration of shaking animation
    [SerializeField] private float riseAnimDuration = 0.1f; // Duration of collapse animation
    [SerializeField] private float collapseAnimDuration = 0.1f; // Duration of collapse animation
    [SerializeField] private float successAnimDuration = 0.1f; // Duration of success animation


    private BridgeStateMachine stateMachine;

    private void Awake() {
        stateMachine = GetComponent<BridgeStateMachine>();
    }

    // Called by BridgeGenerator
    public void AnimateBridgeFallDown(GameObject[] bridgeUnits, int height) {
        StartCoroutine(AnimateShakeAndCollapse(bridgeUnits, -height));
    }

    public void AnimateBuildUpBridge(GameObject[] bridgeUnits, int height) {
        StartCoroutine(AnimateBuildUpUnits(bridgeUnits, height));
    }

    public void AnimateSuccess(GameObject[] playerUnits, int[] heights) {
        StartCoroutine(AnimateSuccessOnUnits(playerUnits, heights));
    }

    // methods for handling bridge start animations and notifying the state machine when they are done
    private IEnumerator AnimateBuildUpUnits(GameObject[] bridgeUnits, int height) {
        yield return StartCoroutine(AnimateUnitsWithOffset(bridgeUnits, height, riseAnimDuration));
        stateMachine.FinishBuilding();
    }

    private IEnumerator AnimateShakeAndCollapse(GameObject[] bridgeUnits, int height) {
        yield return StartCoroutine(ShakeUnit(bridgeUnits));
        yield return StartCoroutine(AnimateUnitsWithOffset(bridgeUnits, height, collapseAnimDuration));
        stateMachine.CompleteCollapse();
    }

    private IEnumerator AnimateSuccessOnUnits(GameObject[] playerUnits, int[] heights) {
        yield return StartCoroutine(AnimateUnitsToPosition(playerUnits, heights, successAnimDuration));
        stateMachine.FinishSuccess();
    }

    // methods for animating the bridge units
    private IEnumerator AnimateUnitsWithOffset(GameObject[] bridgeUnits, int height, float delay) {
        var unitsLength = bridgeUnits.Length;
        for (var i = 0; i < unitsLength - 1; i++) {
            var unit = bridgeUnits[i];
            //TODO: Make the function wait for the last unit before changing state
            var positionY = unit.transform.position.y;
            StartCoroutine(AnimateUnitToDestination(unit, positionY + height));
            yield return new WaitForSeconds(delay);
        }

        // to handle the last unit before other operations
        var lastUnit = bridgeUnits[unitsLength - 1];
        var lPositionY = lastUnit.transform.position.y;
        yield return StartCoroutine(AnimateUnitToDestination(lastUnit, lPositionY + height));
        yield return new WaitForSeconds(delay * 2);
    }

    private IEnumerator AnimateUnitsToPosition(GameObject[] bridgeUnits, int[] heights, float delay) {
        if (bridgeUnits.Length != heights.Length) {
            throw new ArgumentException("bridgeUnits and heights must be of the same length.");
        }

        var unitsLength = bridgeUnits.Length;
        for (var i = 0; i < unitsLength - 1; i++) {
            var unit = bridgeUnits[i];
            //TODO: Make the function wait for the last unit before changing state
            StartCoroutine(AnimateUnitToDestination(unit, heights[i]));
            yield return new WaitForSeconds(delay);
        }

        // to handle the last unit before other operations
        yield return StartCoroutine(AnimateUnitToDestination(bridgeUnits[unitsLength - 1], heights[unitsLength - 1]));
        yield return new WaitForSeconds(delay * 2);
    }

    // Coroutine for animating a single unit to a destination
    IEnumerator AnimateUnitToDestination(GameObject bridgeUnit, float yHeight) {
        Vector2 startPosition = bridgeUnit.transform.position;
        Vector2 endPosition = new Vector3(startPosition.x, yHeight);

        float duration = 1.0f; // Duration of the rise animation in seconds
        float elapsed = 0.0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration; // Goes from 0 to 1

            // Calculate the current position based on the animation curve
            float curveValue = riseAndFallCurve.Evaluate(normalizedTime); // Goes from 0 to 1
            Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, curveValue);

            bridgeUnit.transform.position = currentPosition;

            yield return null;
        }
    }

    // Coroutine for shaking a group of units
    IEnumerator ShakeUnit(GameObject[] bridgeUnits) {
        float elapsedTime = 0;
        Vector3[] originalPositions = new Vector3[bridgeUnits.Length];

        for (int i = 0; i < bridgeUnits.Length; i++) {
            originalPositions[i] = bridgeUnits[i].transform.localPosition;
        }

        while (elapsedTime < shakeDuration) {
            float shakeIntensity = shakeIntensityCurve.Evaluate(elapsedTime / shakeDuration);

            for (int i = 0; i < bridgeUnits.Length; i++) {
                Vector3 randomOffset = new Vector3(Mathf.PerlinNoise(Time.time * shakeIntensity, 0) - 0.5f, 0, 0);
                bridgeUnits[i].transform.localPosition = originalPositions[i] + randomOffset;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset positions after shaking
        for (int i = 0; i < bridgeUnits.Length; i++) {
            bridgeUnits[i].transform.localPosition = originalPositions[i];
        }
    }
}