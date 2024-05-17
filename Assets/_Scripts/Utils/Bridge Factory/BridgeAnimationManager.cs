using System;
using System.Collections;
using UnityEngine;

public class BridgeAnimationManager : MonoBehaviour {
    [SerializeField] private AnimationCurve riseCurve;
    [SerializeField] private AnimationCurve shakeIntensityCurve; // Curve for controlling shake intensity
    [SerializeField] private float shakeDuration; // Duration of shaking animation
    [SerializeField] private float riseDuration = 0.1f; // Duration of collapse animation
    [SerializeField] private float collapseDuration = 0.1f; // Duration of collapse animation
    
    
    private BridgeStateMachine stateMachine;

    private void Awake() {
        stateMachine = GetComponent<BridgeStateMachine>();
    }

    private IEnumerator AnimateUnitsCoroutine(GameObject[] bridgeUnits, int height, float delay) {
        foreach (var unit in bridgeUnits) { //TODO: Make the function wait for the last unit before changing state
            StartCoroutine(AnimateUnitToDestination(unit, height));
            yield return new WaitForSeconds(delay);
        }
        yield return new WaitForSeconds(delay*2);
        stateMachine.FinishBuilding();
    }

    private IEnumerator AnimateShakeAndCollapse(GameObject[] bridgeUnits, int height) {
        yield return StartCoroutine(ShakeUnit(bridgeUnits));
        yield return StartCoroutine(AnimateUnitsCoroutine(bridgeUnits, height, collapseDuration));
        stateMachine.CompleteCollapse();
    }

    public void AnimateFallDownUnits(GameObject[] bridgeUnits, int height) {
        StartCoroutine(AnimateShakeAndCollapse(bridgeUnits, -height));

    }

    public void AnimateBuildUpUnits(GameObject[] bridgeUnits, int height) {
        StartCoroutine(AnimateUnitsCoroutine(bridgeUnits, height, riseDuration));
    }



    IEnumerator AnimateUnitToDestination(GameObject bridgeUnit, int yHeight) {
        Vector2 startPosition = bridgeUnit.transform.position;
        Vector2 endPosition = new Vector3(startPosition.x, startPosition.y + yHeight);

        float duration = 1.0f; // Duration of the rise animation in seconds
        float elapsed = 0.0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration; // Goes from 0 to 1

            // Calculate the current position based on the animation curve
            float curveValue = riseCurve.Evaluate(normalizedTime); // Goes from 0 to 1
            Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, curveValue);

            bridgeUnit.transform.position = currentPosition;

            yield return null;
        }
    }


// TODO: Similar functions for shake and collapse animations



    // IEnumerator AnimateUnitShakeAndCollapse(GameObject bridgeUnits, int targetHeight) {
    //     Vector2 startPosition = bridgeUnits.transform.position;
    //     Vector2 endPosition = new Vector3(startPosition.x, targetHeight);
    //
    //     float duration = 1.0f; // Duration of the rise animation in seconds
    //     float elapsed = 0.0f;
    //
    //     while (elapsed < duration) {
    //         elapsed += Time.deltaTime;
    //         float normalizedTime = elapsed / duration; // Goes from 0 to 1
    //
    //         // Calculate the current position based on the animation curve
    //         float curveValue = riseCurve.Evaluate(normalizedTime); // Goes from 0 to 1
    //         Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, curveValue);
    //
    //         bridgeUnits.transform.position = currentPosition;
    //
    //         yield return null;
    //     }
    // }

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


/*

 */