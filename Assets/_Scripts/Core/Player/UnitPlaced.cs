using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPlaced : MonoBehaviour
{
    [SerializeField] FingerUnit fingerUnit;

    private void OnTriggerEnter2D(Collider2D other) {
        Debug.Log("Got Enter Triggered on " + fingerUnit);
        GameManager.OnOnUnitsPlaced(fingerUnit, isPlaced: true);
    }

    private void OnTriggerExit2D(Collider2D other) {
        Debug.Log("Got Exit Triggered on " + fingerUnit);

        GameManager.OnOnUnitsPlaced(fingerUnit, isPlaced: false);
    }
}
