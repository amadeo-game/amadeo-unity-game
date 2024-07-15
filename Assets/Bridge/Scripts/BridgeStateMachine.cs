using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BridgePackage {
    public enum BridgeStates {
        Idle,
        Building,
        BridgeReady,
        InGame,
        Collapse,
        GameFailed,
        BridgeComplete,
        GameWon
    }

    public class BridgeStateMachine : MonoBehaviour {
        public event Action<int[], BridgeTypeSO> OnBuildStartWithHeights;
        public event Action OnEnablePlayerUnits;
        public event Action OnCollapseStart;
        public event Action OnSuccessStart;
        public event Action<int[], BridgeTypeSO> OnForceResetBridge;

        public BridgeStates currentState { get; private set; }

        private Dictionary<FingerUnit, bool> unitPlacementStatus;

        private void Awake() {
            currentState = BridgeStates.Idle;
            unitPlacementStatus = new Dictionary<FingerUnit, bool>();
        }

        public void ChangeState(BridgeStates state) {
            currentState = state;
            switch (state) {
                case BridgeStates.Idle:

                    break;
                case BridgeStates.Building:

                    break;
                case BridgeStates.BridgeReady:
                    BridgeAPI.NotifyBridgeReady();
                    break;
                case BridgeStates.InGame:
                    Debug.Log("InGameState::Enter");
                    StartGame();
                    BridgeAPI.NotifyGameStart();
                    break;
                case BridgeStates.Collapse:
                    StartCollapsingBridge();
                    break;
                case BridgeStates.GameFailed:
                    BridgeAPI.NotifyBridgeCollapsing();

                    break;
                case BridgeStates.BridgeComplete:
                    StartCompleteBridge();
                    break;
                case BridgeStates.GameWon:
                    BridgeAPI.NotifyBridgeIsComplete();

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public void StartBuilding(int[] heights, BridgeTypeSO bridgeTypeSO) {
            if (currentState is not (BridgeStates.Idle or BridgeStates.BridgeReady or BridgeStates.GameFailed or BridgeStates.GameWon)) {
                return;
            }

            OnBuildStartWithHeights?.Invoke(heights, bridgeTypeSO);
            ChangeState(BridgeStates.Building);
        }

        public void StartGame() {
            OnEnablePlayerUnits?.Invoke();
        }

        public void StartCompleteBridge() {
            var bridgeClient = GetComponent<BridgeClient>();
            bridgeClient.StopReceiveData();
            
            var unitsControl = GetComponent<UnitsControl>();
            unitsControl.DisableControl();



            OnSuccessStart?.Invoke();
        }

        private void StartCollapsingBridge() {
            var unitsControl = GetComponent<UnitsControl>();
            unitsControl.DisableControl();

            var bridgeClient = GetComponent<BridgeClient>();
            bridgeClient.StopReceiveData();

            OnCollapseStart?.Invoke();
        }

        public void ForceResetBridge(int[] heights, BridgeTypeSO bridgeTypeSO) {
            OnForceResetBridge?.Invoke(heights, bridgeTypeSO);
        }

        public void UnitPlaced(FingerUnit fingerUnit, bool isPlaced) {
            unitPlacementStatus[fingerUnit] = isPlaced;
            CheckAllUnitsPlaced();
        }

        private void CheckAllUnitsPlaced() {
            if (unitPlacementStatus.Count == 5 && unitPlacementStatus.Values.All(placed => placed)) {
                StartCompleteBridge();
                ChangeState(BridgeStates.BridgeComplete);
            }
        }
    }
}