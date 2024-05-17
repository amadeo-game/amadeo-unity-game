using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static BridgeBuilder;
using static Bridge;


[RequireComponent(typeof(UnitsControl), typeof(BridgeAnimationManager), typeof(BridgeStateMachine))]
public class BridgeGenerator : MonoBehaviour {
    private const int NumBridgeUnits = 5;

    [SerializeField, Range(0, 5)]
    private int[] playerUnitsHeights = new int[NumBridgeUnits]; // Set this in the Inspector


    [SerializeField] private GameObject bridgePlayerUnitPrefab;
    [SerializeField] private GameObject playerUnitPlaceHolder;
    [SerializeField] private BridgeCollectionSO bridgeCollectionSO;
    
    private BridgeStateMachine stateMachine;
    
    
    [SerializeField, Tooltip("x-axis value from where the bridge will rise"), Min(0)]
    private int bridgeRiseDownOffset = 12;

    private BridgeAnimationManager animationManager;

    private GameObject bridgeHolder;
    private GameObject[] playerUnits;
    private GameObject[] totalBridgeUnits;
    private GameObject[] playerUnitPlaceHolders;

    
    private void OnEnable() {
        GameManager.OnUnitsPlaced += HandleUnitsPlaced;
        stateMachine.OnBuildStart += BuildBridge;
        stateMachine.OnBuildComplete += EnableUnitsControl;
        stateMachine.OnCollapseStart += OnBridgeFailed;
        stateMachine.OnCollapseComplete += OnCollapseComplete;
        stateMachine.OnSuccess += HandleBridgeFailure; // temporary

    }

    private void OnDisable() {
        GameManager.OnUnitsPlaced -= HandleUnitsPlaced;
        stateMachine.OnBuildStart -= BuildBridge;
        stateMachine.OnBuildComplete -= EnableUnitsControl;
        stateMachine.OnCollapseComplete -= OnCollapseComplete;
    }
    
    private void HandleUnitsPlaced() {
        if (stateMachine.CanBuild()) {
            stateMachine.StartBuilding();
        }
    }
    
    private void EnableUnitsControl() {
        unitsControl.SetPlayerUnits( playerUnits);
    }




    private int chosenSpriteCollection = 0;
    private UnitsControl unitsControl;


    private static readonly FingerUnit[] FingerUnits =
        { FingerUnit.first, FingerUnit.second, FingerUnit.third, FingerUnit.fourth, FingerUnit.fifth };



    private void Awake() {
        unitsControl = GetComponent<UnitsControl>();
        animationManager = GetComponent<BridgeAnimationManager>();
        stateMachine = GetComponent<BridgeStateMachine>();
    }

    public void BuildBridge() {
        Build();
        AnimateBuildUp();
        SetPlayerPlaceHolders(true);
    }

    private void SetPlayerPlaceHolders(bool state) {
        playerUnitPlaceHolders.ToList().ForEach(x => x.gameObject.SetActive(state));
    }

    private void AnimateBuildUp() {
        animationManager.AnimateBuildUpUnits(totalBridgeUnits, bridgeRiseDownOffset);
    }

    private void Build() {
        bridgeHolder = new GameObject("Bridge Holder");

        Vector2[] playerUnitsPositions = GetBridgePlayerPositions(playerUnitsHeights);

        playerUnits = BuildPlayerUnits(
            bridge: bridgeHolder,
            playableUnitsPositions: playerUnitsPositions,
            playerUnitPrefab: bridgePlayerUnitPrefab,
            bridgeRiseXOffset: bridgeRiseDownOffset);

        playerUnitPlaceHolders = BuildPlaceHolderUnits(
            bridge: bridgeHolder,
            playableUnitsPositions: playerUnitsPositions,
            playerUnitPlaceHolder: playerUnitPlaceHolder);

        SpriteUnit playerUnitSprite = bridgeCollectionSO.BridgeTypes[chosenSpriteCollection].BridgeSpritesCollections
            .PlayerUnitSprite;

        ReplacePUnitSprite(playerUnitSprite, playerUnits, playerUnitPlaceHolders);

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
        
        totalBridgeUnits = GetSequencedBridgeUnits(playerUnits, bridgeEnvUnits);

    }

    private void OnBridgeFailed() {
        unitsControl.DisableControl();
        SetPlayerPlaceHolders(false);
        animationManager.AnimateFallDownUnits(totalBridgeUnits, bridgeRiseDownOffset);
    }

    private void OnCollapseComplete() {
        Destroy(bridgeHolder.gameObject);
        stateMachine.ResetState();
    }

    // private void AnimateBuildUp() {
    //     
    //         animationManager.AnimateBuildUpUnits(totalBridgeUnits, bridgeRiseDownOffset);
    //         // unitsControl.SetPlayerUnits(bridge.bridgePlayerUnits);
    // }




    private void HandleBridgeFailure() {
        if (bridgeHolder != null && animationManager != null) {
            animationManager.AnimateFallDownUnits(
                totalBridgeUnits, bridgeRiseDownOffset
                );
        }
    }


    private void SetPlayerUnitsFingers(FingerUnit[] fingerUnits, GameObject[] bridgePlayerUnits) {
        for (int i = 0; i < NumBridgeUnits; i++) {
            bridgePlayerUnits[i].GetComponent<UnitPlaced>().fingerUnit = fingerUnits[i];
        }
    }
}

// private void Update() {
//     if (Input.GetKeyDown(KeyCode.Space)) {
//             
//         HandleBridgeFailure();
//     }
// }