using System.Linq;
using UnityEngine;
using static BridgeBuilder;
using static BridgeObjects;

namespace BridgePackage
{
    public enum FingerUnit
    {
        first,
        second,
        third,
        fourth,
        fifth,
    }

    [RequireComponent(typeof(UnitsControl), typeof(BridgeAnimationManager), typeof(BridgeStateMachine))]
    internal class BridgeGenerator : MonoBehaviour
    {
        private const int NumBridgeUnits = 5;

        [SerializeField, Range(0, 5)]
        private int[] playerUnitsHeights = new int[NumBridgeUnits];
        
        [SerializeField] private BridgeTypeSO bridgeTypeSO;

        [SerializeField, Tooltip("x-axis value from where the bridge will rise"), Min(0)]
        private int bridgeRiseDownOffset = 12;

        private BridgeAnimationManager animationManager;
        private BridgeStateMachine stateMachine;
        private GameObject bridgeHolder;
        private GameObject[] playerUnits;
        private GameObject[] totalBridgeUnits;

        private void OnEnable()
        {
            stateMachine.OnBuildStartWithHeights += BuildBridgeWithHeights;
            stateMachine.OnEnablePlayerUnits += EnableUnitsControl;
            stateMachine.OnCollapseStart += OnBridgeFailed;
            stateMachine.OnSuccessStart += AnimateSuccess;
            stateMachine.OnForceResetBridge += OnForceResetWithHeights;
        }

        private void OnDisable()
        {
            stateMachine.OnBuildStartWithHeights -= BuildBridgeWithHeights;
            stateMachine.OnEnablePlayerUnits -= EnableUnitsControl;
            stateMachine.OnCollapseStart -= OnBridgeFailed;
            stateMachine.OnSuccessStart -= AnimateSuccess;
            stateMachine.OnForceResetBridge -= OnForceResetWithHeights;
        }

        private void Awake()
        {
            bridgeHolder = null;
            animationManager = GetComponent<BridgeAnimationManager>();
            stateMachine = GetComponent<BridgeStateMachine>();
        }

        private void Start()
        {
            bridgeHolder = null;
        }

        internal void SetPlayerHeights(int[] unitsHeights)
        {
            playerUnitsHeights = unitsHeights;
        }

        private void EnableUnitsControl()
        {
            var unitsControl = GetComponent<UnitsControl>();
            unitsControl.SetPlayerUnits(playerUnits);
        }

        private void BuildBridgeWithHeights(int[] unitsHeights, BridgeTypeSO bridgeTypeSO = null)
        {
            if (bridgeTypeSO != null)
            {
                this.bridgeTypeSO = bridgeTypeSO;
            }

            SetPlayerHeights(unitsHeights);
            BuildBridge();
        }

        private void BuildBridge()
        {
            Debug.Log($"BuildBridge: bridgeHolder is {bridgeHolder}");

            // Destroy any existing bridge holder if it exists
            if (bridgeHolder != null)
            {
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
            GameObject playerUnitPrefab = bridgeTypeSO.GetPlayableUnitPrefab.PlayerUnit;
            playerUnits = BuildPlayerUnits(bridgeHolder, playerUnitsPositions, playerUnitPrefab, bridgeRiseDownOffset);

            var envUnitsGameObjects = GenerateBridgeEnvironment(envUnits, bridgeTypeSO, bridgeHolder, bridgeRiseDownOffset);

            GameObject guideUnit = bridgeTypeSO.GetPlayableUnitPrefab.GuideUnit;
            BuildGuideUnits(bridgeHolder, playerUnitsPositions, guideUnit);

            totalBridgeUnits = GetSequencedBridgeUnits(playerUnits, envUnitsGameObjects);

            animationManager.AnimateBuildUpBridge(totalBridgeUnits, bridgeRiseDownOffset);
        }

        private void DestroyAllChildren(GameObject parent)
        {
            foreach (Transform child in parent.transform)
            {
                Destroy(child.gameObject);
            }
        }

        private void OnBridgeFailed()
        {
            animationManager.AnimateBridgeFallDown(totalBridgeUnits, bridgeRiseDownOffset);
        }

        private void OnDestroyBridge()
        {
            if (bridgeHolder != null)
            {
                DestroyAllChildren(bridgeHolder);
                Destroy(bridgeHolder);
                bridgeHolder = null;
                Debug.Log("OnDestroyBridge: bridgeHolder destroyed");
            }
        }

        private void AnimateSuccess()
        {
            animationManager.AnimateSuccess(playerUnits, playerUnitsHeights);
        }

        private void OnForceResetWithHeights(int[] unitHeights, BridgeTypeSO bridgeTypeSO = null)
        {
            DisableUnitsControl();
            OnDestroyBridge();
            BuildBridgeWithHeights(unitHeights, bridgeTypeSO);
        }

        private void DisableUnitsControl()
        {
            var unitsControl = GetComponent<UnitsControl>();
            unitsControl.DisableControl();
        }
    }
}
