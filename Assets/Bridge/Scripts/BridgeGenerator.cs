using System.Linq;
using UnityEngine;
using static BridgeBuilder;
using static BridgeObjects;

namespace BridgePackage {
    public enum FingerUnit {
        First,
        Second,
        Third,
        Fourth,
        Fifth,
    }

    [RequireComponent(typeof(UnitsControl), typeof(BridgeAnimationManager), typeof(BridgeStateMachine))]
    internal class BridgeGenerator : MonoBehaviour {
        private const int NumBridgeUnits = 5;

        [SerializeField, Tooltip("x-axis value from where the bridge will rise"), Min(0)]
        private int bridgeRiseDownOffset = 12;

        private BridgeAnimationManager animationManager;
        private BridgeStateMachine stateMachine;
        private GameObject bridgeHolder;
        private GameObject[] playerUnits;
        private GameObject[] totalBridgeUnits;

        private void OnEnable() {
            BridgeEvents.BridgeStateChanged += OnBridgeStateChanged;
            stateMachine.OnForceResetBridge += OnForceResetWithHeights;
        }

        private void OnBridgeStateChanged(BridgeStates state) {
            if (state is BridgeStates.Building) {
                BuildBridgeWithHeights();
            }

            if (state is BridgeStates.InGame) {
                EnableUnitsControl();
            }
            else if (state is BridgeStates.BridgeCollapsing) {
                StartCollapseAnimation();
            }
            else if (state is BridgeStates.BridgeCompleting) {
                AnimateSuccess();
            }
            else if (state is BridgeStates.GameFailed) {
                OnDestroyBridge();
            }
        }

        private void OnDisable() {
            BridgeEvents.BridgeStateChanged -= OnBridgeStateChanged;
            stateMachine.OnForceResetBridge -= OnForceResetWithHeights;
        }

        private void Awake() {
            bridgeHolder = null;
            animationManager = GetComponent<BridgeAnimationManager>();
            stateMachine = GetComponent<BridgeStateMachine>();
        }

        private void Start() {
            bridgeHolder = null;
        }

        private void EnableUnitsControl() {
            Debug.Log("EnableUnitsControl: Enabling units control");
            var unitsControl = GetComponent<UnitsControl>();
            unitsControl.SetPlayerUnits(playerUnits);
        }

        private void BuildBridgeWithHeights() {
            BuildBridge();
        }

        private void BuildBridge() {
            var playerUnitsHeights = BridgeDataManager.Heights;
            Debug.Log($"BuildBridge: bridgeHolder is {bridgeHolder}");

            // Destroy any existing bridge holder if it exists
            if (bridgeHolder != null) {
                DestroyAllChildren(bridgeHolder);
                Destroy(bridgeHolder);
                bridgeHolder = null;
                Debug.Log("BuildBridge: Existing bridgeHolder destroyed");
            }

            // Instantiate a new bridge holder
            bridgeHolder = new GameObject("Bridge Holder");
            Debug.Log("BuildBridge: New bridgeHolder instantiated");

            var envUnits = GetBridgeEnvironmentHeights(playerUnitsHeights);
            var envUnitsHeights = envUnits.Select(unit => unit.Position.y).ToArray();

            var playerUnitsPositions = GetBridgePlayerPositions(playerUnitsHeights);
            GameObject playerUnitPrefab = BridgeDataManager.BridgeType.GetPlayableUnitPrefab.PlayerUnit;
            playerUnits = BuildPlayerUnits(bridgeHolder, playerUnitsPositions, playerUnitPrefab, bridgeRiseDownOffset);

            var envUnitsGameObjects =
                GenerateBridgeEnvironment(envUnits, BridgeDataManager.BridgeType, bridgeHolder, bridgeRiseDownOffset);

            GameObject guideUnit = BridgeDataManager.BridgeType.GetPlayableUnitPrefab.GuideUnit;
            BuildGuideUnits(bridgeHolder, playerUnitsPositions, guideUnit);

            totalBridgeUnits = GetSequencedBridgeUnits(playerUnits, envUnitsGameObjects);

            animationManager.AnimateBuildUpBridge(totalBridgeUnits, bridgeRiseDownOffset);
        }

        private void DestroyAllChildren(GameObject parent) {
            foreach (Transform child in parent.transform) {
                Destroy(child.gameObject);
            }
        }

        private void StartCollapseAnimation() {
            animationManager.AnimateBridgeFallDown(totalBridgeUnits, bridgeRiseDownOffset);
        }

        private void OnDestroyBridge() {
            if (bridgeHolder != null) {
                DestroyAllChildren(bridgeHolder);
                Destroy(bridgeHolder);
                bridgeHolder = null;
                Debug.Log("OnDestroyBridge: bridgeHolder destroyed");
            }
        }

        private void AnimateSuccess() {
            animationManager.AnimateSuccess(playerUnits, BridgeDataManager.Heights);
        }

        private void OnForceResetWithHeights(int[] unitHeights, BridgeTypeSO bridgeTypeSO = null) {
            
            OnDestroyBridge();
            BuildBridge();
        }
    }
}