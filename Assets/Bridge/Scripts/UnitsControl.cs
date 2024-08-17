using System;
using System.Collections;
using Unity.Collections;
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
        private Int16 NUMOFUNITS = 5;
        
        private float[] _bestHeights = new float[5];
        
        SetBestHeight _setBestHeight;

        delegate void SetBestHeight(float height, int index);
        public float BestHeights(int index) => _bestHeights[index];
            

        private void Start() {
            _moveUnits = new MoveUnit[5];
        }

        private void OnEnable() {
            BridgeEvents.StartingGameState += EnablePlayerUnitControl;

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

        // Enable player units control
        private void EnablePlayerUnitControl() {
            _setBestHeight = BridgeDataManager.IsFlexion
                ? (height ,i) => _bestHeights[i] = Mathf.Min(height, _bestHeights[i])
                : (height ,i) => _bestHeights[i] = Mathf.Max(height, _bestHeights[i]); 
            var playables = BridgeDataManager.PlayableUnits;
            for (int i = 0; i < _moveUnits.Length; i++) {
                if (playables[i]) {
                    _moveUnits[i].ResetPosition();
                    _moveUnits[i].SetControl(true);
                }
            }
            
            EnableGuideUnits();
        }
        
        private void EnableGuideUnits() {
            StartCoroutine(EnableGuideUnitsRoutine());
        }

        private IEnumerator EnableGuideUnitsRoutine() {
            yield return new WaitForSecondsRealtime(1f);
            Bridge.EnableGuideUnits();
            BridgeEvents.FinishStartingGameProcess?.Invoke();
        }

        private void OnDisable() {
            BridgeEvents.StartingGameState -= EnablePlayerUnitControl;

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
                _moveUnits[i].SetControl(false);
                _unitsInitialized = false;
            }
            
        }

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
                unit.SetControl(false);
            }
        }

        private void OnActiveUnitChanged(int unitIndex, bool isEnable) {
            if (!_unitsInitialized) {
                return;
            }

            _moveUnits[unitIndex].SetControl(isEnable);
            _moveUnits[unitIndex].ResetPosition();
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
        internal void SetPlayerUnits() {
            // must have for operating this method: Bridge.PlayerUnits is not null
            Debug.Log("UnitsControl: SetPlayerUnits called");

            // store player units components
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

        // internal void OnForcesUpdated(float[] forces) {
        //     for (int i = 0; i < NUMOFUNITS; i++) {
        //         var height = forces[i]; // There is a fixed size of total 5 units, so forces is always of size 5
        //         _moveUnits[i].OnForcesUpdated(height);
        //         _setBestHeight(height, i);
        //     }
        // }
        
        internal void OnForcesUpdated(NativeArray<float> forces)
        {
            for (int i = 0; i < forces.Length; i++)
            {
                float height = forces[i]; // Forces array is a NativeArray<float> of fixed size
                _moveUnits[i].OnForcesUpdated(height);
                _setBestHeight(height, i);
            }
        }

        public void CollectSessionData(bool success) {
            _unitsInitialized = false;
            BridgeDataManager.SetSessionData(Bridge.PlayerUnitsHeights,_bestHeights, success);
        }
    }
}