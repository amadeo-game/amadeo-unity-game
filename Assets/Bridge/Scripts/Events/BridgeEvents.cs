using System;
using BridgePackage;

namespace BridgePackage {
    public static class BridgeEvents {
        // Apply the specified forces to the player units
        public static Action<float[]> ForcesUpdated;

        // // Pass updated copy of the game data from the UI to the Controller
        // public static Action<GameData> UIGameDataUpdated;
        //
        // // Send updated data from the controller to listeners (e.g. GameDataManager, AudioManager, etc.)
        // public static Action<GameData> SettingsUpdated;
        public static Action<BridgeStates> BridgeStateChanged;
        public static Action BridgeReady;
        public static Action OnGameStart;
        public static Action BridgeCollapsed;
        public static Action FailedSession;
        public static Action BridgeIsComplete;
        public static Action WonSession;
        
        public static Action<BridgeData> OnBridgeDataUpdated;

    }
}