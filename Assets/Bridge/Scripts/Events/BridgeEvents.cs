using System;
using BridgePackage;

namespace BridgePackage {
    public static class BridgeEvents {
        // Apply the specified forces to the player units
        public static Action<float[]> ForcesUpdated;
        public static Action<float> OnTimeDurationChanged;

        // State events

        // Initial state of the bridge
        public static Action IdleState;

        // Building the bridge
        public static Action BuildingState;

        // Animating the building of the bridge
        public static Action AnimatingBuildingState;
        
        // Bridge is ready to be played
        public static Action BridgeReadyState;

        // Starting the zeroing process
        public static Action InZeroFState;

        // Last state before the game starts, used to prepare the game (UI, animations, etc)
        public static Action StartingGameState;
        
        // The game is in progress and the player can interact with the units
        public static Action InGameState;
        
        // The game is paused
        public static Action GamePausedState;

        // The bridge is collapsing
        public static Action BridgeCollapsingState;

        // Animate the bridge collapsing
        public static Action AnimatingBridgeCollapsingState;

        // The game has failed
        public static Action GameFailedState;

        // The bridge is completing
        public static Action BridgeCompletingState;
        
        // Animate the bridge completing
        public static Action AnimatingBridgeCompletingState;
        
        // The bridge is completed
        public static Action BridgeIsCompletedState;

        // The game has been won
        public static Action GameWonState;


        
        internal static Action<FingerUnit, bool> UnitPlacementStatusChanged;
        internal static Action<int, bool> ActiveUnitChanged;
        internal static Action<int, float> MvcExtensionUpdated;
        internal static Action<int, float> MvcFlexionUpdated;
        internal static Action<int, float> UnitGraceUpdated;

        
        public static Action<BridgeData> OnBridgeDataUpdated;

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

        
        // ---- Mid-Game Interaction Events ----
        
        // Enable the guide units
        public static Action EnableGameInteraction;
        
        // called when the game is paused
        public static Action PauseGameAction;

        // called when the game is resumed
        public static Action ResumeGameAction;

        // Initiate collapsing the bridge
        public static Action CollapseBridgeAction;
    }
}