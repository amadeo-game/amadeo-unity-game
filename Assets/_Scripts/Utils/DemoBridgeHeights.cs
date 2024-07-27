using UnityEngine;

// Attached to the GameObject GameManager 

public class DemoBridgeHeights : MonoBehaviour {
    [SerializeField, Range(0, 5)] // TODO: support flexion mode (negative values)
    private int[] playerUnitsHeights = { 0, 0, 0, 0, 0 }; // Set this in the Inspector

    // getter for playerUnitsHeights
    public int[] GetPlayerUnitsHeights() {
        return playerUnitsHeights;
    }

    public void OnValueChanged0(float value) {
        playerUnitsHeights[0] = (int)value;
    }

    public void OnValueChanged1(float value) {
        playerUnitsHeights[1] = (int)value;
    }

    public void OnValueChanged2(float value) {
        playerUnitsHeights[2] = (int)value;
    }

    public void OnValueChanged3(float value) {
        playerUnitsHeights[3] = (int)value;
    }

    public void OnValueChanged4(float value) {
        playerUnitsHeights[4] = (int)value;
    }
}