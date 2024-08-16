using System;
using System.Collections;
using System.Collections.Generic;
using BridgePackage;
using UnityEngine;

public class AudioController : MonoBehaviour {
    private void OnEnable() {
        BridgeEvents.BridgeCollapsingState += PlayVictorySound;
        BridgeEvents.BridgeIsCompletedState += PlayDefeatSound;
    }

    private void PlayVictorySound() {
        AudioManager.PlayVictorySound();
    }

    private void PlayDefeatSound() {
        AudioManager.PlayDefeatSound();
    }
}