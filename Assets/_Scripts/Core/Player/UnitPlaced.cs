using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPlaced : MonoBehaviour {
    public FingerUnit fingerUnit;

    private void OnTriggerEnter2D(Collider2D other) {
        Debug.Log("Got Enter Triggered on " + fingerUnit);
        try {
            GameManager.OnOnUnitsPlaced(fingerUnit, isPlaced: true);
        }
        catch (Exception e) {
            Debug.LogWarning($"{e.Message} on {fingerUnit}, \n GameManager is null"); ;
            // throw;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        Debug.Log("Got Exit Triggered on " + fingerUnit);
        try {
            GameManager.OnOnUnitsPlaced(fingerUnit, isPlaced: false);
        }
        catch (Exception e) {
            Debug.LogWarning($"{e.Message} on {fingerUnit}, \n GameManager is null");
            // throw;
        }
    }
}