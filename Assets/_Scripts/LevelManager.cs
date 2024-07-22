using System;
using BridgePackage;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelManager : MonoBehaviour {
    [SerializeField] private BridgeAPI bridgeAPI;
    [SerializeField] private BridgeCollectionSO bridgeCollectionSO;
    [SerializeField] private SessionManager sessionManager;

    [SerializeReference] bool useDynamicDifficulty = false;
    private int levelIndex { get; set; } = 1;
    private int sessionCount { get; set; } = 0;

    private void OnEnable() {
        // GameStatesEvents.GameSessionInitialized += StartSession;
        BridgeAPI.BridgeReady += EnableUnits;
    }
    
    private void OnDestroy() {
        // GameStatesEvents.GameSessionInitialized -= StartSession;
        GameStatesEvents.GameSessionStarted -= EnableUnits;
    }

    public void InitializeSession() {
        Debug.Log("LevelManager :: SetupNewLevel() called.");
        // Initialize the new game session
        // Retrieve or generate new parameters for the game

        if (useDynamicDifficulty && sessionCount > 0) {
            // Adjust the difficulty based on the previous session data
            var sessionData = bridgeAPI.GetSessionData();
            AdjustDifficultyBasedOnSessionData(sessionData);
        }
        else {
            // var heights = GenerateRandomHeights();
            // sessionManager.SetHeights(heights);
            sessionManager.SetBridgeType(GetBridgeTypeSO(levelIndex));
            sessionManager.SetIsLeftHand(GetIsLeftHand());
            sessionManager.SetIsFlexion(GetIsFlexion());
            sessionManager.SetMvcValues(GetMVCValues());
            sessionManager.SetPlayableUnits(GetPlayableUnits());
        }

        sessionCount++;
        GameStatesEvents.GameSessionInitialized?.Invoke();

    }

    public void StartSession() {
        Debug.Log("LevelManager :: StartSession() called.");
        bridgeAPI.BuildBridge(
            sessionManager.Heights,
            sessionManager.BridgeType,
            sessionManager.IsLeftHand,
            sessionManager.IsFlexion,
            sessionManager.MvcValues,
            sessionManager.PlayableUnits,
            sessionManager.UnitsGrace,
            sessionManager.TimeDuration
        );
        GameStatesEvents.GameSessionStarted?.Invoke();

    }
    
    private void EnableUnits() {
        bridgeAPI.EnableGameUnits(sessionManager.ZeroF);
    }

    public void AdjustDifficultyBasedOnSessionData(SessionData sessionData) {
        // Adjust the difficulty based on the session data
        // For example, modify the heights or time duration based on performance
        // This example increases the height range and reduces the time if the player succeeded
        if (sessionData.success) {
            var adjustedHeights = AdjustHeightsForSuccess(sessionData.heights);
            sessionManager.SetHeights(adjustedHeights);
            sessionManager.SetTimeDuration(sessionManager.TimeDuration *
                                           0.9f); // Decrease the time slightly for more challenge
        }
        else {
            var adjustedHeights = AdjustHeightsForFailure(sessionData.heights);
            sessionManager.SetHeights(adjustedHeights);
            sessionManager.SetTimeDuration(sessionManager.TimeDuration *
                                           1.1f); // Increase the time slightly for less challenge
        }
    }

    private int[] GenerateRandomHeights() {
        int[] heights = new int[5];
        for (int i = 0; i < heights.Length; i++) {
            heights[i] = Random.Range(0, 6); // Random heights between 0 and 5
        }

        return heights;
    }

    private BridgeTypeSO GetBridgeTypeSO(int levelIndex) {
        // Retrieve the BridgeTypeSO, can be from a predefined list or configuration
        return bridgeCollectionSO.BridgeTypes[levelIndex];
    }

    private bool GetIsLeftHand() {
        // Determine if the game is for the left hand, can be from a configuration or user input
        return true; // Example
    }

    private bool GetIsFlexion() {
        // Determine if the game is in flexion mode, can be from a configuration or user input
        return true; // Example
    }

    private float[] GetMVCValues() {
        // Retrieve or generate the MVC values
        return new float[] { 1, 1, 1, 1, 1 }; // Example
    }

    private bool[] GetPlayableUnits() {
        // Determine which units are playable
        return new bool[] { true, true, true, true, true }; // Example
    }

    private int[] AdjustHeightsForSuccess(int[] previousHeights) {
        // Adjust heights to be more challenging
        for (int i = 0; i < previousHeights.Length; i++) {
            previousHeights[i] = Mathf.Clamp(previousHeights[i] + 1, 0, 5);
        }

        return previousHeights;
    }

    private int[] AdjustHeightsForFailure(int[] previousHeights) {
        // Adjust heights to be easier
        for (int i = 0; i < previousHeights.Length; i++) {
            previousHeights[i] = Mathf.Clamp(previousHeights[i] - 1, 0, 5);
        }

        return previousHeights;
    }
}