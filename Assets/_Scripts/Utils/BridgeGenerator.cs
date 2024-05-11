using System.Collections.Generic;
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
    private BridgeAnimationManager animationManager;


    private int chosenSpriteCollection = 0;
    private List<int> failedUnits = new List<int>();
    private Bridge bridge;
    private UnitsControl unitsControl;


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
            bridgeCollectionSO.BridgeTypes[chosenSpriteCollection]
        );

        bridge.BuildPlayerUnits();

        bridge.ReplacePUnitSprite();

        FingerUnit[] fingerUnits =
            { FingerUnit.first, FingerUnit.second, FingerUnit.third, FingerUnit.fourth, FingerUnit.fifth };

        for (int i = 0; i < NumBridgeUnits; i++) {
            bridge.bridgePlayerUnits[i].GetComponent<UnitPlaced>().fingerUnit = fingerUnits[i];
        }

        unitsControl.SetPlayerUnits(bridge.bridgePlayerUnits);

        bridge.GenerateBridgeEnvironment(bridge.bridgePlayerUnits);
        
        animationManager.AnimateBuildUp(bridge); // Call the animation manager to animate the bridge build up
        

        // All units -> field in bridge class - GameObject[] bridgeUnits
        // can be used to iterate all the units in the bridge
    }
}