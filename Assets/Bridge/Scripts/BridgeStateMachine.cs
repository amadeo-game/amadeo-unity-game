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

        private Dictionary<FingerUnit, bool> _unitPlacementStatus;

        private bool _isPaused = false;


        private void Awake() {
            currentState = BridgeStates.Idle;
            _unitPlacementStatus = new Dictionary<FingerUnit, bool> {
                { FingerUnit.First, true },
                { FingerUnit.Second, true },
                { FingerUnit.Third, true },
                { FingerUnit.Fourth, true },
                { FingerUnit.Fifth, true }
            };
        }

        private void OnEnable() {
            BridgeTimer.OnTimerComplete += HandleTimerComplete;
            BridgeEvents.ZeroingCompleted += OnZeroingCompleted;
            BridgeEvents.UnitPlacementStatusChanged += UnitPlaced;
        }

        private void OnZeroingCompleted() {
            ChangeState(BridgeStates.StartingGame);
        }

        private void OnDisable() {
            BridgeTimer.OnTimerComplete -= HandleTimerComplete;
            BridgeEvents.ZeroingCompleted -= OnZeroingCompleted;
            BridgeEvents.UnitPlacementStatusChanged -= UnitPlaced;
        }

        private void HandleTimerComplete() {
            if (currentState == BridgeStates.InGame) {
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
                    if (!_isPaused) {
                        BridgeEvents.OnGameStart?.Invoke();
                    }
                    else {
                        _isPaused = false;
                    }

                    StartCoroutine(BridgeTimer.StartTimer()); // Start the timer with the configured duration
                    break;

                case BridgeStates.Paused:
                    _isPaused = true;
                    BridgeEvents.BridgeStateChanged?.Invoke(BridgeStates.Paused);
                    break;

                case BridgeStates.BridgeCollapsing:
                    BridgeEvents.BridgeStateChanged?.Invoke(BridgeStates.BridgeCollapsing);
                    BridgeEvents.BridgeCollapsed?.Invoke();
                    break;

                case BridgeStates.GameFailed:
                    HandleGameWinOrLose(won: false);
                    break;

                case BridgeStates.BridgeCompleting:
                    BridgeEvents.BridgeStateChanged?.Invoke(BridgeStates.BridgeCompleting);
                    BridgeEvents.BridgeIsComplete?.Invoke();
                    BridgeTimer.ResetTimer();
                    break;

                case BridgeStates.GameWon:
                    HandleGameWinOrLose(won: true);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void HandleGameWinOrLose(bool won) {
            if (won) {
                BridgeEvents.BridgeStateChanged?.Invoke(BridgeStates.GameWon);
            }
            else {
                BridgeEvents.BridgeStateChanged?.Invoke(BridgeStates.GameFailed);
            }

            ChangeState(BridgeStates.Idle);
        }

        internal IEnumerator StartingGame() {
            // Wait for 3 seconds before starting the game (for the player to see the bridge, and for Animation to start)
            yield return new WaitForSecondsRealtime(3f);
            // Play Animation (countdown on Screen)
            ChangeState(BridgeStates.InGame);
        }

        public void StartBuilding() {
            if (currentState is not (BridgeStates.Idle or BridgeStates.BridgeReady or BridgeStates.GameFailed
                or BridgeStates.GameWon)) {
                return;
            }

            ChangeState(BridgeStates.Building);
        }

        public void ForceResetBridge(int[] heights, BridgeTypeSO bridgeTypeSO) {
            OnForceResetBridge?.Invoke(heights, bridgeTypeSO);
        }

        public void UnitPlaced(FingerUnit fingerUnit, bool isPlaced) {
            _unitPlacementStatus[fingerUnit] = isPlaced;
            CheckAllUnitsPlaced();
        }

        private void CheckAllUnitsPlaced() {
            if (_unitPlacementStatus.Count == 5 && _unitPlacementStatus.Values.All(placed => placed)) {
                ChangeState(BridgeStates.BridgeCompleting);
            }
        }
    }
}