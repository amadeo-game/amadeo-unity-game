using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BridgePackage {
    internal enum BridgeState {
        Idle,
        Building,
        Built,
        InGame,
        Collapsing,
        Collapsed,
        Winning,
        Win
    }

    internal class BridgeStateMachine : MonoBehaviour {
        private BridgeMediator bridgeMediator;
        private Dictionary<FingerUnit, bool> dicUnitPlaced;
        private BridgeState currentState = BridgeState.Idle;

        private void Awake() {
            dicUnitPlaced = new Dictionary<FingerUnit, bool> {
                { FingerUnit.first, false },
                { FingerUnit.second, false },
                { FingerUnit.third, false },
                { FingerUnit.fourth, false },
                { FingerUnit.fifth, false },
            };
        }

        internal void Initialize(BridgeMediator mediator) {
            bridgeMediator = mediator
                ? mediator
                : throw new ArgumentNullException(nameof(mediator), "BridgeMediator cannot be null.");
            bridgeMediator.OnUnitPlaced += OnUnitPlaced;
        }

        private void OnDestroy() {
            if (bridgeMediator != null) {
                bridgeMediator.OnUnitPlaced -= OnUnitPlaced;
            }
        }

        internal bool CanBuild => currentState == BridgeState.Idle;

        internal bool CanForceReset => currentState != BridgeState.Collapsing &&
                                       currentState != BridgeState.Building &&
                                       currentState != BridgeState.Winning;

        private void OnUnitPlaced(FingerUnit fingerUnit, bool isPlaced) {
            if (dicUnitPlaced.ContainsKey(fingerUnit)) {
                dicUnitPlaced[fingerUnit] = isPlaced;
            }

            if (!dicUnitPlaced.Values.Contains(false)) {
                StartSuccess();
            }
        }

        internal void StartBuilding() {
            if (currentState == BridgeState.Idle) {
                currentState = BridgeState.Building;
                bridgeMediator?.BuildStart();
            }
        }

        internal void StartBuilding(int[] unitHeights, BridgeCollectionSO collectionSO = null,
            int bridgeTypeIndex = 0) {
            if (currentState == BridgeState.Idle) {
                currentState = BridgeState.Building;
                bridgeMediator?.BuildStart(unitHeights, collectionSO, bridgeTypeIndex);
            }
            else if (CanForceReset) {
                bridgeMediator?.ForceResetBridge(unitHeights, collectionSO, bridgeTypeIndex);
                currentState = BridgeState.Building;
            }
        }


        internal void FinishBuilding() {
            if (currentState == BridgeState.Building) {
                currentState = BridgeState.Built;
                BridgeAPI.NotifyBridgeReady();
            }
        }

        internal void StartGame() {
            if (currentState == BridgeState.Built) {
                currentState = BridgeState.InGame;
                bridgeMediator?.EnablePlayerUnits();
                // BridgeAPI.BridgeReady?.Invoke();
                BridgeAPI.NotifyGameStart();
            }
        }

        internal void StartCollapsing() {
            if (currentState == BridgeState.Built || currentState == BridgeState.InGame) {
                currentState = BridgeState.Collapsing;
                bridgeMediator?.CollapseStart();
            }
        }

        internal void CompleteCollapse() {
            if (currentState == BridgeState.Collapsing) {
                currentState = BridgeState.Collapsed;
                bridgeMediator?.CollapseComplete();
                BridgeAPI.NotifyBridgeCollapsed();
            }
        }

        internal void StartSuccess() {
            if (currentState == BridgeState.InGame) {
                currentState = BridgeState.Winning;
                bridgeMediator?.SuccessStart();
            }
        }

        internal void FinishSuccess() {
            if (currentState == BridgeState.Winning) {
                currentState = BridgeState.Win;
                bridgeMediator?.SuccessComplete();
                BridgeAPI.NotifyBridgeIsComplete();
            }
        }

        internal void ResetState() {
            if (currentState == BridgeState.Collapsed || currentState == BridgeState.Win) {
                currentState = BridgeState.Idle;
            }
        }
    }
}