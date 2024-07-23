using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BridgePackage {
    public enum BridgeStates {
        Idle,
        Building,
        BridgeReady,
        InZeroF,
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

        internal static BridgeStates currentState { get; private set; }
        
        private Dictionary<FingerUnit, bool> unitPlacementStatus;
        private BridgeTimer timer;
        private int[] unitHeights;
        private BridgeTypeSO bridgeTypeSO;
        internal bool isLeftHand;
        internal bool isFlexion;
        internal static bool isZeroF;
        
        private float[] mvcValues;
        private bool[] playableUnits;
        private float[] unitsGrace;
        private float timeDuration;

        private void Awake() {
            currentState = BridgeStates.Idle;
            unitPlacementStatus = new Dictionary<FingerUnit, bool>();
            timer = GetComponent<BridgeTimer>();
            timer.OnTimerComplete += HandleTimerComplete;
        }

        private void HandleTimerComplete()
        {
            if (currentState == BridgeStates.InGame)
            {
                ChangeState(BridgeStates.Collapse);
            }
        }

        public void ChangeState(BridgeStates state) {
            currentState = state;
            switch (state) {
                case BridgeStates.Idle:
                    BridgeEvents.BridgeStateChanged?.Invoke(BridgeStates.Idle);
                    break;
                case BridgeStates.Building:
                    BridgeEvents.BridgeStateChanged?.Invoke(BridgeStates.Building);
                    break;
                case BridgeStates.BridgeReady:
                    BridgeEvents.BridgeStateChanged?.Invoke(BridgeStates.BridgeReady);
                    BridgeEvents.BridgeReady?.Invoke();
                    break;
                case BridgeStates.InZeroF:
                    BridgeEvents.BridgeStateChanged?.Invoke(BridgeStates.InZeroF);
                    break;
                case BridgeStates.InGame:
                    BridgeEvents.BridgeStateChanged?.Invoke(BridgeStates.InGame);
                    OnEnablePlayerUnits?.Invoke();
                    
                    BridgeEvents.OnGameStart?.Invoke();
                    timer.StartTimer(timeDuration); // Start the timer with the configured duration
                    break;
                case BridgeStates.Collapse:
                    
                    StartCollapsingBridge();
                    break;
                case BridgeStates.GameFailed:
                    BridgeEvents.BridgeStateChanged?.Invoke(BridgeStates.GameFailed);
                    BridgeEvents.BridgeCollapsed?.Invoke();
                    break;
                case BridgeStates.BridgeComplete:
                    BridgeEvents.BridgeStateChanged?.Invoke(BridgeStates.BridgeComplete);
                    StartCompleteBridge();
                    break;
                case BridgeStates.GameWon:
                    BridgeEvents.BridgeStateChanged?.Invoke(BridgeStates.GameWon);
                    BridgeEvents.BridgeIsComplete?.Invoke();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public void StartBuilding() {
            if (currentState is not (BridgeStates.Idle or BridgeStates.BridgeReady or BridgeStates.GameFailed or BridgeStates.GameWon)) {
                return;
            }

            OnBuildStartWithHeights?.Invoke(unitHeights, this.bridgeTypeSO);
            ChangeState(BridgeStates.Building);
        }

        public void StartCompleteBridge() {
            var bridgeClient = GetComponent<BridgeClient>();
            bridgeClient.StopReceiveData();
            
            var unitsControl = GetComponent<UnitsControl>();
            unitsControl.DisableControl();

            timer.ResetTimer(); // Stop and reset the timer

            OnSuccessStart?.Invoke();
        }

        private void StartCollapsingBridge() {
            var unitsControl = GetComponent<UnitsControl>();
            unitsControl.DisableControl();

            var bridgeClient = GetComponent<BridgeClient>();
            bridgeClient.StopReceiveData();

            timer.ResetTimer(); // Stop and reset the timer

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

        public void SetGameParameters(int[] heights, BridgeTypeSO bridgeTypeSO, bool isLeftHand, bool isFlexion, float[] mvcValues, bool[] playableUnits, float[] unitsGrace, float timeDuration)
        {
            unitHeights = heights;
            this.bridgeTypeSO = bridgeTypeSO;
            this.isLeftHand = isLeftHand;
            this.isFlexion = isFlexion;
            this.mvcValues = mvcValues;
            this.playableUnits = playableUnits;
            this.unitsGrace = unitsGrace;
            this.timeDuration = timeDuration;
        }
    }
}
