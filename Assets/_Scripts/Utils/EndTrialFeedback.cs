using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EndTrialFeedback : MonoBehaviour {
    [SerializeField] private Image anouncementText;
    // private void OnEnable() {
    //     GameManager.OnUnitsPlaced += OnUnitsPlaced;
    // }

    private void Start() {
        anouncementText.gameObject.SetActive(false);
    }

    private void OnUnitsPlaced() {
        anouncementText.gameObject.SetActive(true);
    }
}
