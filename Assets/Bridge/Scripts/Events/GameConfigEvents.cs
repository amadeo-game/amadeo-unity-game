using System;

namespace BridgePackage {
    public static class GameConfigEvents {
        public static Action<bool> EnableIsolatedControl;
        public static Action<bool> EnableMultiFingerControl;
        public static Action<FingerUnit, bool> UnitPlacementStatusChanged;
        public static Action<int, bool> ActiveUnitChanged;
        public static Action<bool[]> PlayableUnitsChanged;
        public static Action<float[]> GraceValuesChanged;
        public static Action<int[]> HeightValuesChanged;
        public static Action<int, float> MvcExtensionUpdated;
        public static Action<int, float> MvcFlexionUpdated;
        public static Action<int, float> UnitGraceUpdated;
        public static Action<int> CountDown;
        public static Action PrepareBridgeConfigs;
        public static Action<BridgeData> OnBridgeDataUpdated;
    }
}