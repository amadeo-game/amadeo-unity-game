using System;
using BridgePackage;

namespace BridgePackage {
    public static class BridgeEvents {
        // Apply the specified forces to the player units
        public static Action<float[]> ForcesUpdated;
        public static Action<float> OnTimeDurationChanged;

        
        public static Action<BridgeStates> BridgeStateChanged;
        internal static Action<FingerUnit, bool> UnitPlacementStatusChanged;
        internal static Action<int, bool> ActiveUnitChanged;
        internal static Action<int, float> MvcExtensionUpdated;
        internal static Action<int, float> MvcFlexionUpdated;
        internal static Action<int, float> UnitGraceUpdated;

        public static Action BridgeReady;
        public static Action OnGameStart;
        public static Action ZeroingCompleted;
        public static Action BridgeCollapsed;
        public static Action FailedSession;
        public static Action BridgeIsComplete;
        public static Action WonSession;

        public static Action<BridgeData> OnBridgeDataUpdated;
    }
}