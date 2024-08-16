using UnityEngine;

using static BridgeBuilder;
using static BridgeObjects;

namespace BridgePackage {
    public static class Bridge {
        internal static GameObject BridgeHolder;
        internal static GameObject[] PlayerUnits;
        internal static GameObject[] GuideUnits;
        internal static int BridgeRiseDownOffset = 12;
        internal static GameObject[] TotalBridgeUnits;
        internal static int[] PlayerUnitsHeights;

        internal static void BuildBridgeWithHeights(int[] playerUnitsHeights, int bridgeRiseDownOffset = 12) {
            // Build bridge with heights
            Bridge.BridgeRiseDownOffset = bridgeRiseDownOffset;
            Bridge.PlayerUnitsHeights = playerUnitsHeights;
            BuildBridge(playerUnitsHeights);
            Debug.Log("Bridge: Bridge built successfully");
        }

        public static void EnableGuideUnits() {
            // Enable guide units
            foreach (var guideUnit in GuideUnits) {
                guideUnit.SetActive(true);
            }
        }
        
        public static void DisableGuideUnits() {
            // Disable guide units
            foreach (var guideUnit in GuideUnits) {
                guideUnit.SetActive(false);
            }
        }

        public static void CollectSessionData(bool success) {
            // Collect session data
        }

        public static void StartCollapseAnimation() {
            // Start collapse animation
        }

        public static void AnimateSuccess() {
            // Animate success
        }

        public static void OnDestroyBridge() {
            // Destroy bridge
        }
        
        private static void BuildBridge(int[] playerUnitsHeights) {
            
            Debug.Log($"BuildBridge: bridgeHolder is {BridgeHolder}");



            // Instantiate a new bridge holder
            BridgeHolder = new GameObject("Bridge Holder");
            Debug.Log("BuildBridge: New bridgeHolder instantiated");

            var envUnits = GetBridgeEnvironmentHeights(playerUnitsHeights);

            var playerUnitsPositions = GetBridgePlayerPositions(playerUnitsHeights);
            GameObject playerUnitPrefab = BridgeDataManager.BridgeType.GetPlayableUnitPrefab.PlayerUnit;
            PlayerUnits = BuildPlayerUnits(BridgeHolder, playerUnitsPositions, playerUnitPrefab, BridgeRiseDownOffset);
            
            

            var envUnitsGameObjects =
                GenerateBridgeEnvironment(envUnits, BridgeDataManager.BridgeType, BridgeHolder, BridgeRiseDownOffset);

            GameObject guideUnit = BridgeDataManager.BridgeType.GetPlayableUnitPrefab.GuideUnit;
            GuideUnits = BuildGuideUnits(BridgeHolder, playerUnitsPositions, guideUnit);
            
            TotalBridgeUnits = GetSequencedBridgeUnits(PlayerUnits, envUnitsGameObjects);

        }
    }
}