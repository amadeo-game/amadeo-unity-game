using System.Collections;
using UnityEngine;

public class BridgeAnimationManager : MonoBehaviour {
    [SerializeField] private AnimationCurve riseCurve;
    [SerializeField] private AnimationCurve shakeIntensityCurve; // Curve for controlling shake intensity
    [SerializeField] private float shakeDuration; // Duration of shaking animation

    public IEnumerator AnimateBuildUpCoroutine(GameObject[] bridgeUnits, int[] heights) {
        int len = bridgeUnits.Length;
        if (len != heights.Length) {
            Debug.Log( "heights len is " + heights.Length + " and bridgeUnits len is " + len);
            Debug.LogError("Bridge units and heights arrays must have the same length");
            yield break;
            
        }
        for (int i = 0; i < len; i++) {
            StartCoroutine(AnimateUnitBuildUp(bridgeUnits[i], heights[i]));
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(2);
    }

    IEnumerator AnimateUnitBuildUp(GameObject bridgeUnit, int height) {
        Vector2 startPosition = bridgeUnit.transform.position;
        Vector2 endPosition = new Vector3(startPosition.x, height);

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

    public IEnumerator AnimateShakeAndCollapse(GameObject[] bridgeUnits) {
        // Shake animation
        yield return StartCoroutine(AnimateShake(bridgeUnits));

        // Collapse animation
        yield return StartCoroutine(AnimateCollapse(bridgeUnits));
    }

    IEnumerator AnimateShake(GameObject[] bridgeUnits) {
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

    IEnumerator AnimateCollapse(GameObject[] bridgeUnits) {
        float collapseSpeed = 5.0f; // Adjust speed as needed

        for (int i = 0; i < bridgeUnits.Length; i++) {
            while (bridgeUnits[i].transform.position.y > -10) // Adjust Y position based on camera view
            {
                bridgeUnits[i].transform.position += Vector3.down * collapseSpeed * Time.deltaTime;
                yield return null;
            }

            // Destroy bridge unit after reaching the collapse position
            Destroy(bridgeUnits[i]);
        }
    }
}