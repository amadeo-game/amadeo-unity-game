using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace BridgePackage {
    public enum BridgeStates {
        Idle,
        Building,
        AnimationBuilding,
        BridgeReady,
        InZeroF,
        Paused,
        StartingGame,
        InGame,
        BridgeCollapsing,
        AnimationBridgeCollapsing,
        GameFailed,
        BridgeCompleting,
        AnimationBridgeCompleting,
        BridgeCompleted,
        GameWon
    }

    public class BridgeStateMachine : MonoBehaviour {
        public event Action<int[], BridgeTypeSO> OnForceResetBridge;

        internal static BridgeStates currentState { get; private set; }

        private Dictionary<FingerUnit, bool> _unitPlacementStatus;

        private bool _isPaused = false;

        // Define if the game is in session mode to load the next trials automatically.
        private bool _inSessionMode = false;

        [SerializeField] private bool _debugMode = false;


        private void Awake() {
            currentState = BridgeStates.Idle;
        }

        private void SetPlayerUnitsDictionary() {
            _unitPlacementStatus = new Dictionary<FingerUnit, bool> {
                { FingerUnit.First, BridgeDataManager.PlayableUnits[0] == false || BridgeDataManager.Heights[0] == 0 },
                { FingerUnit.Second, BridgeDataManager.PlayableUnits[1] == false || BridgeDataManager.Heights[1] == 0 },
                { FingerUnit.Third, BridgeDataManager.PlayableUnits[2] == false || BridgeDataManager.Heights[2] == 0 },
                { FingerUnit.Fourth, BridgeDataManager.PlayableUnits[3] == false || BridgeDataManager.Heights[3] == 0 },
                { FingerUnit.Fifth, BridgeDataManager.PlayableUnits[4] == false || BridgeDataManager.Heights[4] == 0 }
            };
        }


        private void OnEnable() {
            BridgeTimer.OnTimerComplete += HandleTimerComplete;
            BridgeEvents.UnitPlacementStatusChanged += UnitPlaced;

            // Listen to all returned events
            BridgeEvents.FinishedBuildingBridge += () => ChangeState(BridgeStates.AnimationBuilding);
            BridgeEvents.FinishedAnimatingBuildingState += () => ChangeState(BridgeStates.BridgeReady);

            BridgeEvents.FinishedZeroF += () => ChangeState(BridgeStates.StartingGame);
            BridgeEvents.FinishStartingGameProcess += () => ChangeState(BridgeStates.InGame);


            // --- Action Events ---
            BridgeEvents.PlayTrial += OnBuildBridgeAction;
            BridgeEvents.PlaySession += StartSession;
            BridgeEvents.EnableGameInteraction += () => {
                // ChangeState(BridgeStates.InZeroF);
                // TODO: fix the problem of crashing when we dont want zeroF (in ::UnitsControl :: OnInGameState())
                Debug.Log("BridgeStateMachine :: EnableGameInteraction called, ZeroF: " + BridgeDataManager.ZeroF);
                ChangeState(
                    BridgeStates.StartingGame);
            };
            BridgeEvents.PauseGameAction += () => {
                _isPaused = true;
                ChangeState(BridgeStates.Paused);
            };
            BridgeEvents.ResumeGameAction += () => { ChangeState(BridgeStates.InGame); };
            BridgeEvents.CollapseBridgeAction += ForceCollapseBridge;

            BridgeEvents.RestartGameAction += () => { ChangeState(BridgeStates.Idle); };

            BridgeEvents.GameFinishedAction += OnGameFinishedAction;


            // --- Animation Events ---
            BridgeEvents.FinishedAnimatingBridgeCollapsingState += () => ChangeState(BridgeStates.GameFailed);
            BridgeEvents.FinishedAnimatingBridgeCompletingState += () => ChangeState(BridgeStates.BridgeCompleted);
            BridgeEvents.FinishedGameCompletedState += () => ChangeState(BridgeStates.GameWon);
        }

        private void OnGameFinishedAction() {
            // TODO: Implement the game state check

            // Prevent the game from loading the next trial automatically in the next game.
            _inSessionMode = false;

            // Destroy the current bridge, and invoke for final screen
            BridgeEvents.ForceDestroyBridge?.Invoke();

            ChangeState(BridgeStates.Idle);
            BridgeEvents.GameFinishedState?.Invoke();
        }

        private void OnDisable() {
            BridgeTimer.OnTimerComplete -= HandleTimerComplete;
            BridgeEvents.UnitPlacementStatusChanged -= UnitPlaced;

            BridgeEvents.FinishedBuildingBridge -= () => ChangeState(BridgeStates.AnimationBuilding);
            BridgeEvents.FinishedAnimatingBuildingState -= () =>
                ChangeState(BridgeStates.BridgeReady);
            ;

            BridgeEvents.FinishedZeroF -= () => ChangeState(BridgeStates.StartingGame);
            BridgeEvents.FinishStartingGameProcess -= () => ChangeState(BridgeStates.InGame);

            // --- Action Events ---
            BridgeEvents.PlayTrial -= OnBuildBridgeAction;
            BridgeEvents.PlaySession -= StartSession;
            BridgeEvents.EnableGameInteraction -= () => {
                // ChangeState(BridgeStates.InZeroF);
                // TODO: fix the problem of crashing when we dont want zeroF (in ::UnitsControl :: OnInGameState())
                ChangeState(
                    BridgeDataManager.ZeroF ? BridgeStates.InZeroF : BridgeStates.InGame);
            };
            BridgeEvents.PauseGameAction -= () => ChangeState(BridgeStates.Paused);
            BridgeEvents.CollapseBridgeAction -= ForceCollapseBridge;

            BridgeEvents.RestartGameAction -= () => { ChangeState(BridgeStates.Idle); };

            BridgeEvents.GameFinishedAction -= OnGameFinishedAction;

            // --- Animation Events ---
            BridgeEvents.FinishedAnimatingBridgeCollapsingState -= () => ChangeState(BridgeStates.GameFailed);
            BridgeEvents.FinishedAnimatingBridgeCompletingState -= () => ChangeState(BridgeStates.BridgeCompleted);
            BridgeEvents.FinishedGameCompletedState -= () => ChangeState(BridgeStates.GameWon);
        }

        private void StartSession() {
            _inSessionMode = true;
            BridgeEvents.PrepareBridgeConfigs?.Invoke();
            OnBuildBridgeAction();
        }

        private void OnBuildBridgeAction() {
            if (BridgeDataManager.Heights.Length != 5) {
                throw new ArgumentException("unitHeights must have 5 elements.");
            }

            if (Array.Exists(BridgeDataManager.Heights, height => height < -5 || height > 5)) {
                throw new ArgumentException("The height of each unit must be between 0 and 5.");
            }

            Debug.Log("BuildBridge called");
            StartBuilding();
        }

        // private void OnBuildBridgeAction


        private void HandleTimerComplete() {
            if (currentState == BridgeStates.InGame) {
                ChangeState(BridgeStates.BridgeCollapsing);
            }
        }


        internal void ForceCollapseBridge() {
            if (currentState is BridgeStates.InGame || currentState is BridgeStates.Paused) {
                BridgeTimer.ResetTimer();
                ChangeState(BridgeStates.BridgeCollapsing);
            }
        }

        private void ChangeState(BridgeStates state) {
            if (_debugMode) {
                Debug.Log("BridgeStateMachine :: Changing state from " + currentState + " to " + state);
            }

            currentState = state;
            switch (state) {
                case BridgeStates.Idle:
                    BridgeEvents.IdleState?.Invoke();
                    break;

                case BridgeStates.Building:
                    BridgeEvents.BuildingState?.Invoke();
                    break;

                case BridgeStates.AnimationBuilding:
                    BridgeEvents.AnimatingBuildingState?.Invoke();
                    break;

                case BridgeStates.BridgeReady:
                    BridgeEvents.BridgeReadyState?.Invoke();
                    // Change this if you don't want to start the game automatically
                    ChangeState(BridgeDataManager.ZeroF ? BridgeStates.InZeroF : BridgeStates.StartingGame);
                    break;

                case BridgeStates.InZeroF:
                    BridgeEvents.InZeroFState?.Invoke();
                    break;

                case BridgeStates.StartingGame:
                    BridgeEvents.StartingGameState?.Invoke();

                    break;

                case BridgeStates.InGame:
                    StartCoroutine(StartingGame());
                    if (!_isPaused) {
                        SetPlayerUnitsDictionary();
                    }

                    _isPaused = false;
                    break;

                case BridgeStates.Paused:
                    _isPaused = true;
                    BridgeEvents.GamePausedState?.Invoke();
                    BridgeTimer.PauseTimer();
                    break;

                case BridgeStates.BridgeCollapsing:
                    BridgeEvents.BridgeCollapsingState?.Invoke();
                    BridgeTimer.ResetTimer();
                    ChangeState(BridgeStates.AnimationBridgeCollapsing);
                    break;

                case BridgeStates.AnimationBridgeCollapsing:
                    BridgeEvents.AnimatingBridgeCollapsingState?.Invoke();
                    break;

                case BridgeStates.GameFailed:

                    HandleGameWinOrLose(won: false);
                    if (_inSessionMode) {
                        TrialEnded();
                    }

                    break;

                case BridgeStates.BridgeCompleting:
                    BridgeEvents.BridgeCompletingState?.Invoke();
                    BridgeTimer.ResetTimer();
                    ChangeState(BridgeStates.AnimationBridgeCompleting);
                    break;

                case BridgeStates.AnimationBridgeCompleting:
                    BridgeEvents.AnimatingBridgeCompletingState?.Invoke();
                    break;

                case BridgeStates.BridgeCompleted:
                    if (_inSessionMode) {
                        TrialEnded();
                    }

                    BridgeEvents.BridgeIsCompletedState?.Invoke();
                    break;

                case BridgeStates.GameWon:
                    Debug.Log("GameWon State Called");
                    HandleGameWinOrLose(won: true);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void TrialEnded() {
            BridgeEvents.PrepareBridgeConfigs?.Invoke();
            OnBuildBridgeAction();
        }


        private void HandleGameWinOrLose(bool won) {
            if (won) {
                BridgeEvents.GameWonState?.Invoke();
            }
            else {
                BridgeEvents.GameFailedState?.Invoke();
            }
        }

        internal IEnumerator StartingGame() {
            // Wait for 3 seconds before starting the game (for the player to see the bridge, and for Animation to start), and update the UI with the countdown
            for (int i = 3; i >= 0; i--) {
                // Play Animation (countdown on Screen)
                BridgeEvents.CountDown?.Invoke(i);
                yield return new WaitForSecondsRealtime(0.2f);
            }

            StartCoroutine(BridgeTimer.StartTimer()); // Start the timer with the configured duration
            BridgeEvents.InGameState?.Invoke();
        }

        private void StartBuilding() {
            if (currentState is not (BridgeStates.Idle or BridgeStates.GameFailed
                or BridgeStates.BridgeCompleted)) {
                Debug.LogWarning(
                    "BridgeStateMachine :: StartBuilding called while the game is not in the correct state. currentState: " +
                    currentState);
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
                if (currentState is BridgeStates.InGame) {
                    ChangeState(BridgeStates.BridgeCompleting);
                }
            }
        }
    }
}