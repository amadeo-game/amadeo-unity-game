using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndTrialFeedback : MonoBehaviour {
    [SerializeField] private Image beaverText;
    // private void OnEnable() {
    //     GameManager.OnUnitsPlaced += OnUnitsPlaced;
    // }

    private void Start() {
        beaverText.gameObject.SetActive(false);
    }

    private void OnUnitsPlaced() {
        beaverText.gameObject.SetActive(true);
    }
}
