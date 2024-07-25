using System;
using BridgePackage;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class LevelManager : MonoBehaviour {
    [SerializeField] private BridgeAPI bridgeAPI;
    [SerializeField] private BridgeCollectionSO bridgeCollectionSO;
    [FormerlySerializedAs("sessionManager")] [SerializeField] private BridgeDataManager _bridgeDataManager;

    [SerializeReference] bool useDynamicDifficulty = false;
    private int levelIndex { get; set; } = 1;
    private int sessionCount { get; set; } = 0;

    private void OnEnable() {
        // GameStatesEvents.GameSessionInitialized += StartSession;
        BridgeEvents.BridgeReady += EnableUnits;
    }

    private void OnDisable() {
        // GameStatesEvents.GameSessionInitialized -= StartSession;
        BridgeEvents.BridgeReady -= EnableUnits;
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
            BridgeDataManager.SetLevel(levelIndex);
            BridgeDataManager.SetIsLeftHand(GetIsLeftHand());
            BridgeDataManager.SetIsFlexion(GetIsFlexion());
            BridgeDataManager.SetMvcValuesExtension(GetMvcValuesExtension());
            BridgeDataManager.SetMvcValuesFlexion(GetMVCValuesFlexion());
            BridgeDataManager.SetPlayableUnits(GetPlayableUnits());
        }

        sessionCount++;
        GameStatesEvents.GameSessionInitialized?.Invoke();
    }

    public void StartSession() {
        string hand = BridgeDataManager.IsLeftHand ? "Left" : "Right";
        Debug.Log("LevelManager :: StartSession() called., chosen Hand is " + hand);
        bridgeAPI.BuildBridge(

        );
        GameStatesEvents.GameSessionStarted?.Invoke();

    }
    
    public void ForceEndSession() {
        Debug.Log("LevelManager :: StopSession() called.");
        // Stop the game session
        // Perform any cleanup or save data
        bridgeAPI.CollapseBridge();
    }
    
    public void PauseSession() {
        Debug.Log("LevelManager :: PauseSession() called.");
        // Pause the game session
        // Perform any cleanup or save data
        bridgeAPI.PauseBridge();
    }
    
    public void OnSessionEnd() {
        Debug.Log("LevelManager :: EndSession() called.");
        // End the game session
        // Perform any cleanup or save data
        GameStatesEvents.GameSessionEnded?.Invoke();
    }
    
    private void EnableUnits() {
        bridgeAPI.EnableGameUnits(BridgeDataManager.ZeroF);
    }

    public void AdjustDifficultyBasedOnSessionData(SessionData sessionData) {
        // Adjust the difficulty based on the session data
        // For example, modify the heights or time duration based on performance
        // This example increases the height range and reduces the time if the player succeeded
        if (sessionData.success) {
            var adjustedHeights = AdjustHeightsForSuccess(sessionData.heights);
            BridgeDataManager.SetHeights(adjustedHeights);
            BridgeDataManager.SetTimeDuration(BridgeDataManager.TimeDuration *
                                              0.9f); // Decrease the time slightly for more challenge
        }
        else {
            var adjustedHeights = AdjustHeightsForFailure(sessionData.heights);
            BridgeDataManager.SetHeights(adjustedHeights);
            BridgeDataManager.SetTimeDuration(BridgeDataManager.TimeDuration *
                                              1.1f); // Increase the time slightly for less challenge
        }
    }

    private int[] AssignRandomHeights() {
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

    private float[] GetMvcValuesExtension() {
        // Retrieve or generate the MVC values
        return new float[] { 1, 1, 1, 1, 1 }; // Example
    }
    
    private float[] GetMVCValuesFlexion() {
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

    public void ResumeSession() {
        Debug.Log("LevelManager :: ResumeSession() called.");
        // Resume the game session
        // Retrieve the saved data and resume the game
        bridgeAPI.ResumeBridge();
    }
}