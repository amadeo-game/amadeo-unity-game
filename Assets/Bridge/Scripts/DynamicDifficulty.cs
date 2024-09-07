using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BridgePackage {
    internal enum ControlMethod {
        IndividualControl,
        IsolatedControl,
        SimultaneousMultiFingerControl
    }

    public class DynamicDifficulty : MonoBehaviour {
        [SerializeField] private bool _debug;

        // The current finger method, easiest is Individual Control, then Isolated-Control, then multi-finger Simultaneous-MultiFinger-Control
        private ControlMethod _controlMethod;
        private bool _allowIsolatedControl = false;
        private bool _allowMultiFingerSimultaneous = false;

        private HashSet<int> _allowedFingers;
        private HashSet<int> _nonActiveFingers;

        private HashSet<int> _activeFingers;


        private readonly int[] _mvcToHeights = new int[5];

        internal static bool _firstTrial = true;

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
            _controlMethod = ControlMethod.IndividualControl;
        }

        // ------------ Event Handlers ------------
        private void OnEnable() {
            GameConfigEvents.PrepareBridgeConfigs += UpdateDifficulty;
            GameConfigEvents.EnableIsolatedControl += OnIsolatedControl;
            GameConfigEvents.EnableMultiFingerControl += OnMultiFingerSimultaneous;
        }

        private void OnMultiFingerSimultaneous(bool isMultiFingerSimultaneous) {
            _allowMultiFingerSimultaneous = isMultiFingerSimultaneous;
        }

        private void OnIsolatedControl(bool isIsolatedControl) {
            _allowIsolatedControl = isIsolatedControl;
        }

        private void OnDisable() {
            GameConfigEvents.PrepareBridgeConfigs -= UpdateDifficulty;
            GameConfigEvents.EnableIsolatedControl -= OnIsolatedControl;
            GameConfigEvents.EnableMultiFingerControl -= OnMultiFingerSimultaneous;
        }

        private void Start() {
            _levelData = new LevelData();
            _levelData.Level = BridgeDataManager.Level;
            _numOfLevelsInGame = BridgeDataManager.NumberOfLevels;
            _activeFingers = new HashSet<int>();
            SetMvcHeights();

            if (_allowMultiFingerSimultaneous && !_allowIsolatedControl) {
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
            _controlMethod = ControlMethod.IndividualControl;
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

        private void UpdateDifficulty() {
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
                    GameActions.GameFinishedAction?.Invoke();
                    return;
                }
            }
            else {
                _levelData.TrialCount++;
            }


            AdjustFingerMethod(sessionData);

            if (_activeFingers.Count == 0) return;

            int[] updatedHeights = BridgeDataManager.HeightsAbsolute;
            UpdateGraceValues(sessionData, _activeFingers, updatedHeights);

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
                return true;
            }

            BridgeDataManager.SetLevel(_levelData.Level);

            if (_levelData.Level >= _multiFingerControlStartLevel && _allowMultiFingerSimultaneous) {
                _controlMethod = ControlMethod.SimultaneousMultiFingerControl;
            }
            else if (_levelData.Level >= _isolatedControlStartLevel && _allowIsolatedControl) {
                _controlMethod = ControlMethod.IsolatedControl;
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
                if (_allowIsolatedControl && currentLevel >= _isolatedControlStartLevel &&
                    _controlMethod == ControlMethod.IndividualControl) {
                    _controlMethod = ControlMethod.IsolatedControl;
                }
                else if (currentLevel >= _multiFingerControlStartLevel &&
                         _controlMethod == (_allowIsolatedControl
                             ? ControlMethod.IsolatedControl
                             : ControlMethod.IndividualControl)) {
                    _controlMethod = ControlMethod.SimultaneousMultiFingerControl;
                }
            }
            else {
                var lowerDifficulty = _levelData.NumOfFailedTrials >= _failuresBeforeModeReduction;

                if (lowerDifficulty) {
                    _levelData.NumOfFailedTrials = 0;
                    if (_controlMethod == ControlMethod.SimultaneousMultiFingerControl) {
                        _controlMethod = (_allowIsolatedControl
                            ? ControlMethod.IsolatedControl
                            : ControlMethod.IndividualControl);
                    }
                    else if (_controlMethod == ControlMethod.IsolatedControl) {
                        _controlMethod = ControlMethod.IndividualControl;
                    }
                }
            }
        }

        private void UpdateNextPlayableFingers(SessionData sessionData, int[] updatedHeights) {
            _activeFingers.Clear();

            switch (_controlMethod) {
                case ControlMethod.IndividualControl:
                    var randomFinger = GetRandomAllowedFinger();
                    _activeFingers.Add(randomFinger);
                    SetFingerHeightInRange(randomFinger, updatedHeights);
                    break;

                case ControlMethod.IsolatedControl:
                    var randomFingerIsolatedControl = GetRandomAllowedFinger();
                    SetFingerHeightInRange(randomFingerIsolatedControl, updatedHeights);
                    UpdateStaticFingersForIsolatedControl(randomFingerIsolatedControl, updatedHeights, sessionData);
                    break;

                case ControlMethod.SimultaneousMultiFingerControl:
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
                    Debug.Log("Best Y Position: " + bestYPositions[finger] + " Updated Height: " + updatedHeights[finger] +
                              " Difference: " + Mathf.Abs(bestYPositions[finger] - updatedHeights[finger]) +
                              " Grace Value: " + updatedGraceValues[finger]);
                    if (Mathf.Abs(bestYPositions[finger] - updatedHeights[finger]) <= 1) {
                        updatedGraceValues[finger] = Mathf.Min(MaxGraceValue, updatedGraceValues[finger] + 1);
                        Debug.Log("Grace Value Increased, finger: " + finger + " Grace Value: " +
                                  updatedGraceValues[finger]);
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