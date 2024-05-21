using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static BridgeBuilder;
using static Bridge;
public enum FingerUnit {
    first,
    second,
    third,
    fourth,
    fifth,
}

[RequireComponent(typeof(UnitsControl), typeof(BridgeAnimationManager), typeof(BridgeStateMachine))]
public class BridgeGenerator : MonoBehaviour {
    private const int NumBridgeUnits = 5;

    [SerializeField, Range(0, 5)] // TODO: support flexion mode (negative values)
    private int[] playerUnitsHeights = new int[NumBridgeUnits]; // Set this in the Inspector


    [SerializeField] private GameObject bridgePlayerUnitPrefab;
    [SerializeField] private GameObject playerUnitPlaceHolder;
    [SerializeField] private BridgeCollectionSO bridgeCollectionSO;


    [SerializeField, Tooltip("x-axis value from where the bridge will rise"), Min(0)]
    private int bridgeRiseDownOffset = 12;


    private BridgeAnimationManager animationManager;
    private BridgeStateMachine stateMachine;
    private BridgeMediator bridgeMediator;


    private GameObject bridgeHolder;
    private GameObject[] playerUnits;
    private GameObject[] totalBridgeUnits;
    private GameObject[] playerGuideUnits;

    
    private void OnEnable()
    {
        if (bridgeMediator != null)
        {
            bridgeMediator.OnBuildStart += BuildBridge;
            bridgeMediator.OnBuildComplete += EnableUnitsControl;
            bridgeMediator.OnCollapseStart += OnBridgeFailed;
            bridgeMediator.OnCollapseComplete += OnCollapseComplete;
            bridgeMediator.OnSuccessStart += AnimateSuccess;
        }
    }

    private void OnDisable()
    {
        if (bridgeMediator != null)
        {
            bridgeMediator.OnBuildStart -= BuildBridge;
            bridgeMediator.OnBuildComplete -= EnableUnitsControl;
            bridgeMediator.OnCollapseStart -= OnBridgeFailed;
            bridgeMediator.OnCollapseComplete -= OnCollapseComplete;
            bridgeMediator.OnSuccessStart -= AnimateSuccess;
        }
    }

    private void EnableUnitsControl() {
        unitsControl.SetPlayerUnits( playerUnits);
    }

    private int chosenSpriteCollection = 0;
    private UnitsControl unitsControl;


    private static readonly FingerUnit[] FingerUnits =
        { FingerUnit.first, FingerUnit.second, FingerUnit.third, FingerUnit.fourth, FingerUnit.fifth };



    private void Awake()
    {
        unitsControl = GetComponent<UnitsControl>();
        animationManager = GetComponent<BridgeAnimationManager>();
        stateMachine = GetComponent<BridgeStateMachine>();
        bridgeMediator = GetComponent<BridgeMediator>();

        stateMachine.Initialize(bridgeMediator);
    }

    private void BuildBridge() {
        Build();
        AnimateBuildUp();
        SetPlayerGuideUnits(true); // TODO: find better name for this method
    }

    private void SetPlayerGuideUnits(bool state) {
        playerGuideUnits.ToList().ForEach(x => x.gameObject.SetActive(state));
    }

    private void Build() {
        if (bridgeHolder != null) {
            return;
        }
        bridgeHolder = new GameObject("Bridge Holder");

        // Set Positions logic only
        
        // Get the positions of the player units on the bridge
        Vector2[] playerUnitsPositions = GetBridgePlayerPositions(playerUnitsHeights);

        // Generate the player units as GameObjects
        playerUnits = BuildPlayerUnits(
            bridge: bridgeHolder,
            playableUnitsPositions: playerUnitsPositions,
            playerUnitPrefab: bridgePlayerUnitPrefab,
            bridgeRiseYOffset: bridgeRiseDownOffset);

        foreach (var unit in playerUnits)
        {
            var unitReachedDestination = unit.GetComponent<UnitReachedDestination>();
            if (unitReachedDestination != null)
            {
                unitReachedDestination.Initialize(bridgeMediator);
            }
            else
            {
                Debug.LogError("UnitReachedDestination component is missing on player unit.");
            }
        }       
        // Generate the player unit GuideUnits as GameObjects
        playerGuideUnits = BuildGuideUnits(
            bridge: bridgeHolder,
            playableUnitsPositions: playerUnitsPositions,
            playerUnitPlaceHolder: playerUnitPlaceHolder);

        
        SpriteUnit playerUnitSprite = bridgeCollectionSO.BridgeTypes[chosenSpriteCollection].BridgeSpritesCollections
            .PlayerUnitSprite;

        ReplacePUnitSprite(playerUnitSprite, playerUnits, playerGuideUnits);

        SetPlayerUnitsFingers(FingerUnits, playerUnits);

        
        UnitProperties[] bridgeEnvMeasures = GetBridgeEnvironmentHeights(playerUnitsHeights);
        
        GameObject[] bridgeEnvUnits = GenerateBridgeEnvironment(
            unitPropertiesArray: bridgeEnvMeasures,
            bridgeCollectionSO.BridgeTypes[chosenSpriteCollection],
            bridgeHolder,
            bridgeRiseDownOffset);

        ReplaceEnvSprites(
            bridgeEnvUnits: bridgeEnvUnits,
            spriteUnit: bridgeCollectionSO.BridgeTypes[chosenSpriteCollection].BridgeSpritesCollections
                .EnvironmentSprites[0]);
        
        // Sequenced bridge units is comfortable for sequence animation
        totalBridgeUnits = GetSequencedBridgeUnits(playerUnits, bridgeEnvUnits);

    }

    private void OnBridgeFailed() {
        DisableUnitsControl();
        SetPlayerGuideUnits(false);
        AnimateBridgeFallDown();
    }

    private void DisableUnitsControl() {
        unitsControl.DisableControl();
    }

    private void OnCollapseComplete() {
        Destroy(bridgeHolder.gameObject);
        stateMachine.ResetState();
    }

    private void AnimateBridgeFallDown()
    {
        if (bridgeHolder != null && animationManager != null)
        {
            animationManager.AnimateBridgeFallDown(totalBridgeUnits, bridgeRiseDownOffset);
        }
    }

    private void AnimateBuildUp() {
        animationManager.AnimateBuildUpBridge(totalBridgeUnits, bridgeRiseDownOffset);
    }
    
    private void AnimateSuccess() {
        DisableUnitsControl();
        animationManager.AnimateSuccess(playerUnits, playerUnitsHeights);
    }


    private void SetPlayerUnitsFingers(FingerUnit[] fingerUnits, GameObject[] bridgePlayerUnits)
    {
        for (int i = 0; i < NumBridgeUnits; i++)
        {
            var unitReachedDestination = bridgePlayerUnits[i].GetComponent<UnitReachedDestination>();
            if (unitReachedDestination != null)
            {
                unitReachedDestination.fingerUnit = fingerUnits[i];
            }
            else
            {
                Debug.LogError("UnitReachedDestination component is missing on player unit.");
            }
        }
    }
}