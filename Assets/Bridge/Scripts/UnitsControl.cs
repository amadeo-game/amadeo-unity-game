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
            BridgeEvents.BridgeStateChanged += OnBridgeStateChanged;
            BridgeEvents.ActiveUnitChanged += OnActiveUnitChanged;
            BridgeEvents.MvcExtensionUpdated += OnMvcExtensionUpdated;
            BridgeEvents.MvcFlexionUpdated += OnMvcFlexionUpdated;
            BridgeEvents.UnitGraceUpdated += OnUnitGraceUpdated;
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

        private void OnDisable() {
            BridgeEvents.BridgeStateChanged -= OnBridgeStateChanged;
            BridgeEvents.ActiveUnitChanged -= OnActiveUnitChanged;
            BridgeEvents.MvcExtensionUpdated -= OnMvcExtensionUpdated;
            BridgeEvents.MvcFlexionUpdated -= OnMvcFlexionUpdated;
            BridgeEvents.UnitGraceUpdated -= OnUnitGraceUpdated;
        }

        private void OnActiveUnitChanged(int unitIndex, bool isEnable) {
            if (!_unitsInitialized) {
                return;
            }

            _moveUnits[unitIndex].SetControl(isEnable);
        }

        private void OnBridgeStateChanged(BridgeStates state) {
            if (!_unitsInitialized) {
                return;
            }

            if (state is BridgeStates.Paused) {
                foreach (MoveUnit unit in _moveUnits) {
                    unit.SetControl(false, resetPos: false);
                }
            }
            else if (state is BridgeStates.BridgeCollapsing || state is BridgeStates.BridgeCompleting) {
                var heights = BridgeDataManager.Heights;
                for (int i = 0; i < _moveUnits.Length; i++) {
                    _moveUnits[i].SetControl(false, goToHeight: heights[i]);
                    _unitsInitialized = false;
                }
            }
            else if (state is BridgeStates.StartingGame) {
                foreach (var moveUnit in _moveUnits) {
                    moveUnit.SetControl(false);
                }
            }
            else if (state is BridgeStates.InGame) {
                var playables = BridgeDataManager.PlayableUnits;
                for (int i = 0; i < _moveUnits.Length; i++) {
                    if (playables[i]) {
                        _moveUnits[i].SetControl(true, resetPos: false);
                    }
                }
            }
        }


        public void SetPlayerUnits(GameObject[] units) {
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