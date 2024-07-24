using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BridgePackage {
    public enum BridgeStates {
        Idle,
        Building,
        BridgeReady,
        InZeroF,
        Paused,
        StartingGame,
        InGame,
        BridgeCollapsing,
        GameFailed,
        BridgeCompleting,
        GameWon
    }

    public class BridgeStateMachine : MonoBehaviour {
        public event Action<int[], BridgeTypeSO> OnForceResetBridge;

        internal static BridgeStates currentState { get; private set; }
        
        private Dictionary<FingerUnit, bool> unitPlacementStatus;


        private void Awake() {
            currentState = BridgeStates.Idle;
            unitPlacementStatus = new Dictionary<FingerUnit, bool>();
        }

        private void OnEnable() {
            BridgeTimer.OnTimerComplete += HandleTimerComplete;
        }
        
        private void OnDisable() {
            BridgeTimer.OnTimerComplete -= HandleTimerComplete;
        }

        private void HandleTimerComplete()
        {
            if (currentState == BridgeStates.InGame)
            {
                ChangeState(BridgeStates.BridgeCollapsing);
            }
        }
        
        internal void ForceCollapseBridge() {
            BridgeTimer.ResetTimer();
            ChangeState(BridgeStates.BridgeCollapsing);
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
                case BridgeStates.StartingGame:
                    StartCoroutine(StartingGame());
                    BridgeEvents.BridgeStateChanged?.Invoke(BridgeStates.StartingGame);
                    break;
                case BridgeStates.InGame:
                    BridgeEvents.BridgeStateChanged?.Invoke(BridgeStates.InGame);
                    BridgeEvents.OnGameStart?.Invoke();
                    StartCoroutine(BridgeTimer.StartTimer()); // Start the timer with the configured duration
                    break;
                case BridgeStates.Paused:
                    BridgeEvents.BridgeStateChanged?.Invoke(BridgeStates.Paused);
                    break;
                case BridgeStates.BridgeCollapsing:
                    BridgeEvents.BridgeStateChanged?.Invoke(BridgeStates.BridgeCollapsing);
                    break;
                
                case BridgeStates.GameFailed:
                    BridgeEvents.BridgeStateChanged?.Invoke(BridgeStates.GameFailed);
                    break;
                
                case BridgeStates.BridgeCompleting:
                    BridgeEvents.BridgeStateChanged?.Invoke(BridgeStates.BridgeCompleting);
                    BridgeTimer.ResetTimer();
                    break;
                
                case BridgeStates.GameWon:
                    BridgeEvents.BridgeStateChanged?.Invoke(BridgeStates.GameWon);
                    BridgeEvents.BridgeIsComplete?.Invoke();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        internal IEnumerator StartingGame() {
            // Wait for 3 seconds before starting the game (for the player to see the bridge, and for Animation to start)
            yield return new WaitForSecondsRealtime(3f); 
            // Play Animation (countdown on Screen)
            ChangeState(BridgeStates.InGame);
        }

        public void StartBuilding() {
            if (currentState is not (BridgeStates.Idle or BridgeStates.BridgeReady or BridgeStates.GameFailed or BridgeStates.GameWon)) {
                return;
            }
            ChangeState(BridgeStates.Building);
        }

        public void ForceResetBridge(int[] heights, BridgeTypeSO bridgeTypeSO) {
            OnForceResetBridge?.Invoke(heights, bridgeTypeSO);
        }

        public void UnitPlaced(FingerUnit fingerUnit, bool isPlaced) {
            unitPlacementStatus[fingerUnit] = isPlaced;
            Debug.Log("Unit placed: " + fingerUnit + " " + isPlaced);
            CheckAllUnitsPlaced();
        }

        private void CheckAllUnitsPlaced() {
            if (unitPlacementStatus.Count == 5 && unitPlacementStatus.Values.All(placed => placed)) {
                ChangeState(BridgeStates.BridgeCompleting);
            }
        }
    }
}
