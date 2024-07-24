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
        private UnitsControl unitsControl;
        private GameObject _bridgeHolder;
        private GameObject[] _playerUnits;
        private GameObject[] _guideUnits;
        private GameObject[] _totalBridgeUnits;

        private void OnEnable() {
            BridgeEvents.BridgeStateChanged += OnBridgeStateChanged;
            stateMachine.OnForceResetBridge += OnForceResetWithHeights;
        }

        private void OnBridgeStateChanged(BridgeStates state) {
            if (state is BridgeStates.Building) {
                BuildBridgeWithHeights();
            } 
            else if (state is BridgeStates.StartingGame) {
                EnableGuideUnits();
            }
            else if (state is BridgeStates.BridgeCollapsing) {
                CollectSessionData(success: false);
                StartCollapseAnimation();
            }
            else if (state is BridgeStates.BridgeCompleting) {
                CollectSessionData(success: true);
                AnimateSuccess();
            }
            else if (state is BridgeStates.GameFailed) {
                OnDestroyBridge();
            }
        }

        private void CollectSessionData(bool success) {
            unitsControl.CollectSessionData(success: success);
        }

        private void OnDisable() {
            BridgeEvents.BridgeStateChanged -= OnBridgeStateChanged;
            stateMachine.OnForceResetBridge -= OnForceResetWithHeights;
        }

        private void Awake() {
            _bridgeHolder = null;
            animationManager = GetComponent<BridgeAnimationManager>();
            stateMachine = GetComponent<BridgeStateMachine>();
            unitsControl = GetComponent<UnitsControl>();

        }

        private void Start() {
            _bridgeHolder = null;
        }

        private void EnableGuideUnits() {
            foreach (var guideUnit in _guideUnits) {
                guideUnit.SetActive(true);
            }
        }

        private void BuildBridgeWithHeights() {
            BuildBridge();
        }

        private void BuildBridge() {
            var playerUnitsHeights = BridgeDataManager.Heights;
            Debug.Log($"BuildBridge: bridgeHolder is {_bridgeHolder}");

            // Destroy any existing bridge holder if it exists
            if (_bridgeHolder != null) {
                DestroyAllChildren(_bridgeHolder);
                Destroy(_bridgeHolder);
                _bridgeHolder = null;
                Debug.Log("BuildBridge: Existing bridgeHolder destroyed");
            }

            // Instantiate a new bridge holder
            _bridgeHolder = new GameObject("Bridge Holder");
            Debug.Log("BuildBridge: New bridgeHolder instantiated");

            var envUnits = GetBridgeEnvironmentHeights(playerUnitsHeights);
            var envUnitsHeights = envUnits.Select(unit => unit.Position.y).ToArray();

            var playerUnitsPositions = GetBridgePlayerPositions(playerUnitsHeights);
            GameObject playerUnitPrefab = BridgeDataManager.BridgeType.GetPlayableUnitPrefab.PlayerUnit;
            _playerUnits = BuildPlayerUnits(_bridgeHolder, playerUnitsPositions, playerUnitPrefab, bridgeRiseDownOffset);
            
            unitsControl.SetPlayerUnits(_playerUnits);

            var envUnitsGameObjects =
                GenerateBridgeEnvironment(envUnits, BridgeDataManager.BridgeType, _bridgeHolder, bridgeRiseDownOffset);

            GameObject guideUnit = BridgeDataManager.BridgeType.GetPlayableUnitPrefab.GuideUnit;
            _guideUnits = BuildGuideUnits(_bridgeHolder, playerUnitsPositions, guideUnit);
            
            _totalBridgeUnits = GetSequencedBridgeUnits(_playerUnits, envUnitsGameObjects);

            animationManager.AnimateBuildUpBridge(_totalBridgeUnits, bridgeRiseDownOffset);
        }

        private void DestroyAllChildren(GameObject parent) {
            foreach (Transform child in parent.transform) {
                Destroy(child.gameObject);
            }
        }

        private void StartCollapseAnimation() {
            animationManager.AnimateBridgeFallDown(_totalBridgeUnits, bridgeRiseDownOffset);
        }

        private void OnDestroyBridge() {
            if (_bridgeHolder != null) {
                DestroyAllChildren(_bridgeHolder);
                Destroy(_bridgeHolder);
                _bridgeHolder = null;
                Debug.Log("OnDestroyBridge: bridgeHolder destroyed");
            }
        }

        private void AnimateSuccess() {
            animationManager.AnimateSuccess(_playerUnits, BridgeDataManager.Heights);
        }

        private void OnForceResetWithHeights(int[] unitHeights, BridgeTypeSO bridgeTypeSO = null) {
            
            OnDestroyBridge();
            BuildBridge();
        }
    }
}