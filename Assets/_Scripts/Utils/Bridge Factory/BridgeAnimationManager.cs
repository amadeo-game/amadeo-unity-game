using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeAnimationManager : MonoBehaviour
{
    public void AnimateBuildUp(Bridge bridge)
    {
        // Access bridge units through the bridge object
        if (bridge != null)
        {
            StartCoroutine(AnimateBuildUpCoroutine(bridge.bridgePlayerUnits, bridge.PlayerBridgeUnitsPlaceHolders, bridge.PlayerUnitsHeights));
        }
    }

    IEnumerator AnimateBuildUpCoroutine(GameObject[] bridgeUnits, GameObject[] unitPlaceHolders, int[] heights)
    {
        for (int i = 0; i < bridgeUnits.Length; i++)
        {
            // Access and animate bridge unit and its placeholder (similar to previous example)
            yield return StartCoroutine(AnimateUnitBuildUp(bridgeUnits[i], unitPlaceHolders[i], heights[i]));
        }
    }

    IEnumerator AnimateUnitBuildUp(GameObject bridgeUnit, GameObject unitPlaceHolder, int height)
    {
        // Implement animation logic as before
        yield return null;
    }

    // Similar functions for shake and collapse animations
}

