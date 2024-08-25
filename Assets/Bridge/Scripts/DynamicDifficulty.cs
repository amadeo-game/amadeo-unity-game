using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace BridgePackage {
    internal enum FingerMethod {
        SingleFinger,
        Individuation,
        MultiFingerSimultaneous
    }

    public class DynamicDifficulty : MonoBehaviour {
        [SerializeField] private bool _debug;

        // The current finger method, easiest is single finger, then individuation, then multi-finger simultaneous
        private FingerMethod _fingerMethod;
        private bool _allowIndividuation;
        private bool _allowMultiFingerSimultaneous;

        private HashSet<int> _allowedFingers;
        private HashSet<int> _nonActiveFingers;

        private HashSet<int> _activeFingers;


        private int[] _mvcToHeights = new int[5];

        private bool _firstTrial = true;

        // Set to when the individuation starts
        [SerializeField] private int _individuationStartLevel = 2;

        // Should be greater than the individuation start level, because it is usually harder
        [SerializeField] private int _multiFingerSimultaneousStartLevel = 5;

        // When this number of failed trials is reached, the finger method is lowered.
        [SerializeField] private int _numOfFailedTrialsBeforeLoweringFingerMethod = 2;

        // When this number of successful trials is reached, the height difficulty is increased.
        [SerializeField] private int _numOfSuccessfulTrialsBeforeIncreasingHeightDifficulty = 2;

        [SerializeField] private int _numOfTrialsInLevel = 5;

        private int _numOfStaticFingersIndividuation = 1;

        private int _numOfLevelsInGame;

        LevelData _levelData;

        private void Awake() {
            // set default finger method
            _fingerMethod = FingerMethod.SingleFinger;
        }

        // ------------ Event Handlers ------------
        private void OnEnable() {
            BridgeEvents.PrepareBridgeConfigs += UpdateDifficulty;
            BridgeEvents.EnableIndividuation += OnIndividuation;
            BridgeEvents.EnableMultiFingerSimultaneous += OnMultiFingerSimultaneous;
        }

        private void OnMultiFingerSimultaneous(bool isMultiFingerSimultaneous) {
            _allowMultiFingerSimultaneous = isMultiFingerSimultaneous;
        }

        private void OnIndividuation(bool isIndividuation) {
            _allowIndividuation = isIndividuation;
        }

        private void OnDisable() {
            BridgeEvents.PrepareBridgeConfigs -= UpdateDifficulty;
            BridgeEvents.EnableIndividuation -= OnIndividuation;
            BridgeEvents.EnableMultiFingerSimultaneous -= OnMultiFingerSimultaneous;
        }

        private void Start() {
            _levelData = new LevelData();
            _levelData.Level = BridgeDataManager.Level;
            _numOfLevelsInGame = BridgeDataManager.NumberOfLevels;
            _activeFingers = new HashSet<int>();
            SetMvcHeights();

            if (_allowMultiFingerSimultaneous && !_allowIndividuation) {
                _multiFingerSimultaneousStartLevel = _individuationStartLevel;
            } // If the multi-finger simultaneous is allowed, but individuation is not, then the multi-finger simultaneous start level should be the same as the individuation start level.
            else if (_multiFingerSimultaneousStartLevel < _individuationStartLevel) {
                _multiFingerSimultaneousStartLevel = _individuationStartLevel + 1;
            } // If the multi-finger simultaneous start level is less than the individuation start level, then set the multi-finger simultaneous start level to the individuation start level + 1.
        }

        // The Force getting from the AMADEO should be top 20n for healthy adults,
        // so we should convert it to the max height possible in our game which is 5.
        private void SetMvcHeights() {
            var mvc = BridgeDataManager.IsFlexion
                ? BridgeDataManager.MvcValuesFlexion
                : BridgeDataManager.MvcValuesExtension;
            for (int i = 0; i < _mvcToHeights.Length; i++) {
                _mvcToHeights[i] = Mathf.Min((int)mvc[i] / 4, 5);
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
            _fingerMethod = FingerMethod.SingleFinger;
            // Get the indexes of the active fingers
            _allowedFingers = BridgeDataManager.PlayableUnits.Select((value, index) => new { value, index })
                .Where(pair => pair.value)
                .Select(pair => pair.index)
                .ToHashSet();

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
        }

        private void UpdateDifficulty() {
            Debug.Log("UpdateDifficulty: Start");
            if (_firstTrial) {
                InitSession();
                _activeFingers.Add(_allowedFingers.First());
                _firstTrial = false;
                var initialHeights = BridgeDataManager.Heights;
                initialHeights[_activeFingers.First()] = 1;

                BridgeDataManager.SetHeights(initialHeights);
                BridgeDataManager.SetPlayableUnits(new bool[5]);
                BridgeDataManager.PlayableUnits[_activeFingers.First()] = true;
                // _levelData.LastPlayedFingers.Clear();
                // _levelData.LastPlayedFingers.Add(_activeFingers.First());
                return;
            }

            SessionData sessionData = BridgeDataManager.SessionData;
            if (_debug) {
                Debug.Log("SessionData: Success ? " + sessionData.success);
            }

            if (_levelData.TrialCount >= _numOfTrialsInLevel) {
                _levelData.TrialCount = 0;
                _levelData.NumOfSuccessfulTrials = 0;
                _levelData.NumOfFailedTrials = 0;
                _levelData.Level++;
                if (_levelData.Level >= _numOfLevelsInGame) {
                    _firstTrial = true;
                    BridgeEvents.GameFinishedAction?.Invoke();
                    return;
                }

                BridgeDataManager.SetLevel(_levelData.Level);
                if (_levelData.Level >= _multiFingerSimultaneousStartLevel && _allowMultiFingerSimultaneous) {
                    _fingerMethod = FingerMethod.MultiFingerSimultaneous;
                }
                else if (_levelData.Level >= _individuationStartLevel && _allowIndividuation) {
                    _fingerMethod = FingerMethod.Individuation;
                }
            }
            else {
                _levelData.TrialCount++;
            }

            if (sessionData.success) {
                _levelData.NumOfSuccessfulTrials++;
            }
            else {
                _levelData.NumOfFailedTrials++;
            }

            var currentLevel = _levelData.Level;
            // Adjust finger method difficulty based on the session data
            if (sessionData.success) {
                // If the difficulty is has lowered from the previous trial, Increase back the difficulty based on the current level
                if (currentLevel >= _individuationStartLevel && _fingerMethod == FingerMethod.SingleFinger) {
                    _fingerMethod = FingerMethod.Individuation;
                }
                else if (currentLevel >= _multiFingerSimultaneousStartLevel &&
                         _fingerMethod == FingerMethod.Individuation) {
                    _fingerMethod = FingerMethod.MultiFingerSimultaneous;
                }
            }
            else if (_fingerMethod is not FingerMethod.SingleFinger) {
                var lowerDifficulty = _levelData.NumOfFailedTrials >= _numOfFailedTrialsBeforeLoweringFingerMethod;
                if (lowerDifficulty) {
                    if (_fingerMethod is FingerMethod.MultiFingerSimultaneous) {
                        _fingerMethod = FingerMethod.Individuation;
                    }
                    else if (_fingerMethod is FingerMethod.Individuation) {
                        _fingerMethod = FingerMethod.SingleFinger;
                    }
                }
            }


            var heights = sessionData.heights;
            var lastPlayedFingers = _activeFingers;
            if (lastPlayedFingers.Count == 0) {
                return;
            }

            int[] updatedHeights = BridgeDataManager.HeightsAbsolute;

            // Adjust heights,on the last played fingers
            // iterate over the last played fingers, if the session was successful, increase the height of the finger, else decrease the height of the finger
            // For each change in the height, we should check that the height is higher that 0 and less than the MVC/4 of the finger

            if (_debug) {
                Debug.Log("Last Played Fingers: " + string.Join(",", lastPlayedFingers));
            }

            foreach (var finger in lastPlayedFingers) {
                if (sessionData.success) {
                    if (_levelData.NumOfSuccessfulTrials >= _numOfSuccessfulTrialsBeforeIncreasingHeightDifficulty) {
                        _levelData.NumOfSuccessfulTrials = 0;

                        updatedHeights[finger] = Mathf.Min(_mvcToHeights[finger], updatedHeights[finger] + 1);
                    }
                }
                else {
                    Debug.Log("Finger height + " + updatedHeights[finger]);
                    updatedHeights[finger] = Mathf.Max(1, updatedHeights[finger] - 1);
                }
            }

            if (_allowedFingers.Count >= 1) {
                // In this part, we will adjust the next playable fingers based on the finger method
                switch (_fingerMethod) {
                    case FingerMethod.SingleFinger:
                        Debug.Log("UpdateDifficulty: SingleFinger");
                        // if session was successful, increase the height of the active finger

                        // Randomly select a finger from the allowed fingers
                        Debug.Log("allowedFingers: " + string.Join(",", _allowedFingers));
                        var randomFinger =
                            _allowedFingers.ElementAt(Random.Range(0, _allowedFingers.Count));

                        _activeFingers.Clear();
                        _activeFingers.Add(randomFinger);
                        // Make sure the current height for this finger is between the range of 1 and the MVC/4 of the finger
                        // If so, leave it as is, else set it to 1 or the MVC/4 of the finger
                        if (updatedHeights[randomFinger] == 0) {
                            updatedHeights[randomFinger] = 1;
                        }
                        else if (updatedHeights[randomFinger] > _mvcToHeights[randomFinger]) {
                            updatedHeights[randomFinger] = _mvcToHeights[randomFinger];
                        }


                        break;
                    case FingerMethod.Individuation:
                        Debug.Log("UpdateDifficulty: Individuation");
                        // in individuation, we want to make the same thing in single finger,
                        // we choose a random finger from the allowed fingers and activate it with a height in the valid range 
                        // but we want to make sure also to activate the other allowed fingers with a height of 0

                        // Randomly select a finger from the allowed fingers
                        var randomFingerIndividuation =
                            _allowedFingers.ElementAt(UnityEngine.Random.Range(0, _allowedFingers.Count));

                        // var howManySimultaneousFingers = _activeFingers.Count;

                        // Make sure the current height for this finger is between the range of 1 and the MVC/4 of the finger
                        // If so, leave it as is, else set it to 1 or the MVC/4 of the finger
                        if (updatedHeights[randomFingerIndividuation] == 0) {
                            updatedHeights[randomFingerIndividuation] = 1;
                        }
                        else if (updatedHeights[randomFingerIndividuation] > _mvcToHeights[randomFingerIndividuation]) {
                            updatedHeights[randomFingerIndividuation] = _mvcToHeights[randomFingerIndividuation];
                        }

                        // if the last session was a success,
                        // increase the number of static fingers in individuation up to the allowed fingers count - 1
                        if (sessionData.success) {
                            _numOfStaticFingersIndividuation = Mathf.Min(_allowedFingers.Count - 1,
                                _numOfStaticFingersIndividuation + 1);
                        }
                        else {
                            _numOfStaticFingersIndividuation = Mathf.Max(1, _numOfStaticFingersIndividuation - 1);
                        }

                        // Activate the other allowed fingers with a height of 0
                        // choose _numOfStaticFingersIndividuation arbitrarily num fingers from _allowedFingers that is not the randomFingerIndividuation, and apply their height to be 0
                        // it may be that the _numOfStaticFingersIndividuation is less than the number of allowed fingers, so we need to choose randomly from the allowed fingers
                        var nonActiveFingersIndividuation = _allowedFingers.ToList();
                        nonActiveFingersIndividuation.Remove(randomFingerIndividuation);
                        for (int i = 0; i < _numOfStaticFingersIndividuation; i++) {
                            if (_numOfStaticFingersIndividuation >= _allowedFingers.Count) {
                                break;
                            }

                            var randomStaticFinger =
                                nonActiveFingersIndividuation[Random.Range(0, nonActiveFingersIndividuation.Count)];
                            updatedHeights[randomStaticFinger] = 0;
                            nonActiveFingersIndividuation.Remove(randomStaticFinger);
                        }


                        _activeFingers.Clear();

                        // Add the random finger and the other fingers with a height of 0 to the active fingers
                        _activeFingers.UnionWith(_allowedFingers);
                        break;
                    case FingerMethod.MultiFingerSimultaneous:
                        Debug.Log("UpdateDifficulty: MultiFingerSimultaneous");
                        // in multi-finger simultaneous, we want to make the same thing in individuation, but we want to put all the fingers with height greater than 0


                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // shuffle all the heights values that are not an active finger index, using the non-active fingers
            var nonActiveFingers = _nonActiveFingers.ToList();
            foreach (int finger in nonActiveFingers) {
                updatedHeights[finger] = Random.Range(1, _mvcToHeights[finger]);
            }

            // Set the updated heights
            BridgeDataManager.SetHeights(updatedHeights);


            // Set the playable units
            bool[] playableUnits = new bool[5];
            for (int i = 0; i < BridgeDataManager.PlayableUnits.Length; i++) {
                playableUnits[i] = _activeFingers.Contains(i);
            }

            BridgeDataManager.SetPlayableUnits(playableUnits);
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