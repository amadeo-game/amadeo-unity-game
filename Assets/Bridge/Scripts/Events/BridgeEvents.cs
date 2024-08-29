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

        // Game Over
        public static Action GameFinishedState;


        public static Action<bool> EnableIsolatedControl;
        public static Action<bool> EnableMultiFingerControl;

        internal static Action<FingerUnit, bool> UnitPlacementStatusChanged;
        internal static Action<int, bool> ActiveUnitChanged;
        public static Action<bool[]> PlayableUnitsChanged;
        public static Action<float[]> GraceValuesChanged;
        public static Action<int[]> HeightValuesChanged;
        internal static Action<int, float> MvcExtensionUpdated;
        internal static Action<int, float> MvcFlexionUpdated;
        internal static Action<int, float> UnitGraceUpdated;

        public static Action<int> CountDown;
        
        // For adjusting difficulty
        public static Action PrepareBridgeConfigs;
        
        public static Action<BridgeData> OnBridgeDataUpdated;
        
        internal static Action ForceDestroyBridge;


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

        // ---- Game Over Return Events ----
        public static Action FinishedGameCompletedState;
        public static Action FinishedGameFailedState;


        // ---- Mid-Game Interaction Events ----

        // 
        public static Action PlayTrial;

        public static Action PlaySession;

        // Enable the guide units
        public static Action EnableGameInteraction;

        // called when the game is paused
        public static Action PauseGameAction;

        // called when the game is resumed
        public static Action ResumeGameAction;

        // Initiate collapsing the bridge
        public static Action CollapseBridgeAction;

        // Restart the game
        public static Action RestartGameAction;

        // Game Over Action
        public static Action GameFinishedAction;
        
        
        
    }
}