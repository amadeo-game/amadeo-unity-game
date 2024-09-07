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
            GameConfigEvents.UnitPlacementStatusChanged += UnitPlaced;

            // Listen to all returned events
            BridgeEvents.FinishedBuildingBridge += () => ChangeState(BridgeStates.AnimationBuilding);
            BridgeEvents.FinishedAnimatingBuildingState += () => ChangeState(BridgeStates.BridgeReady);

            BridgeEvents.FinishedZeroF += () => ChangeState(BridgeStates.StartingGame);
            BridgeEvents.FinishStartingGameProcess += () => ChangeState(BridgeStates.InGame);


            // --- Action Events ---
            GameActions.PlayTrial += OnBuildBridgeAction;
            GameActions.PlaySession += StartSession;
            GameActions.EnableGameInteraction += PrepareAndStartGame;
            GameActions.PauseGameAction += () => {
                _isPaused = true;
                ChangeState(BridgeStates.Paused);
            };
            GameActions.ResumeGameAction += () => { ChangeState(BridgeStates.InGame); };
            GameActions.ForceDestroyBridge += ForceCollapseBridge;

            GameActions.RestartGameAction += () => { ChangeState(BridgeStates.Idle); };

            GameActions.GameFinishedAction += OnGameFinishedAction;


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
            DynamicDifficulty._firstTrial = true;
            GameActions.ForceDestroyBridge?.Invoke();
            // ChangeState(BridgeStates.Idle);
            GameEvents.SessionEnded?.Invoke();
        }

        private void OnDisable() {
            BridgeTimer.OnTimerComplete -= HandleTimerComplete;
            GameConfigEvents.UnitPlacementStatusChanged -= UnitPlaced;

            BridgeEvents.FinishedBuildingBridge -= () => ChangeState(BridgeStates.AnimationBuilding);
            BridgeEvents.FinishedAnimatingBuildingState -= () =>
                ChangeState(BridgeStates.BridgeReady);
            ;

            BridgeEvents.FinishedZeroF -= () => ChangeState(BridgeStates.StartingGame);
            BridgeEvents.FinishStartingGameProcess -= () => ChangeState(BridgeStates.InGame);

            // --- Action Events ---
            GameActions.PlayTrial -= OnBuildBridgeAction;
            GameActions.PlaySession -= StartSession;
            GameActions.EnableGameInteraction -= PrepareAndStartGame;
            GameActions.PauseGameAction -= () => ChangeState(BridgeStates.Paused);
            GameActions.ResumeGameAction -= () => { ChangeState(BridgeStates.InGame); };

            GameActions.ForceDestroyBridge -= ForceCollapseBridge;

            GameActions.RestartGameAction -= () => { ChangeState(BridgeStates.Idle); };

            GameActions.GameFinishedAction -= OnGameFinishedAction;

            // --- Animation Events ---
            BridgeEvents.FinishedAnimatingBridgeCollapsingState -= () => ChangeState(BridgeStates.GameFailed);
            BridgeEvents.FinishedAnimatingBridgeCompletingState -= () => ChangeState(BridgeStates.BridgeCompleted);
            BridgeEvents.FinishedGameCompletedState -= () => ChangeState(BridgeStates.GameWon);
        }

        private void StartSession() {
            _inSessionMode = true;
            GameConfigEvents.PrepareBridgeConfigs?.Invoke();
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
                ChangeState(BridgeStates.BridgeCollapsing);
            }
        }

        // private void ChangeState(BridgeStates state) {
        //     if (_debugMode) {
        //         Debug.Log("BridgeStateMachine :: Changing state from " + currentState + " to " + state);
        //     }
        //
        //     currentState = state;
        //     switch (state) {
        //         case BridgeStates.Idle:
        //             BridgeEvents.IdleState?.Invoke();
        //             GameEvents.GameIdle?.Invoke();
        //             break;
        //
        //         case BridgeStates.Building:
        //             BridgeEvents.BuildingState?.Invoke();
        //             GameEvents.GameBuilding?.Invoke();
        //
        //             break;
        //
        //         case BridgeStates.AnimationBuilding:
        //             BridgeEvents.AnimatingBuildingState?.Invoke();
        //             break;
        //
        //         case BridgeStates.BridgeReady:
        //             BridgeEvents.BridgeReadyState?.Invoke();
        //             // Change this if you don't want to start the game automatically
        //             PrepareAndStartGame();
        //             break;
        //
        //         case BridgeStates.InZeroF:
        //             BridgeEvents.InZeroFState?.Invoke();
        //
        //             break;
        //
        //         case BridgeStates.StartingGame:
        //             BridgeEvents.StartingGameState?.Invoke();
        //             GameEvents.GameStarting?.Invoke();
        //             break;
        //
        //         case BridgeStates.InGame:
        //             StartCoroutine(StartingGame());
        //             if (!_isPaused) {
        //                 SetPlayerUnitsDictionary();
        //             }
        //
        //             _isPaused = false;
        //             break;
        //
        //         case BridgeStates.Paused:
        //             _isPaused = true;
        //             BridgeEvents.GamePausedState?.Invoke();
        //             BridgeTimer.PauseTimer();
        //             break;
        //
        //         case BridgeStates.BridgeCollapsing:
        //             BridgeEvents.BridgeCollapsingState?.Invoke();
        //             GameEvents.TrialFailing?.Invoke();
        //             BridgeTimer.ResetTimer();
        //             ChangeState(BridgeStates.AnimationBridgeCollapsing);
        //             break;
        //
        //         case BridgeStates.AnimationBridgeCollapsing:
        //             BridgeEvents.AnimatingBridgeCollapsingState?.Invoke();
        //             break;
        //
        //         case BridgeStates.GameFailed:
        //
        //             HandleGameWinOrLose(won: false);
        //             if (_inSessionMode) {
        //                 TrialEnded();
        //             }
        //
        //             if (DynamicDifficulty._firstTrial) {
        //                 ChangeState(BridgeStates.Idle);
        //             }
        //
        //             break;
        //
        //         case BridgeStates.BridgeCompleting:
        //             BridgeEvents.BridgeCompletingState?.Invoke();
        //             GameEvents.TrialCompleting?.Invoke();
        //             BridgeTimer.ResetTimer();
        //             ChangeState(BridgeStates.AnimationBridgeCompleting);
        //             break;
        //
        //         case BridgeStates.AnimationBridgeCompleting:
        //             BridgeEvents.AnimatingBridgeCompletingState?.Invoke();
        //             break;
        //
        //         case BridgeStates.BridgeCompleted:
        //             if (_inSessionMode) {
        //                 TrialEnded();
        //             }
        //             else {
        //                 ChangeState(BridgeStates.GameWon);
        //             }
        //
        //             break;
        //
        //         case BridgeStates.GameWon:
        //             Debug.Log("GameWon State Called");
        //             HandleGameWinOrLose(won: true);
        //             break;
        //
        //         default:
        //             throw new ArgumentOutOfRangeException(nameof(state), state, null);
        //     }
        // }

        private void ChangeState(BridgeStates state) {
            if (_debugMode) {
                Debug.Log($"BridgeStateMachine :: Changing state from {currentState} to {state}");
            }

            currentState = state;
            HandleStateAction(state);
        }

        private void HandleStateAction(BridgeStates state) {
            switch (state) {
                case BridgeStates.Idle:
                    HandleIdleState();
                    break;
                case BridgeStates.Building:
                    HandleBuildingState();
                    break;
                case BridgeStates.AnimationBuilding:
                    BridgeEvents.AnimatingBuildingState?.Invoke();
                    break;
                case BridgeStates.BridgeReady:
                    BridgeEvents.BridgeReadyState?.Invoke();
                    PrepareAndStartGame();
                    break;
                case BridgeStates.InZeroF:
                    BridgeEvents.InZeroFState?.Invoke();
                    break;
                case BridgeStates.StartingGame:
                    BridgeEvents.StartingGameState?.Invoke();
                    GameEvents.GameStarting?.Invoke();
                    break;
                case BridgeStates.InGame:
                    HandleInGameState();
                    break;
                case BridgeStates.Paused:
                    HandlePausedState();
                    break;
                case BridgeStates.BridgeCollapsing:
                    HandleBridgeCollapsingState();
                    break;
                case BridgeStates.AnimationBridgeCollapsing:
                    BridgeEvents.AnimatingBridgeCollapsingState?.Invoke();
                    break;
                case BridgeStates.GameFailed:
                    HandleGameFailedState();
                    break;
                case BridgeStates.BridgeCompleting:
                    HandleBridgeCompletingState();
                    break;
                case BridgeStates.AnimationBridgeCompleting:
                    BridgeEvents.AnimatingBridgeCompletingState?.Invoke();
                    break;
                case BridgeStates.BridgeCompleted:
                    HandleBridgeCompletedState();
                    break;
                case BridgeStates.GameWon:
                    HandleGameWonState();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state,
                        "Unhandled state in BridgeStateMachine");
            }
        }

        private void HandleIdleState() {
            BridgeEvents.IdleState?.Invoke();
            GameEvents.GameIdle?.Invoke();
        }

        private void HandleBuildingState() {
            BridgeEvents.BuildingState?.Invoke();
            GameEvents.GameBuilding?.Invoke();
        }

        private void HandleInGameState() {
            StartCoroutine(StartingGame());
            if (!_isPaused) {
                SetPlayerUnitsDictionary();
            }

            _isPaused = false;
        }

        private void HandlePausedState() {
            _isPaused = true;
            BridgeEvents.GamePausedState?.Invoke();
            BridgeTimer.PauseTimer();
        }

        private void HandleBridgeCollapsingState() {
            BridgeEvents.BridgeCollapsingState?.Invoke();
            GameEvents.TrialFailing?.Invoke();
            BridgeTimer.ResetTimer();
            ChangeState(BridgeStates.AnimationBridgeCollapsing);
        }

        private void HandleGameFailedState() {
            HandleGameWinOrLose(won: false);
            if (_inSessionMode) {
                TrialEnded();
            }

            if (DynamicDifficulty._firstTrial) {
                ChangeState(BridgeStates.Idle);
            }
        }

        private void HandleBridgeCompletingState() {
            BridgeEvents.BridgeCompletingState?.Invoke();
            GameEvents.TrialCompleting?.Invoke();
            BridgeTimer.ResetTimer();
            ChangeState(BridgeStates.AnimationBridgeCompleting);
        }

        private void HandleBridgeCompletedState() {
            if (_inSessionMode) {
                TrialEnded();
            }
            else {
                ChangeState(BridgeStates.GameWon);
            }
        }

        private void HandleGameWonState() {
            Debug.Log("GameWon State Called");
            HandleGameWinOrLose(won: true);
        }


        private void PrepareAndStartGame() {
            if (currentState is BridgeStates.BridgeReady) {
                ChangeState(BridgeDataManager.ZeroF ? BridgeStates.InZeroF : BridgeStates.StartingGame);
            }
        }

        private void TrialEnded() {
            GameConfigEvents.PrepareBridgeConfigs?.Invoke();
            if (!DynamicDifficulty._firstTrial) {
                OnBuildBridgeAction();
            }
        }


        private void HandleGameWinOrLose(bool won) {
            if (won) {
                BridgeEvents.GameWonState?.Invoke();
                GameEvents.TrialCompleted?.Invoke();
            }
            else {
                BridgeEvents.GameFailedState?.Invoke();
                GameEvents.TrialFailed?.Invoke();
            }

            ChangeState(BridgeStates.Idle);
        }

        internal IEnumerator StartingGame() {
            // Wait for 3 seconds before starting the game (for the player to see the bridge, and for Animation to start), and update the UI with the countdown
            for (int i = 3; i >= 0; i--) {
                // Play Animation (countdown on Screen)
                GameConfigEvents.CountDown?.Invoke(i);
                yield return new WaitForSecondsRealtime(0.2f);
            }

            StartCoroutine(BridgeTimer.StartTimer()); // Start the timer with the configured duration
            BridgeEvents.InGameState?.Invoke();
            GameEvents.GameIsRunning?.Invoke();
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