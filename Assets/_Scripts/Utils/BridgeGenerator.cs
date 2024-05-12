using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(UnitsControl), typeof(BridgeAnimationManager))]
public class BridgeGenerator : MonoBehaviour {
    private const int NumBridgeUnits = 5;

    [SerializeField, Range(0, 5)]
    private int[] playerUnitsHeights = new int[NumBridgeUnits]; // Set this in the Inspector


    [SerializeField] private GameObject bridgeEnvUnitPrefab;
    [SerializeField] private GameObject bridgePlayerUnitPrefab;
    [SerializeField] private GameObject playerUnitPlaceHolder;
    [SerializeField] private BridgeCollectionSO bridgeCollectionSO;

    [SerializeField, Tooltip("x-axis value from where the bridge will rise"), Min(0)]
    private int bridgeRiseDownOffset = 12;

    private BridgeAnimationManager animationManager;


    private int chosenSpriteCollection = 0;

    // private List<int> failedUnits = new List<int>();
    private Bridge bridge;
    private UnitsControl unitsControl;


    private static readonly FingerUnit[] FingerUnits =
        { FingerUnit.first, FingerUnit.second, FingerUnit.third, FingerUnit.fourth, FingerUnit.fifth };

    void Start() {
        unitsControl = GetComponent<UnitsControl>();
        animationManager = GetComponent<BridgeAnimationManager>(); // Reference the animation manager

        Build();
    }

    private void Build() {
        bridge = new Bridge(
            playerUnitsHeights,
            bridgePlayerUnitPrefab,
            bridgeEnvUnitPrefab,
            playerUnitPlaceHolder,
            bridgeCollectionSO.BridgeTypes[chosenSpriteCollection].BridgeSpritesCollections,
            bridgeCollectionSO.BridgeTypes[chosenSpriteCollection],
            bridgeRiseDownOffset
        );

        bridge.BuildPlayerUnits();

        bridge.ReplacePUnitSprite();

        SetPlayerUnitsFingers(FingerUnits);

        bridge.GenerateBridgeEnvironment();
        bridge.ReplaceEnvSprites();

        StartCoroutine(AnimateBuildUp());
    }

    IEnumerator AnimateBuildUp() {
        // Access bridge units through the bridge object
        if (bridge != null) {
            yield return StartCoroutine(
                animationManager.AnimateBuildUpCoroutine(bridge.bridgePlayerUnits, bridge.BridgeEnvUnits,
                    bridge.PlayerUnitsHeights, bridge.EnvUnitHeights));
        }

        bridge.EnablePlaceHolders();

        unitsControl.SetPlayerUnits(bridge.bridgePlayerUnits);
    }
    
    
    public void HandleBridgeFailure()
    {
        if (bridge != null && animationManager != null)
        {
            StartCoroutine(AnimateShakeAndCollapseCoroutine());
        }
    }

    IEnumerator AnimateShakeAndCollapseCoroutine()
    {
        yield return StartCoroutine(animationManager.AnimateShakeAndCollapse(bridge.bridgePlayerUnits));
    }




    private void SetPlayerUnitsFingers(FingerUnit[] fingerUnits) {
        for (int i = 0; i < NumBridgeUnits; i++) {
            bridge.bridgePlayerUnits[i].GetComponent<UnitPlaced>().fingerUnit = fingerUnits[i];
        }
    }
}

// private void Update() {
//     if (Input.GetKeyDown(KeyCode.Space)) {
//         unitsControl.enabled = false;
//         HandleBridgeFailure();
//     }
// }