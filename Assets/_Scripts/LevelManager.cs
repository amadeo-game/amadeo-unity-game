using System;
using BridgePackage;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class LevelManager : MonoBehaviour {
    [SerializeField] private BridgeCollectionSO bridgeCollectionSO;

    [FormerlySerializedAs("sessionManager")] [SerializeField]
    private BridgeDataManager _bridgeDataManager;

    [SerializeReference] bool useDynamicDifficulty = false;
    private int levelIndex { get; set; } = 1;
    private int sessionCount { get; set; } = 0;

    private bool GameEnded { get; set; } = false;

    private void OnEnable()
    {
        // GameStatesEvents.GameSessionInitialized += StartSession;
        BridgeEvents.BridgeReadyState += EnableUnits;

        BridgeEvents.BridgeIsCompletedState += OnSessionEnd;
        BridgeEvents.GameWonState += PrepareNextSession;

        BridgeEvents.IdleState += StartNextSession;

    }

    private void StartNextSession() {
        if (!BridgeDataManager.AutoStart) {
            return;
        }
        
        StartSession();
    }

    private void PrepareNextSession() {
        InitializeSession();
        BridgeEvents.RestartGameAction?.Invoke();
    }

    private void OnDisable()
    {
        // GameStatesEvents.GameSessionInitialized -= StartSession;
        BridgeEvents.BridgeReadyState -= EnableUnits;
        BridgeEvents.IdleState -= StartNextSession;
        BridgeEvents.GameWonState -= PrepareNextSession;
        BridgeEvents.BridgeIsCompletedState -= OnSessionEnd;

    }


    public void InitializeSession() {
        if (!BridgeDataManager.AutoStart) {
            return;
        }

        Debug.Log("LevelManager :: SetupNewLevel() called.");
        // Initialize the new game session
        // Retrieve or generate new parameters for the game

        if (sessionCount > 2) {
            if (levelIndex > bridgeCollectionSO.BridgeTypes.Length - 1) {
                levelIndex = 1;
                GameEnded = true;
            }
            else {
                levelIndex++;
            }
            sessionCount = 0;
        }

        if (useDynamicDifficulty && sessionCount is > 0) {
            // Adjust the difficulty based on the previous session data
            var sessionData = BridgeDataManager.SessionData;
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
    }


    public void StartSession() {
        string hand = BridgeDataManager.IsLeftHand ? "Left" : "Right";
        Debug.Log("LevelManager :: StartSession() called., chosen Hand is " + hand);
        // Start the game session
        BridgeEvents.BuildingBridgeAction?.Invoke();
        GameplayEvents.GameStarted?.Invoke();
        sessionCount++;
    }

    public void ForceEndSession() {
        Debug.Log("LevelManager :: StopSession() called.");
        // Stop the game session
        // Perform any cleanup or save data
        BridgeEvents.CollapseBridgeAction?.Invoke();
    }

    public void PauseSession() {
        Debug.Log("LevelManager :: PauseSession() called.");
        // Pause the game session
        // Perform any cleanup or save data
        BridgeEvents.PauseGameAction?.Invoke();
    }

    public void OnSessionEnd() {
        Debug.Log("LevelManager :: EndSession() called.");
        // End the game session
        // Perform any cleanup or save data
        BridgeEvents.FinishedGameCompletedState?.Invoke();
    }

    private void EnableUnits() {
        BridgeEvents.EnableGameInteraction?.Invoke();
    }

    public void AdjustDifficultyBasedOnSessionData(SessionData sessionData) {
        // Adjust the difficulty based on the session data
        // For example, modify the heights or time duration based on performance
        // This example increases the height range and reduces the time if the player succeeded
        if (sessionData.success) {
            var adjustedHeights = AdjustHeightsForSuccess(sessionData.heights);
            BridgeDataManager.SetHeights(adjustedHeights);
        }
        else {
            var adjustedHeights = AdjustHeightsForFailure(sessionData.heights, sessionData.BestYPositions);
            BridgeDataManager.SetHeights(adjustedHeights);
            BridgeDataManager.SetTimeDuration(BridgeDataManager.TimeDuration *
                                              1.1f); // Increase the time slightly for less challenge
            // increase 1 more unit to each grace
            var adjustedGrace = new float[5];
            for (int i = 0; i < adjustedGrace.Length; i++) {
                adjustedGrace[i] = BridgeDataManager.UnitsGrace[i] + 1;
            }
            
            BridgeDataManager.SetUnitsGrace(adjustedGrace);
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
        int changeFinger = (int)(Random.Range(0f, 4.9f));// TODO: Write better
        // Adjust heights to be more challenging
        for (int i = 0; i < previousHeights.Length; i++) {
            if (BridgeDataManager.PlayableUnits[i])
            {
                previousHeights[i] = Mathf.Clamp(previousHeights[i] + 1, 1, 5);
            }
        }
        return previousHeights;
    }

    private int[] AdjustHeightsForFailure(int[] previousHeights, float[] bestYPositions) {
        // Adjust heights to be easier
        for (int i = 0; i < previousHeights.Length; i++) {
            var diffHeight = Mathf.Abs(previousHeights[i] - bestYPositions[i]);
            if (diffHeight > 0) {
                previousHeights[i] = Mathf.Clamp(previousHeights[i] - 1, 1, 5);
            }
        }
        return previousHeights;
    }

    public void ResumeSession() {
        Debug.Log("LevelManager :: ResumeSession() called.");
        // Resume the game session
        // Retrieve the saved data and resume the game
        BridgeEvents.ResumeGameAction?.Invoke();
    }
}