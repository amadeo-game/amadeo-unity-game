using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BridgePackage;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class LevelManager : MonoBehaviour {
//     [SerializeField] private BridgeCollectionSO bridgeCollectionSO;
//
//     [SerializeField] private float _timeBetweenSessions = 1f;
//     [SerializeField] private bool _allowFingerChange = false;
//     
//     [SerializeField] private Canvas _gameEndCanvas;
//
//     // [FormerlySerializedAs("sessionManager")] [SerializeField]
//     //
//     //
//     // private BridgeDataManager _bridgeDataManager;
//     [SerializeField] bool[] _playableUnits = new bool[5];
//     [SerializeReference] bool useDynamicDifficulty = false;
//     private int levelIndex { get; set; } = 1;
//     private int sessionCount { get; set; } = 0;
//
//     [SerializeField] private int _sessionsPerLevel = 3;
//
//     private bool GameEnded { get; set; } = false;
//
//     // private void OnEnable() {
//     //     // GameStatesEvents.GameSessionInitialized += StartSession;
//     //     BridgeEvents.BridgeReadyState += EnableUnits;
//     //
//     //     BridgeEvents.BridgeIsCompletedState += OnSessionEnd;
//     //     BridgeEvents.TrialCompleted += PrepareNextSession;
//     //     BridgeEvents.TrialFailed += PrepareNextSession;
//     //
//     //     BridgeEvents.IdleState += StartNextSession;
//     // }
//     
//     private void ShowGameEndCanvas(bool show = true) {
//         _gameEndCanvas.gameObject.SetActive(show);
//     }
//
//
//     private void PrepareNextSession() {
//         InitializeSession();
//         StartCoroutine(RestartGameAfterDelay());
//     }
//
//     // coroutine to wait a little bit before starting the next session
//
//     IEnumerator RestartGameAfterDelay() {
//         yield return new WaitForSeconds(_timeBetweenSessions);
//         BridgeEvents.RestartGameAction?.Invoke();
//         yield return null;
//     }
//
//
//     // private void OnDisable() {
//     //     // GameStatesEvents.GameSessionInitialized -= StartSession;
//     //     BridgeEvents.BridgeReadyState -= EnableUnits;
//     //     BridgeEvents.IdleState -= StartNextSession;
//     //     BridgeEvents.TrialCompleted -= PrepareNextSession;
//     //     BridgeEvents.TrialFailed += PrepareNextSession;
//     //
//     //     BridgeEvents.BridgeIsCompletedState -= OnSessionEnd;
//     // }
//
//
//     public void InitializeSession() {
//         if (!BridgeDataManager.AutoStart) {
//             return;
//         }
//         levelIndex = BridgeDataManager.Level;
//
//         _playableUnits = BridgeDataManager.PlayableUnits;
//         Debug.Log("LevelManager :: SetupNewLevel() called.");
//         // Initialize the new game session
//
//         // 3 sessions per level, then move to the next level
//         // level cannot be higher than the number of BridgeTypes - 1
//
//         if (sessionCount > _sessionsPerLevel) {
//             levelIndex++;
//             sessionCount = 0;
//             if (levelIndex > bridgeCollectionSO.BridgeTypes.Length - 1) {
//                 levelIndex = 1;
//                 GameEnded = true;
//                 return;
//             }
//         }
//
//         BridgeDataManager.SetLevel(levelIndex);
//         if (useDynamicDifficulty && sessionCount > 0) {
//             // Adjust the difficulty based on the previous session data
//             var sessionData = BridgeDataManager.SessionData;
//             AdjustDifficultyBasedOnSessionData(sessionData);
//         }
//         else {
//             BridgeDataManager.SetLevel(levelIndex);
//             BridgeDataManager.SetIsLeftHand(GetIsLeftHand());
//             BridgeDataManager.SetIsFlexion(GetIsFlexion());
//             BridgeDataManager.SetMvcValuesExtension(GetMvcValuesExtension());
//             BridgeDataManager.SetMvcValuesFlexion(GetMVCValuesFlexion());
//             // BridgeDataManager.SetPlayableUnits(GetPlayableUnits());
//         }
//
//         sessionCount++;
//     }
//
//     private void StartNextSession() {
//         if (!BridgeDataManager.AutoStart) {
//             return;
//         }
//
//
//         Debug.Log("LevelManager :: StartNextSession() called.");
//         StartSession();
//     }
//
//     // public void StartSession() {
//     //
//     //     if (GameEnded) {
//     //             Debug.Log("LevelManager :: GameEnded, no more levels to play.");
//     //             ShowGameEndCanvas();
//     //             GameEnded = false;
//     //
//     //         return;
//     //     }
//     //         
//     //     ShowGameEndCanvas(false);
//     //     string hand = BridgeDataManager.IsLeftHand ? "Left" : "Right";
//     //     Debug.Log("LevelManager :: StartSession() called., chosen Hand is " + hand);
//     //     // Start the game session
//     //     BridgeEvents.PlayTrial?.Invoke();
//     //     GameplayEvents.GameStarted?.Invoke();
//     //     sessionCount++;
//     // }
//
//     public void ForceEndSession() {
//         Debug.Log("LevelManager :: StopSession() called.");
//         // Stop the game session
//         // Perform any cleanup or save data
//         GameActions.ForceDestroyBridge?.Invoke();
//     }
//
//     public void PauseSession() {
//         Debug.Log("LevelManager :: PauseSession() called.");
//         // Pause the game session
//         // Perform any cleanup or save data
//         GameActions.PauseGameAction?.Invoke();
//     }
//
//
//
//     private void EnableUnits() {
//         GameActions.EnableGameInteraction?.Invoke();
//     }
//
//     public void AdjustDifficultyBasedOnSessionData(SessionData sessionData) {
//         // Adjust the difficulty based on the session data
//         // For example, modify the heights or time duration based on performance
//         // This example increases the height range and reduces the time if the player succeeded
//         var adjustedHeights = ApplyDynamicHeight(sessionData.heights, sessionData.success, sessionData.BestYPositions);
//         
//         float[] graces = BridgeDataManager.UnitsGrace;
//         for (int i = 0; i < graces.Length; i++) {
//             float newGrace;
//             if (sessionData.success) {
//                 newGrace = Mathf.Min(1, graces[i] - 1);
//             }
//             else {
//                 newGrace = Mathf.Max(4, graces[i] + 1);
//             }
//
//             BridgeDataManager.SetUnitsGrace(i, newGrace);
//         }
//         
//
//         // else {
//         //     // var adjustedHeights = AdjustHeightsForFailure(sessionData.heights, sessionData.BestYPositions);
//         //     BridgeDataManager.SetHeights(adjustedHeights);
//         //     BridgeDataManager.SetTimeDuration(BridgeDataManager.TimeDuration *
//         //                                       1.1f); // Increase the time slightly for less challenge
//         //     // increase 1 more unit to each grace
//         //     var adjustedGrace = new float[5];
//         //     for (int i = 0; i < adjustedGrace.Length; i++) {
//         //         adjustedGrace[i] = BridgeDataManager.UnitsGrace[i] + 1;
//         //     }
//
//
//         BridgeDataManager.SetHeights(adjustedHeights);
//         // BridgeDataManager.SetPlayableUnits(_playableUnits);
//     }
//
//     private int[] AssignRandomHeights() {
//         int[] heights = new int[5];
//         for (int i = 0; i < heights.Length; i++) {
//             heights[i] = Random.Range(0, 6); // Random heights between 0 and 5
//         }
//
//         return heights;
//     }
//
//     private BridgeTypeSO GetBridgeTypeSO(int levelIndex) {
//         // Retrieve the BridgeTypeSO, can be from a predefined list or configuration
//         return bridgeCollectionSO.BridgeTypes[levelIndex];
//     }
//
//     private bool GetIsLeftHand() {
//         // Determine if the game is for the left hand, can be from a configuration or user input
//         return true; // Example
//     }
//
//     private bool GetIsFlexion() {
//         // Determine if the game is in flexion mode, can be from a configuration or user input
//         return true; // Example
//     }
//
//     private float[] GetMvcValuesExtension() {
//         // Retrieve or generate the MVC values
//         return new float[] { 1, 1, 1, 1, 1 }; // Example
//     }
//
//     private float[] GetMVCValuesFlexion() {
//         // Retrieve or generate the MVC values
//         return new float[] { 1, 1, 1, 1, 1 }; // Example
//     }
//
//     private bool[] GetPlayableUnits() {
//         // Determine which units are playable
//         return new bool[] { true, true, true, true, true }; // Example
//     }
//
//     private int[] ApplyDynamicHeight(int[] previousHeights, bool success, float[] bestYPosition) {
//         // random number between 1 and 5
//         float[] mvcs = BridgeDataManager.IsFlexion
//             ? BridgeDataManager.MvcValuesFlexion
//             : BridgeDataManager.MvcValuesExtension;
//         // MVC is BridgeDataManager
//         // only the playable units i will be adjusted, 50% chance to be increase by 1 if the unit is less then min(5 ,(int)(mvcs[i]/5))
//         // for the rest of the units, they will be placed randomly between 0 and 5, because they are not playable
//         int maxSimultaneousUnits = (levelIndex / 3);
//
//         // take only the number of maxSimultaneousUnits and choose randomly which finger to active and the rest turn off in playableUnits
//         // int unitToActive = Random.Range(0, 5);
//
//         // choose maxSimultaneousUnits random numbers between 0 and 5 and turn them on in playableUnits, the rest turn off
//
//         if (_allowFingerChange) {
//             HashSet<int> unitsToActive = new HashSet<int>();
//             for (int i = 0; i < maxSimultaneousUnits+1; i++) {
//                 var unit = Random.Range(0, 5);
//                 int tries = 0;
//                 while (unitsToActive.Contains(unit) || tries < 5) {
//                     tries++;
//                 }
//
//                 unitsToActive.Add(unit);
//             }
//
//             for (int i = 0; i < _playableUnits.Length; i++) {
//                 _playableUnits[i] = false;
//             }
//
//             foreach (var unit in unitsToActive) {
//                 _playableUnits[unit] = true;
//             }
//         }
//
//         Debug.Log("LevelManager :: playableUnits " + string.Join(",", _playableUnits));
//         
//         if (success) {
//             // Adjust heights to be harder
//             for (int i = 0; i < previousHeights.Length; i++) {
//                 if (_playableUnits[i]) {
//                     if (Random.Range(0, 2) == 1) {
//                         Debug.Log("Adjusting height for unit " + i + " to be harder.");
//                         var topLimit = Mathf.Min(5, (int)(mvcs[i] / 5) + 1);
//                         Debug.Log("Top limit for unit " + i + " is " + topLimit);
//                         previousHeights[i] = Mathf.Min(previousHeights[i] + 1, topLimit);
//                     }
//                     else {
//                         previousHeights[i] = Mathf.Max(previousHeights[i], 1);
//                     }
//                 }
//                 else {
//                     previousHeights[i] = Random.Range(0, 6);
//                 }
//             }
//         }
//         else {
//             // Adjust heights to be easier
//             for (int i = 0; i < previousHeights.Length; i++) {
//                 if (_playableUnits[i]) {
//                     previousHeights[i] = Mathf.Max(1, previousHeights[i] - 1);
//                 }
//                 else {
//                     previousHeights[i] = Random.Range(0, 6);
//                 }
//             }
//         }
//
//         return previousHeights;
//     }
//
//     public void ResumeSession() {
//         Debug.Log("LevelManager :: ResumeSession() called.");
//         // Resume the game session
//         // Retrieve the saved data and resume the game
//         BridgeEvents.ResumeGameAction?.Invoke();
//     }
//
// //     // Retrieve or generate new parameters for the game
// //
// //     if (sessionCount > 2) {
// //         if (levelIndex > bridgeCollectionSO.BridgeTypes.Length - 1) {
// //             levelIndex = 1;
// //             GameEnded = true;
// //         }
// //         else {
// //             levelIndex++;
// //         }
// //         sessionCount = 0;
// //     }
// //
// //     if (useDynamicDifficulty && sessionCount > 0) {
// //         // Adjust the difficulty based on the previous session data
// //         var sessionData = BridgeDataManager.SessionData;
// //         AdjustDifficultyBasedOnSessionData(sessionData);
// //     }
// // else {
// //     BridgeDataManager.SetLevel(levelIndex);
// //     BridgeDataManager.SetIsLeftHand(GetIsLeftHand());
// //     BridgeDataManager.SetIsFlexion(GetIsFlexion());
// //     BridgeDataManager.SetMvcValuesExtension(GetMvcValuesExtension());
// //     BridgeDataManager.SetMvcValuesFlexion(GetMVCValuesFlexion());
// //     // BridgeDataManager.SetPlayableUnits(GetPlayableUnits());
// // }
}