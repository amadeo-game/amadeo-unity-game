using System;
using System.Linq;
using BridgePackage;
using UnityEngine;

namespace BridgePackage {
    [RequireComponent(typeof(BridgeDataManager))]
    public class UnitsControl : MonoBehaviour {
        private GameObject[] _playerUnits;
        private MoveUnit[] _moveUnits; // Array to store MoveUnit components
        private bool _unitsInitialized = false;

        private void Start() {
            _moveUnits = new MoveUnit[5];
        }

        private void OnEnable() {
            BridgeEvents.BridgeStateChanged += OnBridgeStateChanged;
            BridgeEvents.ActiveUnitChanged += OnActiveUnitChanged;
            BridgeEvents.MvcExtensionUpdated += OnMvcExtensionUpdated;
            BridgeEvents.MvcFlexionUpdated += OnMvcFlexionUpdated;
        }

        private void OnMvcExtensionUpdated(int fingerIndex, float mvcValue) {
            _moveUnits[fingerIndex].MvcE = mvcValue;
        }

        private void OnMvcFlexionUpdated(int fingerIndex, float mvcValue) {
            _moveUnits[fingerIndex].MvcF = mvcValue;
        }

        private void OnDisable() {
            BridgeEvents.BridgeStateChanged -= OnBridgeStateChanged;
            BridgeEvents.ActiveUnitChanged -= OnActiveUnitChanged;
            BridgeEvents.MvcExtensionUpdated -= OnMvcExtensionUpdated;
            BridgeEvents.MvcFlexionUpdated -= OnMvcFlexionUpdated;
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
                }
            }
            else if (state is BridgeStates.InGame) {
                var playables = BridgeDataManager.PlayableUnits;
                for (int i = 0; i < _moveUnits.Length; i++) {
                    if (playables[i]) {
                        _moveUnits[i].SetControl(true);
                    }
                }
            }
        }


        public void SetPlayerUnits(GameObject[] units) {
            _moveUnits = units.Select(unit => unit.GetComponent<MoveUnit>()).ToArray();


            // Set FingerUnit enum for each player Unit
            for (int i = 0; i < _moveUnits.Length; i++) {
                Debug.Log("Setting FingerUnit for unit " + i + " with index " + i + " and name " + _moveUnits[i].name);
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