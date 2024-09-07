using System;
using System.Collections;
using System.Collections.Generic;
using BridgePackage;
using UnityEngine;

public class AudioController : MonoBehaviour {
    private void OnEnable() {
        GameEvents.TrialCompleting += PlayVictorySound;
        GameEvents.TrialFailing += PlayDefeatSound;
    }
    
    private void OnDisable() {
        GameEvents.TrialCompleting -= PlayVictorySound;
        GameEvents.TrialFailing -= PlayDefeatSound;
    }

    private void PlayVictorySound() {
        AudioManager.PlayVictorySound();
    }

    private void PlayDefeatSound() {
        AudioManager.PlayDefeatSound();
    }
}