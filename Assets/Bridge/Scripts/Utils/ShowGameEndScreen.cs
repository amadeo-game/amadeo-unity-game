using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BridgePackage
{
    public class ShowGameEndScreen : MonoBehaviour {
        [SerializeField] private GameObject gameEndScreen;
        private void OnEnable() {
            GameEvents.GameBuilding += HideScreen;
            GameEvents.SessionEnded += ShowScreen;
        }
        
        private void OnDisable() {
            GameEvents.GameBuilding -= HideScreen;
            GameEvents.SessionEnded -= ShowScreen;
        }
        
        private void ShowScreen() {
            // Show the game end screen
            gameEndScreen.SetActive(true);
        }
        
        private void HideScreen() {
            // Hide the game end screen
            gameEndScreen.SetActive(false);
        }
    }
}
