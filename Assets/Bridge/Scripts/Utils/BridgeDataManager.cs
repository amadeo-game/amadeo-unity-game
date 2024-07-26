using UnityEngine;

namespace BridgePackage {
    public class BridgeDataManager : MonoBehaviour {
        private static BridgeData BridgeData = new BridgeData();
        [SerializeField] BridgeCollectionSO bridgeCollection;

        private void Start() {
            if (this.bridgeCollection == null) {
                Debug.LogError("BridgeCollectionSO is not assigned in BridgeDataManager, cannot proceed.");
            }

            SetBridgeCollection(bridgeCollection);
        }

        void OnEnable() {
            BridgeEvents.OnBridgeDataUpdated += OnBridgeDataUpdated;
        }

        private static void OnBridgeDataUpdated(BridgeData bridgeData) {
            BridgeData = bridgeData;
        }

        void OnDisable() {
            BridgeEvents.OnBridgeDataUpdated -= OnBridgeDataUpdated;
        }

        public static int[] Heights {
            get {
                // Debug.Log("BridgeDataManager :: Before Heights accessed " + string.Join(",", BridgeData.heights));
                if (BridgeData.isFlexion) {
                    for (int i = 0; i < BridgeData.heights.Length; i++) {
                        var height = BridgeData.heights[i];
                        BridgeData.heights[i] = Mathf.Min(height, -height);
                    }
                    // Debug.Log("BridgeDataManager :: AFTER Heights accessed " + string.Join(",", BridgeData.heights));
                }

                return BridgeData.heights;
            }
        }

        public static BridgeTypeSO BridgeType => BridgeData.bridgeCollection.BridgeTypes[BridgeData.level];
        public static int Level => BridgeData.level;
        public static bool IsLeftHand => BridgeData.isLeftHand;
        public static bool IsFlexion => BridgeData.isFlexion;
        public static float[] MvcValuesExtension => BridgeData.mvcValuesExtension;
        public static float[] MvcValuesFlexion => BridgeData.mvcValuesFlexion;
        public static bool[] PlayableUnits => BridgeData.playableUnits;
        public static float[] UnitsGrace => BridgeData.unitsGrace;
        public static float TimeDuration => BridgeData.TimeDuration;
        public static bool ZeroF => BridgeData.zeroF;
        public static bool AutoStart => BridgeData.autoStart;
        public static SessionData SessionData => BridgeData.SessionData;

        // write all the setters too

        public static void SetHeights(int[] newHeights) {
            if (newHeights.Length == 5) {
                BridgeData.heights = newHeights;
            }
        }

        public static void SetLevel(int newLevel) {
            BridgeData.level = newLevel;
        }

        public static void SetBridgeCollection(BridgeCollectionSO newBridgeCollection) {
            BridgeData.bridgeCollection = newBridgeCollection;
        }

        public static void SetIsLeftHand(bool leftHand) {
            BridgeData.isLeftHand = leftHand;
        }

        public static void SetIsFlexion(bool flexion) {
            BridgeData.isFlexion = flexion;
        }

        public static void SetMvcValuesExtension(float[] newMvcValues) {
            if (newMvcValues.Length == 5) {
                BridgeData.mvcValuesExtension = newMvcValues;
            }
        }

        public static void SetMvcValuesFlexion(float[] newMvcValues) {
            if (newMvcValues.Length == 5) {
                BridgeData.mvcValuesFlexion = newMvcValues;
            }
        }

        public static void SetMvcValuesExtension(int index, float value) {
            if (index < 0 || index > 4) {
                Debug.LogWarning("Index must be between 0 and 4.");
                return;
            }

            BridgeData.mvcValuesExtension[index] = value;
            BridgeEvents.MvcExtensionUpdated?.Invoke(index, value);
        }

        public static void SetMvcValuesFlexion(int index, float value) {
            if (index < 0 || index > 4) {
                Debug.LogWarning("Index must be between 0 and 4.");
                return;
            }

            BridgeData.mvcValuesFlexion[index] = value;
            BridgeEvents.MvcFlexionUpdated?.Invoke(index, value);
        }

        public static void SetPlayableUnits(bool[] newPlayableUnits) {
            if (newPlayableUnits.Length == 5) {
                BridgeData.playableUnits = newPlayableUnits;
            }
        }

        public static void SetPlayableUnit(int index, bool value) {
            BridgeData.playableUnits[index] = value;
            BridgeEvents.ActiveUnitChanged?.Invoke(index, value);
        }

        public static void SetTimeDuration(float newTimeDuration) {
            BridgeData.TimeDuration = newTimeDuration;
        }

        public static void SetUnitsGrace(float[] floats) {
            if (floats.Length == 5) {
                BridgeData.unitsGrace = floats;
            }
        }

        public static void SetZeroF(bool newZeroF) {
            BridgeData.zeroF = newZeroF;
        }

        public static void SetAutoStart(bool newAutoPlay) {
            BridgeData.autoStart = newAutoPlay;
        }

        public static void SetSessionData(float[] bestHeights, bool isSuccessful) {
            BridgeData.SessionData.heights = BridgeData.heights;
            BridgeData.SessionData.BestYPositions = bestHeights;
            BridgeData.SessionData.success = isSuccessful;
        }
    }
}