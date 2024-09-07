using System;

namespace BridgePackage {
    public static class GameEvents {
        // Triggered when the trial is failed
        public static Action TrialCompleting;

        // Triggered when the trial is completed
        public static Action TrialFailing;

        // The game has failed
        public static Action TrialFailed;

        // The game has been won
        public static Action TrialCompleted;


        // State events

        // Initial state of the bridge
        public static Action GameIdle;

        // Building the bridge
        public static Action GameBuilding;

        // Starting the zeroing process
        public static Action GameInZeroF;

        // Last state before the game starts, used to prepare the game (UI, animations, etc)
        public static Action GameStarting;

        // The game is in progress and the player can interact with the units
        public static Action GameIsRunning;

        // The game is paused
        public static Action GamePausedState;
        
        // Game Over
        public static Action SessionEnded;


        // For adjusting difficulty


        // ---- RETURN EVENTS ----


        // Zeroing is completed - ready to enable the playable units
        public static Action FinishedZeroF;

        // Finished building the bridge
        public static Action FinishedBuildingBridge;

        // FinishStartingGameProcess
        public static Action FinishStartingGameProcess;

        // ---- Animation return events ----
        // Finished animate building
        public static Action FinishedAnimatingBuildingState;

        // Finished animate collapsing
        public static Action FinishedAnimatingBridgeCollapsingState;

        // Finished animate completing
        public static Action FinishedAnimatingBridgeCompletingState;
    }
}