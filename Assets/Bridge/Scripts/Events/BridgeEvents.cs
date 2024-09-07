using System;
using BridgePackage;

namespace BridgePackage {
    internal static class BridgeEvents {
        internal static Action<float> OnTimeDurationChanged;

        // State events

        // Initial state of the bridge
        internal static Action IdleState; // *****

        // Building the bridge
        internal static Action BuildingState;

        // Animating the building of the bridge
        internal static Action AnimatingBuildingState;

        // Bridge is ready to be played
        internal static Action BridgeReadyState;

        // Starting the zeroing process
        internal static Action InZeroFState;

        // Last state before the game starts, used to prepare the game (UI, animations, etc)
        internal static Action StartingGameState;

        // The game is in progress and the player can interact with the units
        internal static Action InGameState;

        // The game is paused
        internal static Action GamePausedState;

        // The bridge is collapsing
        internal static Action BridgeCollapsingState;

        // Animate the bridge collapsing
        internal static Action AnimatingBridgeCollapsingState;

        // The game has failed
        internal static Action GameFailedState;

        // The bridge is completing
        internal static Action BridgeCompletingState;

        // Animate the bridge completing
        internal static Action AnimatingBridgeCompletingState;

        // The bridge is completed
        internal static Action BridgeIsCompletedState;

        // The game has been won
        internal static Action GameWonState;


        // For adjusting difficulty


        // ---- RETURN EVENTS ----


        // Zeroing is completed - ready to enable the playable units
        internal static Action FinishedZeroF;

        // Finished building the bridge
        internal static Action FinishedBuildingBridge;

        // FinishStartingGameProcess
        internal static Action FinishStartingGameProcess;

        // ---- Animation return events ----
        // Finished animate building
        internal static Action FinishedAnimatingBuildingState;

        // Finished animate collapsing
        internal static Action FinishedAnimatingBridgeCollapsingState;

        // Finished animate completing
        internal static Action FinishedAnimatingBridgeCompletingState;

        // ---- Game Over Return Events ----
        internal static Action FinishedGameCompletedState;
        internal static Action FinishedGameFailedState;

        
    }
}