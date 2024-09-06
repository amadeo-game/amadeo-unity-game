using System.Collections;
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

        private BridgeStateMachine stateMachine;
        private UnitsControl _unitsControl;
        // private GameObject _bridgeHolder;
        private GameObject[] _playerUnits;
        private GameObject[] _guideUnits;
        private GameObject[] _totalBridgeUnits;

        private void OnEnable() {
            BridgeEvents.BuildingState += OnBuildingState;
            stateMachine.OnForceResetBridge += OnForceResetWithHeights;
            
            BridgeEvents.BridgeCollapsingState += OnBridgeCollapsingState;
            BridgeEvents.BridgeCompletingState += OnBridgeCompletingState;

            BridgeEvents.GameFailedState += OnDestroyBridge;
            BridgeEvents.ForceDestroyBridge += OnDestroyBridge;
        }
        
        private void OnDisable() {
            BridgeEvents.BuildingState -= OnBuildingState;
            stateMachine.OnForceResetBridge -= OnForceResetWithHeights;
            
            // TODO: Check This events
            BridgeEvents.BridgeCollapsingState -= OnBridgeCollapsingState;
            BridgeEvents.BridgeCompletingState -= OnBridgeCompletingState;
            
            BridgeEvents.GameFailedState -= OnDestroyBridge;
            BridgeEvents.ForceDestroyBridge -= OnDestroyBridge;
        }

        private void OnBridgeCompletingState() {
            CollectSessionData(success: true);
        }

        private void OnBridgeCollapsingState() {
            Bridge.DisableGuideUnits();
            CollectSessionData(success: false);
        }

        private void OnBuildingState() {
            // Destroy any existing bridge holder if it exists
            if (Bridge.BridgeHolder != null) {
                DestroyAllChildren(Bridge.BridgeHolder);
                Destroy(Bridge.BridgeHolder);
                Bridge.BridgeHolder = null;
                Debug.Log("BuildBridge: Existing bridgeHolder destroyed");
            }
            
            // Build the bridge
            var playerUnitsHeights = BridgeDataManager.Heights;
            Bridge.BuildBridgeWithHeights(playerUnitsHeights, bridgeRiseDownOffset);
            
            // Set the player units
            _unitsControl.SetPlayerUnits();
            
            // Fire the FinishedBuildingBridge event
            BridgeEvents.FinishedBuildingBridge?.Invoke();
        }

        private void CollectSessionData(bool success) {
            _unitsControl.CollectSessionData(success: success);
        }



        private void Awake() {
            stateMachine = GetComponent<BridgeStateMachine>();
            _unitsControl = GetComponent<UnitsControl>();

        }
        


        // private void BuildBridgeWithHeights() {
        //     BuildBridge();
        // }
        //
        // private void BuildBridge() {
        //     var playerUnitsHeights = BridgeDataManager.Heights;
        //     Debug.Log($"BuildBridge: bridgeHolder is {_bridgeHolder}");
        //
        //     // Destroy any existing bridge holder if it exists
        //     if (_bridgeHolder != null) {
        //         DestroyAllChildren(_bridgeHolder);
        //         Destroy(_bridgeHolder);
        //         _bridgeHolder = null;
        //         Debug.Log("BuildBridge: Existing bridgeHolder destroyed");
        //     }
        //
        //     // Instantiate a new bridge holder
        //     _bridgeHolder = new GameObject("Bridge Holder");
        //     Debug.Log("BuildBridge: New bridgeHolder instantiated");
        //
        //     var envUnits = GetBridgeEnvironmentHeights(playerUnitsHeights);
        //
        //     var playerUnitsPositions = GetBridgePlayerPositions(playerUnitsHeights);
        //     GameObject playerUnitPrefab = BridgeDataManager.BridgeType.GetPlayableUnitPrefab.PlayerUnit;
        //     _playerUnits = BuildPlayerUnits(_bridgeHolder, playerUnitsPositions, playerUnitPrefab, bridgeRiseDownOffset);
        //     
        //     // unitsControl.SetPlayerUnits(_playerUnits);
        //
        //     var envUnitsGameObjects =
        //         GenerateBridgeEnvironment(envUnits, BridgeDataManager.BridgeType, _bridgeHolder, bridgeRiseDownOffset);
        //
        //     GameObject guideUnit = BridgeDataManager.BridgeType.GetPlayableUnitPrefab.GuideUnit;
        //     _guideUnits = BuildGuideUnits(_bridgeHolder, playerUnitsPositions, guideUnit);
        //     
        //     _totalBridgeUnits = GetSequencedBridgeUnits(_playerUnits, envUnitsGameObjects);
        //
        //     // animationManager.AnimateBuildUpBridge(_totalBridgeUnits, bridgeRiseDownOffset);
        // }

        private void DestroyAllChildren(GameObject parent) {
            foreach (Transform child in parent.transform) {
                Destroy(child.gameObject);
            }
        }
        

        private void OnDestroyBridge() {
            if (Bridge.BridgeHolder != null) {
                DestroyAllChildren(Bridge.BridgeHolder);
                Destroy(Bridge.BridgeHolder);
                Bridge.BridgeHolder = null;
                Debug.Log("OnDestroyBridge: bridgeHolder destroyed");
            }
        }
        

        private void OnForceResetWithHeights(int[] unitHeights, BridgeTypeSO bridgeTypeSO = null) {
            
            OnDestroyBridge();
            Bridge.BuildBridgeWithHeights(BridgeDataManager.Heights, bridgeRiseDownOffset);
        }
    }
}