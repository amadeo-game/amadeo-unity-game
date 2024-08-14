using System;
using System.Linq;
using BridgePackage;
using UnityEngine;
using UnityEngine.Serialization;

namespace BridgePackage {
    [RequireComponent(typeof(BridgeDataManager))]
    public class UnitsControl : MonoBehaviour {
        [SerializeField] private float _graceGrowthRate = 0.5f;
        private GameObject[] _playerUnits;
        private MoveUnit[] _moveUnits; // Array to store MoveUnit components
        private BoxCollider2D[] _unitsColliders; // Array to store colliders of player units
        private readonly bool[] _unitsRotated = new bool[5];
        private bool _unitsInitialized = false;

        private void Start() {
            _moveUnits = new MoveUnit[5];
        }

        private void OnEnable() {
            BridgeEvents.StartingGameState += SetPlayerUnits;
            BridgeEvents.InGameState += OnInGameState;
            
            // BridgeEvents.BridgeStateChanged += OnBridgeStateChanged;
            BridgeEvents.ActiveUnitChanged += OnActiveUnitChanged;
            BridgeEvents.MvcExtensionUpdated += OnMvcExtensionUpdated;
            BridgeEvents.MvcFlexionUpdated += OnMvcFlexionUpdated;
            BridgeEvents.UnitGraceUpdated += OnUnitGraceUpdated;
            
            BridgeEvents.GamePausedState += OnPausedState;
            BridgeEvents.ResumeGameAction += OnResumeState;
            
            BridgeEvents.BridgeCollapsingState += OnGameOverState;
            BridgeEvents.BridgeCompletingState += OnGameOverState;
            
            
        }

        private void OnInGameState() {
            var playables = BridgeDataManager.PlayableUnits;
            for (int i = 0; i < _moveUnits.Length; i++) {
                if (playables[i]) {
                    _moveUnits[i].SetControl(true, resetPos: false);
                }
            }
        }

        private void OnDisable() {
            BridgeEvents.StartingGameState -= SetPlayerUnits;
            BridgeEvents.InGameState -= OnInGameState;
            
            // BridgeEvents.BridgeStateChanged -= OnBridgeStateChanged;
            BridgeEvents.ActiveUnitChanged -= OnActiveUnitChanged;
            BridgeEvents.MvcExtensionUpdated -= OnMvcExtensionUpdated;
            BridgeEvents.MvcFlexionUpdated -= OnMvcFlexionUpdated;
            BridgeEvents.UnitGraceUpdated -= OnUnitGraceUpdated;

            BridgeEvents.GamePausedState -= OnPausedState;
            BridgeEvents.ResumeGameAction -= OnResumeState;
            
            BridgeEvents.BridgeCollapsingState -= OnGameOverState;
            BridgeEvents.BridgeCompletingState -= OnGameOverState;
            
        }

        private void OnUnitGraceUpdated(int fingerIndex, float graceValue) {
            if (!_unitsInitialized) {
                return;
            }
            Vector2 newSize = _unitsColliders[fingerIndex].size;
            if (_unitsRotated[fingerIndex]) {
                newSize.x = _graceGrowthRate * graceValue;
            }
            else {
                newSize.y = _graceGrowthRate * graceValue;
            }

            _unitsColliders[fingerIndex].size = newSize;
        }

        private void OnMvcExtensionUpdated(int fingerIndex, float mvcValue) {
            if (!_unitsInitialized) {
                return;
            }

            _moveUnits[fingerIndex].MvcE = mvcValue;
        }

        private void OnMvcFlexionUpdated(int fingerIndex, float mvcValue) {
            if (!_unitsInitialized) {
                return;
            }

            _moveUnits[fingerIndex].MvcF = mvcValue;
        }



        private void OnGameOverState() {
            var heights = BridgeDataManager.Heights;
            for (int i = 0; i < _moveUnits.Length; i++) {
                _moveUnits[i].SetControl(false, goToHeight: heights[i]);
                _unitsInitialized = false;
            }        }

        private void OnResumeState() {
            if (!_unitsInitialized) {
                return;
            }

            foreach (MoveUnit unit in _moveUnits) {
                unit.SetControl(true);
            }
        }

        private void OnPausedState() {
            if (!_unitsInitialized) {
                return;
            }


            foreach (MoveUnit unit in _moveUnits) {
                unit.SetControl(false, resetPos: false);
            }
        }

        private void OnActiveUnitChanged(int unitIndex, bool isEnable) {
            if (!_unitsInitialized) {
                return;
            }

            _moveUnits[unitIndex].SetControl(isEnable);
        }

        // private void OnBridgeStateChanged(BridgeStates state) {
        //
        //     if (state is BridgeStates.StartingGame) {
        //         foreach (var moveUnit in _moveUnits) {
        //             moveUnit.SetControl(false);
        //         }
        //
        //     }
        //     else if (state is BridgeStates.InGame) {
        //
        //     }
        // }


        // Set player units, When Bridge is built 
        public void SetPlayerUnits() {
            Debug.Log("UnitsControl: SetPlayerUnits called");
            var units = Bridge.PlayerUnits;
            _moveUnits = units.Select(unit => unit.GetComponent<MoveUnit>()).ToArray();
            _unitsColliders = _moveUnits.Select(unit => unit.Collider).ToArray();


            // check for each unit if it is rotated
            for (int i = 0; i < _moveUnits.Length; i++) {
                _unitsRotated[i] = Math.Abs(_moveUnits[i].transform.rotation.eulerAngles.z - 90f) < 10;
            }

            // Set FingerUnit enum for each player Unit
            for (int i = 0; i < _moveUnits.Length; i++) {
                int localIndex = i;
                _moveUnits[i].SetFingerUnit((FingerUnit)localIndex, localIndex);
            }

            _unitsInitialized = true;
        }

        public void CollectSessionData(bool success) {
            _unitsInitialized = false;
            float[] bestHeights = _moveUnits.Select(unit => unit.BestHeight).ToArray();
            BridgeDataManager.SetSessionData(bestHeights, success);
        }
    }
}