using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace BridgePackage {
    internal enum FingerMethod {
        IndividualControl,
        IsolatedControl,
        SimultaneousMultiFingerControl
    }

    public class DynamicDifficulty : MonoBehaviour {
        [SerializeField] private bool _debug;

        // The current finger method, easiest is Individual Control, then Isolated-Control, then multi-finger Simultaneous-MultiFinger-Control
        private FingerMethod _fingerMethod;
        private bool _allowIndividuation;
        private bool _allowMultiFingerSimultaneous;

        private HashSet<int> _allowedFingers;
        private HashSet<int> _nonActiveFingers;

        private HashSet<int> _activeFingers;


        private readonly int[] _mvcToHeights = new int[5];

        private bool _firstTrial = true;

        // Set to when the Isolated-Control starts
        [SerializeField] private int _isolatedControlStartLevel = 2;

        // Should be greater than the Isolated-Control start level, because it is usually harder
        [SerializeField] private int _multiFingerControlStartLevel = 5;

        // When this number of failed trials is reached, the finger method is lowered.
        [SerializeField] private int _failuresBeforeModeReduction = 2;

        // When this number of successful trials is reached, the height difficulty is increased.
        [SerializeField] private int _successesBeforeHeightIncrease = 2;

        // The number of trials in each level, before moving to the next level
        [SerializeField] private int _trialsPerLevel = 5;

        [SerializeField] private int _startingAmountOfStaticFingers = 1;

        [SerializeField] private int _startingAmountOfMultiFingerControl = 2;

        // The number of static fingers in Isolated-Control
        private int _staticFingersInIsolatedControl = 1;

        // The number of fingers in multi-finger simultaneous
        private int _activeFingersInMultiFingerControl = 2;


        private int _numOfLevelsInGame;

        LevelData _levelData;


        // ------------ Constants ------------ 

        private const int MaxHeight = 5;
        private const int MvcDivisor = 4;

        private const int MaxGraceValue = 4;
        private const int MinGraceValue = 1;


        private void Awake() {
            // set default finger method
            _fingerMethod = FingerMethod.IndividualControl;
        }

        // ------------ Event Handlers ------------
        private void OnEnable() {
            BridgeEvents.PrepareBridgeConfigs += UpdateDifficulty;
            BridgeEvents.EnableIsolatedControl += OnIsolatedControl;
            BridgeEvents.EnableMultiFingerControl += OnMultiFingerSimultaneous;
        }

        private void OnMultiFingerSimultaneous(bool isMultiFingerSimultaneous) {
            _allowMultiFingerSimultaneous = isMultiFingerSimultaneous;
        }

        private void OnIsolatedControl(bool isIsolatedControl) {
            _allowIndividuation = isIsolatedControl;
        }

        private void OnDisable() {
            BridgeEvents.PrepareBridgeConfigs -= UpdateDifficulty;
            BridgeEvents.EnableIsolatedControl -= OnIsolatedControl;
            BridgeEvents.EnableMultiFingerControl -= OnMultiFingerSimultaneous;
        }

        private void Start() {
            _levelData = new LevelData();
            _levelData.Level = BridgeDataManager.Level;
            _numOfLevelsInGame = BridgeDataManager.NumberOfLevels;
            _activeFingers = new HashSet<int>();
            SetMvcHeights();

            if (_allowMultiFingerSimultaneous && !_allowIndividuation) {
                _multiFingerControlStartLevel = _isolatedControlStartLevel;
            } // If the multi-finger simultaneous is allowed, but Isolated-Control is not, then the multi-finger simultaneous start level should be the same as the Isolated-Control start level.
            else if (_multiFingerControlStartLevel < _isolatedControlStartLevel) {
                _multiFingerControlStartLevel = _isolatedControlStartLevel + 1;
            } // If the multi-finger simultaneous start level is less than the Isolated-Control start level, then set the multi-finger simultaneous start level to the Isolated-Control start level + 1.
        }

        // The Force getting from the AMADEO should be top 20n for healthy adults,
        // so we should convert it to the max height possible in our game which is 5.
        private void SetMvcHeights() {
            var mvc = BridgeDataManager.IsFlexion
                ? BridgeDataManager.MvcValuesFlexion
                : BridgeDataManager.MvcValuesExtension;
            // for (int i = 0; i < _mvcToHeights.Length; i++) {
            //     _mvcToHeights[i] = Mathf.Min((int)mvc[i] / MvcDivisor, MaxHeight);
            // }

            for (int i = 0; i < Mathf.Min(_mvcToHeights.Length, mvc.Length); i++) {
                _mvcToHeights[i] = Mathf.Min((int)mvc[i] / MvcDivisor, MaxHeight);
            }

            if (_mvcToHeights.Length != mvc.Length) {
                Debug.LogWarning("SetMvcHeights: Length mismatch between _mvcToHeights and mvc arrays.");
            }


            if (_debug) {
                Debug.Log("DynamicDifficulty :: SetMVCHeights");
                Debug.Log("Trial Count: " + _levelData.TrialCount + " Level: " + _levelData.Level);
                Debug.Log("MVC to Heights: " + string.Join(",", _mvcToHeights));
            }
        }

        // ------------ Methods ------------

        // Should be called at the start of each session
        private void InitSession() {
            _fingerMethod = FingerMethod.IndividualControl;
            // Get the indexes of the active fingers
            _allowedFingers = new HashSet<int>();
            for (int i = 0; i < BridgeDataManager.PlayableUnits.Length; i++) {
                if (BridgeDataManager.PlayableUnits[i]) {
                    _allowedFingers.Add(i);
                }
            }

            if (_debug) {
                Debug.Log("Active Fingers: " + string.Join(",", _allowedFingers));
            }

            // check if allowed fingers are empty
            if (_allowedFingers.Count == 0) {
                Debug.LogError("No active fingers found");
                return;
            }

            // Get the indexes of the non-active fingers
            _nonActiveFingers = BridgeDataManager.PlayableUnits
                .Select((value, index) => new { value, index })
                .Where(pair => !pair.value)
                .Select(pair => pair.index)
                .ToHashSet();

            // Set Starting Static and MultiFingerControl Fingers
            _staticFingersInIsolatedControl = _startingAmountOfStaticFingers;
            _activeFingersInMultiFingerControl = _startingAmountOfMultiFingerControl;
        }

        // private void UpdateDifficulty() {
        //     Debug.Log("UpdateDifficulty: Start");
        //     if (_firstTrial) {
        //         InitSession();
        //         _activeFingers.Add(_allowedFingers.First());
        //         _firstTrial = false;
        //         var initialHeights = BridgeDataManager.Heights;
        //         // initialHeights[_activeFingers.First()] = 1;
        //         if (_activeFingers.Count > 0) {
        //             initialHeights[_activeFingers.First()] = 1;
        //             BridgeDataManager.SetHeights(initialHeights);
        //             BridgeDataManager.SetPlayableUnits(new bool[5]);
        //             BridgeDataManager.PlayableUnits[_activeFingers.First()] = true;
        //         }
        //         else {
        //             Debug.LogWarning("UpdateDifficulty: _activeFingers is empty.");
        //         }
        //
        //         return;
        //     }
        //
        //     SessionData sessionData = BridgeDataManager.SessionData;
        //     if (_debug) {
        //         Debug.Log("SessionData: Success ? " + sessionData.success);
        //     }
        //
        //     if (_levelData.TrialCount >= _trialsPerLevel) {
        //         _levelData.TrialCount = 0;
        //         _levelData.NumOfSuccessfulTrials = 0;
        //         _levelData.NumOfFailedTrials = 0;
        //         _levelData.Level++;
        //         if (_levelData.Level >= _numOfLevelsInGame) {
        //             _firstTrial = true;
        //             BridgeEvents.GameFinishedAction?.Invoke();
        //             return;
        //         }
        //
        //         BridgeDataManager.SetLevel(_levelData.Level);
        //         if (_levelData.Level >= _multiFingerControlStartLevel && _allowMultiFingerSimultaneous) {
        //             _fingerMethod = FingerMethod.SimultaneousMultiFingerControl;
        //         }
        //         else if (_levelData.Level >= _isolatedControlStartLevel && _allowIndividuation) {
        //             _fingerMethod = FingerMethod.IsolatedControl;
        //         }
        //     }
        //     else {
        //         _levelData.TrialCount++;
        //     }
        //
        //     if (sessionData.success) {
        //         _levelData.NumOfSuccessfulTrials++;
        //     }
        //     else {
        //         _levelData.NumOfFailedTrials++;
        //     }
        //
        //     var currentLevel = _levelData.Level;
        //     // Adjust finger method difficulty based on the session data
        //     if (sessionData.success) {
        //         // If the difficulty is has lowered from the previous trial, Increase back the difficulty based on the current level
        //         if (currentLevel >= _isolatedControlStartLevel && _fingerMethod == FingerMethod.IndividualControl) {
        //             _fingerMethod = FingerMethod.IsolatedControl;
        //         }
        //         else if (currentLevel >= _multiFingerControlStartLevel &&
        //                  _fingerMethod == FingerMethod.IsolatedControl) {
        //             _fingerMethod = FingerMethod.SimultaneousMultiFingerControl;
        //         }
        //     }
        //     else if (_fingerMethod is not FingerMethod.IndividualControl) {
        //         var lowerDifficulty = _levelData.NumOfFailedTrials >= _failuresBeforeModeReduction;
        //         if (lowerDifficulty) {
        //             if (_fingerMethod is FingerMethod.SimultaneousMultiFingerControl) {
        //                 _fingerMethod = FingerMethod.IsolatedControl;
        //             }
        //             else if (_fingerMethod is FingerMethod.IsolatedControl) {
        //                 _fingerMethod = FingerMethod.IndividualControl;
        //             }
        //         }
        //     }
        //
        //
        //     var lastPlayedFingers = _activeFingers;
        //     if (lastPlayedFingers.Count == 0) {
        //         return;
        //     }
        //
        //     int[] updatedHeights = BridgeDataManager.HeightsAbsolute;
        //
        //     // Adjust heights,on the last played fingers
        //     // iterate over the last played fingers, if the session was successful, increase the height of the finger, else decrease the height of the finger
        //     // For each change in the height, we should check that the height is higher that 0 and less than the MVC/4 of the finger
        //
        //     if (_debug) {
        //         Debug.Log("Last Played Fingers: " + string.Join(",", lastPlayedFingers));
        //     }
        //
        //     foreach (var finger in lastPlayedFingers) {
        //         if (finger < updatedHeights.Length) {
        //             if (sessionData.success) {
        //                 updatedHeights[finger] = Mathf.Min(_mvcToHeights[finger], updatedHeights[finger] + 1);
        //             } else {
        //                 updatedHeights[finger] = Mathf.Max(1, updatedHeights[finger] - 1);
        //             }
        //         } else {
        //             Debug.LogError($"UpdateDifficulty: Finger index {finger} is out of bounds in updatedHeights.");
        //         }
        //     }
        //
        //
        //     if (_allowedFingers.Count >= 1) {
        //         // In this part, we will adjust the next playable fingers based on the finger method
        //         switch (_fingerMethod) {
        //             case FingerMethod.IndividualControl:
        //                 Debug.Log("UpdateDifficulty: Individual Control");
        //                 // if session was successful, increase the height of the active finger
        //
        //                 // Randomly select a finger from the allowed fingers
        //                 Debug.Log("allowedFingers: " + string.Join(",", _allowedFingers));
        //                 var allowedFingersList = _allowedFingers.ToList();
        //                 var randomFinger = allowedFingersList[Random.Range(0, allowedFingersList.Count)];
        //
        //
        //                 _activeFingers.Clear();
        //                 _activeFingers.Add(randomFinger);
        //                 // Make sure the current height for this finger is between the range of 1 and the MVC/4 of the finger
        //                 // If so, leave it as is, else set it to 1 or the MVC/4 of the finger
        //                 if (updatedHeights[randomFinger] == 0) {
        //                     updatedHeights[randomFinger] = 1;
        //                 }
        //                 else if (updatedHeights[randomFinger] > _mvcToHeights[randomFinger]) {
        //                     updatedHeights[randomFinger] = _mvcToHeights[randomFinger];
        //                 }
        //
        //
        //                 break;
        //             case FingerMethod.IsolatedControl:
        //                 Debug.Log("UpdateDifficulty: Isolated-Control");
        //
        //                 // in Isolated-Control, we want to make the same thing in Individual Control,
        //                 // we choose a random finger from the allowed fingers and activate it with a height in the valid range 
        //                 // but we want to make sure also to activate the other allowed fingers with a height of 0
        //
        //                 // Randomly select a finger from the allowed fingers
        //                 var randomFingerIsolatedControl =
        //                     _allowedFingers.ElementAt(UnityEngine.Random.Range(0, _allowedFingers.Count));
        //
        //                 // Make sure the current height for this finger is between the range of 1 and the MVC/4 of the finger
        //                 // If so, leave it as is, else set it to 1 or the MVC/4 of the finger
        //                 if (updatedHeights[randomFingerIsolatedControl] == 0) {
        //                     updatedHeights[randomFingerIsolatedControl] = 1;
        //                 }
        //                 else if (updatedHeights[randomFingerIsolatedControl] >
        //                          _mvcToHeights[randomFingerIsolatedControl]) {
        //                     updatedHeights[randomFingerIsolatedControl] = _mvcToHeights[randomFingerIsolatedControl];
        //                 }
        //
        //                 // if the last session was a success,
        //                 // increase the number of static fingers in Isolated-Control up to the allowed fingers count - 1
        //                 if (sessionData.success) {
        //                     _staticFingersInIsolatedControl = Mathf.Min(_allowedFingers.Count - 1,
        //                         _staticFingersInIsolatedControl + 1);
        //                 }
        //                 else {
        //                     _staticFingersInIsolatedControl = Mathf.Max(1, _staticFingersInIsolatedControl - 1);
        //                 }
        //
        //                 // Activate the other allowed fingers with a height of 0
        //                 // choose _staticFingersInIsolatedControl arbitrarily num fingers from _allowedFingers that is not the randomFingerIsolated-Control, and apply their height to be 0
        //                 // it may be that the _staticFingersInIsolatedControl is less than the number of allowed fingers, so we need to choose randomly from the allowed fingers
        //                 var nonActiveFingersIsolatedMode = _allowedFingers.ToList();
        //                 nonActiveFingersIsolatedMode.Remove(randomFingerIsolatedControl);
        //                 if (_debug) {
        //                     Debug.Log("Non Active Fingers: " + string.Join(",", nonActiveFingersIsolatedMode) +
        //                               " Chosen Random Finger: " + randomFingerIsolatedControl +
        //                               " Num of Static Fingers: " + _staticFingersInIsolatedControl +
        //                               " Allowed Fingers: " + string.Join(",", _allowedFingers) + " Active Fingers: " +
        //                               string.Join(",", _activeFingers) + " Updated Heights: " +
        //                               string.Join(",", updatedHeights));
        //                 }
        //
        //                 for (int i = _allowedFingers.Count;
        //                      i > _allowedFingers.Count - _staticFingersInIsolatedControl + 1;
        //                      i--) {
        //                     var randomStaticFinger =
        //                         nonActiveFingersIsolatedMode[Random.Range(0, nonActiveFingersIsolatedMode.Count)];
        //
        //                     nonActiveFingersIsolatedMode.Remove(randomStaticFinger);
        //                 }
        //
        //                 foreach (int i in nonActiveFingersIsolatedMode) {
        //                     updatedHeights[i] = 0;
        //                 }
        //
        //
        //                 if (_debug) {
        //                     Debug.Log("Non Active Fingers: " + string.Join(",", nonActiveFingersIsolatedMode));
        //                 }
        //
        //                 _activeFingers.Clear();
        //
        //                 if (_debug) {
        //                     Debug.Log("Non Active Fingers: " + string.Join(",", nonActiveFingersIsolatedMode));
        //                 }
        //
        //                 // Set _activeFingers to the random finger and the rest of the chosen static fingers
        //                 _activeFingers.Add(randomFingerIsolatedControl);
        //                 _activeFingers.UnionWith(nonActiveFingersIsolatedMode);
        //                 if (_debug) {
        //                     Debug.Log("Active Fingers: " + string.Join(",", _activeFingers));
        //                 }
        //
        //                 break;
        //
        //
        //             case FingerMethod.SimultaneousMultiFingerControl:
        //                 Debug.Log("UpdateDifficulty: MultiFingerSimultaneous");
        //                 // in multi-finger simultaneous, we want to make the same thing in Isolated-Control,
        //                 // except that we want to the _activeFingersInMultiFingerControl from the allowed fingers with a height in the valid range
        //
        //                 // Randomly select _activeFingersInMultiFingerControl fingers from the allowed fingers
        //
        //                 // if the last session was a success,
        //                 // increase the number of fingers in multi-finger simultaneous up to the allowed fingers count
        //                 if (sessionData.success) {
        //                     _activeFingersInMultiFingerControl = Mathf.Min(_allowedFingers.Count,
        //                         _activeFingersInMultiFingerControl + 1);
        //                 }
        //                 else {
        //                     _activeFingersInMultiFingerControl =
        //                         Mathf.Max(2, _activeFingersInMultiFingerControl - 1);
        //                 }
        //
        //                 var randomFingersMultiFingerSimultaneous = _allowedFingers.ToList();
        //                 for (int i = _allowedFingers.Count;
        //                      i >= _allowedFingers.Count - _activeFingersInMultiFingerControl;
        //                      i++) {
        //                     var randomFingerMultiFingerSimultaneous =
        //                         randomFingersMultiFingerSimultaneous[
        //                             Random.Range(0, randomFingersMultiFingerSimultaneous.Count)];
        //
        //                     // Make sure the current height for this finger is between the range of 1 and the MVC/4 of the finger
        //                     // If so, leave it as is, else set it to 1 or the MVC/4 of the finger
        //                     if (updatedHeights[randomFingerMultiFingerSimultaneous] == 0) {
        //                         updatedHeights[randomFingerMultiFingerSimultaneous] = 1;
        //                     }
        //                     else if (updatedHeights[randomFingerMultiFingerSimultaneous] >
        //                              _mvcToHeights[randomFingerMultiFingerSimultaneous]) {
        //                         updatedHeights[randomFingerMultiFingerSimultaneous] =
        //                             _mvcToHeights[randomFingerMultiFingerSimultaneous];
        //                     }
        //
        //                     randomFingersMultiFingerSimultaneous.Remove(randomFingerMultiFingerSimultaneous);
        //                 }
        //
        //                 _activeFingers.Clear();
        //
        //                 // Set _activeFingers to the random fingers
        //                 _activeFingers.UnionWith(randomFingersMultiFingerSimultaneous);
        //
        //
        //                 break;
        //             default:
        //                 throw new ArgumentOutOfRangeException();
        //         }
        //     }
        //
        //     // shuffle all the heights values that are not an active finger index, using the non-active fingers
        //     var nonActiveFingers = _nonActiveFingers.ToList();
        //     foreach (int finger in nonActiveFingers) {
        //         updatedHeights[finger] = Random.Range(1, _mvcToHeights[finger]);
        //     }
        //
        //     // Set the updated heights
        //     BridgeDataManager.SetHeights(updatedHeights);
        //
        //
        //     // Set the playable units
        //     bool[] playableUnits = new bool[5];
        //     for (int i = 0; i < BridgeDataManager.PlayableUnits.Length; i++) {
        //         playableUnits[i] = _activeFingers.Contains(i);
        //     }
        //
        //     BridgeDataManager.SetPlayableUnits(playableUnits);
        //
        //     // Update Grace Values
        //     Dictionary<int, float> bestYPositions = sessionData.BestYPositions
        //         .Select((value, index) => new { value, index })
        //         .ToDictionary(pair => pair.index, pair => pair.value);
        //
        //     // For each outcome, If the session was successful, increase the grace value of the lastPlayedFingers, else decrease the grace value of the lastPlayedFingers
        //     // For each change in the grace value, we should check that the grace value is between 1 and 4
        //     // Increase the value of a finger grace only the session was failure and if the differ between the HighestYPos and the sessionData.heights is less than 1,
        //     // If the session was successful, decrease the grace value of the lastPlayedFingers
        //
        //     float[] updatedGraceValues = BridgeDataManager.UnitsGrace;
        //     foreach (var finger in lastPlayedFingers) {
        //         if (sessionData.success) {
        //             updatedGraceValues[finger] = Mathf.Max(MinGraceValue, updatedGraceValues[finger] - 1);
        //         }
        //         else {
        //             if (Mathf.Abs(bestYPositions[finger] - updatedHeights[finger]) < 1) {
        //                 updatedGraceValues[finger] = Mathf.Min(MaxGraceValue, updatedGraceValues[finger] + 1);
        //             }
        //         }
        //     }
        //
        //     BridgeDataManager.SetUnitsGrace(updatedGraceValues);
        // }

        private void UpdateDifficulty() {
            Debug.Log("UpdateDifficulty: Start");

            if (_firstTrial) {
                InitSession();
                _activeFingers.Add(_allowedFingers.First());
                _firstTrial = false;
                var initialHeights = BridgeDataManager.Heights;
                if (_activeFingers.Count > 0) {
                    initialHeights[_activeFingers.First()] = 1;
                    BridgeDataManager.SetHeights(initialHeights);
                    BridgeDataManager.SetPlayableUnits(new bool[5]);
                    BridgeDataManager.PlayableUnits[_activeFingers.First()] = true;
                }
                else {
                    Debug.LogWarning("UpdateDifficulty: _activeFingers is empty.");
                }

                return;
            }

            SessionData sessionData = BridgeDataManager.SessionData;
            if (_debug) {
                Debug.Log("SessionData: Success ? " + sessionData.success);
            }

            if (sessionData.success) {
                _levelData.NumOfSuccessfulTrials++;
            }
            else {
                _levelData.NumOfFailedTrials++;
            }

            if (_levelData.TrialCount >= _trialsPerLevel) {
                bool isGameFinished = AdvanceToNextLevel();
                if (isGameFinished) {
                    BridgeEvents.GameFinishedAction?.Invoke();
                    return;
                }
            }
            else {
                _levelData.TrialCount++;
            }


            AdjustFingerMethod(sessionData);

            if (_activeFingers.Count == 0) return;

            int[] updatedHeights = BridgeDataManager.HeightsAbsolute;
            AdjustHeightsForFingers(sessionData, _activeFingers, updatedHeights);
            UpdateNextPlayableFingers(sessionData, updatedHeights);

            // shuffle all the heights values that are not an active finger index, using the non-active fingers
            ShuffleNonActiveFingers(updatedHeights);

            // Set the updated heights
            BridgeDataManager.SetHeights(updatedHeights);

            // Set the playable units
            bool[] playableUnits = new bool[5];
            for (int i = 0; i < BridgeDataManager.PlayableUnits.Length; i++) {
                playableUnits[i] = _activeFingers.Contains(i);
            }

            BridgeDataManager.SetPlayableUnits(playableUnits);

            UpdateGraceValues(sessionData, _activeFingers, updatedHeights);
        }


        // ------------ Helper Methods ------------        
        private int GetRandomAllowedFinger() {
            var allowedFingersList = _allowedFingers.ToList();
            return allowedFingersList[Random.Range(0, allowedFingersList.Count)];
        }

        private bool AdvanceToNextLevel() {
            _levelData.TrialCount = 0;
            _levelData.NumOfSuccessfulTrials = 0;
            _levelData.NumOfFailedTrials = 0;
            _levelData.Level++;

            if (_levelData.Level >= _numOfLevelsInGame) {
                _firstTrial = true;
                return true;
            }

            BridgeDataManager.SetLevel(_levelData.Level);

            if (_levelData.Level >= _multiFingerControlStartLevel && _allowMultiFingerSimultaneous) {
                _fingerMethod = FingerMethod.SimultaneousMultiFingerControl;
            }
            else if (_levelData.Level >= _isolatedControlStartLevel && _allowIndividuation) {
                _fingerMethod = FingerMethod.IsolatedControl;
            }

            return false;
        }

        private void ShuffleNonActiveFingers(int[] updatedHeights) {
            var nonActiveFingers = _nonActiveFingers.ToList();
            foreach (int finger in nonActiveFingers) {
                updatedHeights[finger] = Random.Range(0, _mvcToHeights[finger]);
            }
        }

        private void AdjustFingerMethod(SessionData sessionData) {
            var currentLevel = _levelData.Level;

            if (sessionData.success) {
                // Increase difficulty back if it was lowered
                if (currentLevel >= _isolatedControlStartLevel && _fingerMethod == FingerMethod.IndividualControl) {
                    _fingerMethod = FingerMethod.IsolatedControl;
                }
                else if (currentLevel >= _multiFingerControlStartLevel &&
                         _fingerMethod == FingerMethod.IsolatedControl) {
                    _fingerMethod = FingerMethod.SimultaneousMultiFingerControl;
                }
            }
            else {
                var lowerDifficulty = _levelData.NumOfFailedTrials >= _failuresBeforeModeReduction;

                if (lowerDifficulty) {
                    _levelData.NumOfFailedTrials = 0;
                    if (_fingerMethod == FingerMethod.SimultaneousMultiFingerControl) {
                        _fingerMethod = FingerMethod.IsolatedControl;
                    }
                    else if (_fingerMethod == FingerMethod.IsolatedControl) {
                        _fingerMethod = FingerMethod.IndividualControl;
                    }
                }
            }
        }

        private void UpdateNextPlayableFingers(SessionData sessionData, int[] updatedHeights) {
            _activeFingers.Clear();

            switch (_fingerMethod) {
                case FingerMethod.IndividualControl:
                    var randomFinger = GetRandomAllowedFinger();
                    _activeFingers.Add(randomFinger);
                    SetFingerHeightInRange(randomFinger, updatedHeights);
                    break;

                case FingerMethod.IsolatedControl:
                    var randomFingerIsolatedControl = GetRandomAllowedFinger();
                    SetFingerHeightInRange(randomFingerIsolatedControl, updatedHeights);
                    UpdateStaticFingersForIsolatedControl(randomFingerIsolatedControl, updatedHeights, sessionData);
                    break;

                case FingerMethod.SimultaneousMultiFingerControl:
                    UpdateMultiFingerControl(updatedHeights, sessionData);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetFingerHeightInRange(int finger, int[] updatedHeights) {
            if (updatedHeights[finger] == 0) {
                updatedHeights[finger] = 1;
            }
            else if (updatedHeights[finger] > _mvcToHeights[finger]) {
                updatedHeights[finger] = _mvcToHeights[finger];
            }
        }

        private void UpdateStaticFingersForIsolatedControl(int activeFinger, int[] updatedHeights,
            SessionData sessionData) {
            if (_allowedFingers.Count <= 1) return;
            if (sessionData.success) {
                bool winStreak = _levelData.NumOfSuccessfulTrials >= _successesBeforeHeightIncrease;
                if (winStreak) {
                    _levelData.NumOfSuccessfulTrials = 0;
                    _staticFingersInIsolatedControl =
                        Mathf.Min(_allowedFingers.Count - 1, _staticFingersInIsolatedControl + 1);
                }
            }
            else {
                _staticFingersInIsolatedControl = Mathf.Max(1, _staticFingersInIsolatedControl - 1);
            }


            var nonActiveFingers = _allowedFingers.ToList();
            nonActiveFingers.Remove(activeFinger);

            for (int i = 0; i < _staticFingersInIsolatedControl; i++) {
                var staticFinger = nonActiveFingers[Random.Range(0, nonActiveFingers.Count)];
                nonActiveFingers.Remove(staticFinger);
                updatedHeights[staticFinger] = 0;
                _activeFingers.Add(staticFinger);
            }

            // Add the isolated active finger
            _activeFingers.Add(activeFinger);
        }

        private void UpdateMultiFingerControl(int[] updatedHeights, SessionData sessionData) {
            if (_allowedFingers.Count <= 1) return;
            if (sessionData.success) {
                bool winStreak = _levelData.NumOfSuccessfulTrials >= _successesBeforeHeightIncrease;
                if (winStreak) {
                    _levelData.NumOfSuccessfulTrials = 0;
                    _activeFingersInMultiFingerControl =
                        Mathf.Min(_allowedFingers.Count, _activeFingersInMultiFingerControl + 1);
                }
            }
            else {
                _activeFingersInMultiFingerControl = Mathf.Max(2, _activeFingersInMultiFingerControl - 1);
            }

            var selectedFingers = _allowedFingers.ToList();
            for (int i = 0; i < _activeFingersInMultiFingerControl; i++) {
                var selectedFinger = selectedFingers[Random.Range(0, selectedFingers.Count)];
                SetFingerHeightInRange(selectedFinger, updatedHeights);
                _activeFingers.Add(selectedFinger);
                selectedFingers.Remove(selectedFinger);
            }
        }


        private void AdjustHeightsForFingers(SessionData sessionData, IEnumerable<int> fingers, int[] updatedHeights) {
            // Adjust heights,on the last played fingers
            // iterate over the last played fingers, if the session was successful, increase the height of the finger, else decrease the height of the finger
            // For each change in the height, we should check that the height is higher that 0 and less than the MVC/4 of the finger
            foreach (var finger in fingers) {
                if (finger < updatedHeights.Length) {
                    if (sessionData.success) {
                        updatedHeights[finger] = Mathf.Min(_mvcToHeights[finger], updatedHeights[finger] + 1);
                    }
                    else {
                        updatedHeights[finger] = Mathf.Max(1, updatedHeights[finger] - 1);
                    }
                }
                else {
                    Debug.LogError($"UpdateDifficulty: Finger index {finger} is out of bounds in updatedHeights.");
                }
            }
        }

        private void UpdateGraceValues(SessionData sessionData, IEnumerable<int> fingers, int[] updatedHeights) {
            // For each outcome, If the session was successful, increase the grace value of the lastPlayedFingers, else decrease the grace value of the lastPlayedFingers
            // For each change in the grace value, we should check that the grace value is between 1 and 4
            // Increase the value of a finger grace only the session was failure and if the differ between the HighestYPos and the sessionData.heights is less than 1,
            // If the session was successful, decrease the grace value of the lastPlayedFingers

            float[] updatedGraceValues = BridgeDataManager.UnitsGrace;
            Dictionary<int, float> bestYPositions = sessionData.BestYPositions
                .Select((value, index) => new { value, index })
                .ToDictionary(pair => pair.index, pair => pair.value);

            foreach (var finger in fingers) {
                if (sessionData.success) {
                    updatedGraceValues[finger] = Mathf.Max(MinGraceValue, updatedGraceValues[finger] - 1);
                }
                else {
                    if (Mathf.Abs(bestYPositions[finger] - updatedHeights[finger]) < 1) {
                        updatedGraceValues[finger] = Mathf.Min(MaxGraceValue, updatedGraceValues[finger] + 1);
                    }
                }
            }

            BridgeDataManager.SetUnitsGrace(updatedGraceValues);
        }
    }

    // This struct will contain all the relevant data for the current level, such as the number of failed trials.
    public struct LevelData {
        public int Level;
        public int TrialCount;

        public int NumOfSuccessfulTrials;

        // public HashSet<int> LastPlayedFingers;
        public int NumOfFailedTrials;
    }
}