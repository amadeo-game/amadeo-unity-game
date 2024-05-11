using System.Collections;
using UnityEngine;

public class BridgeAnimationManager : MonoBehaviour {
    [SerializeField] private AnimationCurve riseCurve;

    public IEnumerator AnimateBuildUpCoroutine(GameObject[] bridgeUnits, GameObject[] bridgeEnvUnits,
        int[] playerUnitHeights, int[] envUnitHeights) {
        for (int i = 0; i < bridgeUnits.Length; i++) {
            // Animate environment unit
            StartCoroutine(AnimateUnitBuildUp(bridgeEnvUnits[i], envUnitHeights[i]));
            yield return new WaitForSeconds(0.1f);
            // Animate Player Unit
            StartCoroutine(AnimateUnitBuildUp(bridgeUnits[i], playerUnitHeights[i]));
            yield return new WaitForSeconds(0.1f);
        }
        StartCoroutine(AnimateUnitBuildUp(bridgeEnvUnits[^1], envUnitHeights[^1]));
        

        yield return new WaitForSeconds(2);
    }

    IEnumerator AnimateUnitBuildUp(GameObject bridgeUnit, int height) {
        Vector2 startPosition = bridgeUnit.transform.position;
        Vector2 endPosition  = new Vector3(startPosition.x, height);

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

}

// TODO: Similar functions for shake and collapse animations