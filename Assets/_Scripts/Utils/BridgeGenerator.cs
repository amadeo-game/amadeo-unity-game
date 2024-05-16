using System.Collections;
using System.Linq;
using UnityEngine;
using static BridgeBuilder;
using static Bridge;


[RequireComponent(typeof(UnitsControl), typeof(BridgeAnimationManager))]
public class BridgeGenerator : MonoBehaviour {
    private const int NumBridgeUnits = 5;

    [SerializeField, Range(0, 5)]
    private int[] playerUnitsHeights = new int[NumBridgeUnits]; // Set this in the Inspector


    [SerializeField] private GameObject bridgePlayerUnitPrefab;
    [SerializeField] private GameObject playerUnitPlaceHolder;
    [SerializeField] private BridgeCollectionSO bridgeCollectionSO;

    [SerializeField, Tooltip("x-axis value from where the bridge will rise"), Min(0)]
    private int bridgeRiseDownOffset = 12;

    private BridgeAnimationManager animationManager;


    private int chosenSpriteCollection = 0;

    // private List<int> failedUnits = new List<int>();
    private UnitsControl unitsControl;


    private static readonly FingerUnit[] FingerUnits =
        { FingerUnit.first, FingerUnit.second, FingerUnit.third, FingerUnit.fourth, FingerUnit.fifth };

    void Start() {
        unitsControl = GetComponent<UnitsControl>();
        animationManager = GetComponent<BridgeAnimationManager>(); // Reference the animation manager
        Build();
    }

    private void Build() {
        GameObject bridgeHolder = new GameObject("Bridge Holder");

        Vector2[] playerUnitsPositions = GetBridgePlayerPositions(playerUnitsHeights);

        GameObject[] playerUnits = BuildPlayerUnits(
            bridge: bridgeHolder,
            playableUnitsPositions: playerUnitsPositions,
            playerUnitPrefab: bridgePlayerUnitPrefab,
            bridgeRiseXOffset: bridgeRiseDownOffset);

        GameObject[] playerUnitPlaceHolders = BuildPlaceHolderUnits(
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

        int[] bridgeEnvHeights = bridgeEnvMeasures.Select(unit => (int)unit.Position.y).ToArray();
        
        int[] totalBridgeHeights = GetSequencedBridgeHeights( bridgeEnvHeights, playerUnitsHeights);
        GameObject[] totalBridgeUnits = GetSequencedBridgeUnits(playerUnits, bridgeEnvUnits);
        
        Debug.Log( string.Join(", ", playerUnitsHeights.Select(x => x.ToString()).ToArray()));
        Debug.Log( string.Join(", ", bridgeEnvHeights.Select(x => x.ToString()).ToArray()));
        Debug.Log(string.Join(", ", totalBridgeHeights.Select(x => x.ToString()).ToArray()));
        StartCoroutine(AnimateBuildUp(totalBridgeHeights, totalBridgeUnits));

        playerUnitPlaceHolders.ToList().ForEach(x => x.gameObject.SetActive(true));
    }

    IEnumerator AnimateBuildUp(int[] sequencedBridgeHeights, GameObject[] sequencedBridgeUnits) {
        yield return StartCoroutine(
            animationManager.AnimateBuildUpCoroutine(sequencedBridgeUnits, sequencedBridgeHeights));


        // unitsControl.SetPlayerUnits(bridge.bridgePlayerUnits);
    }


    // public void HandleBridgeFailure() {
    //     if (bridge != null && animationManager != null) {
    //         StartCoroutine(AnimateShakeAndCollapseCoroutine());
    //     }
    // }

    // IEnumerator AnimateShakeAndCollapseCoroutine() {
    //     yield return StartCoroutine(animationManager.AnimateShakeAndCollapse(bridge.bridgePlayerUnits));
    // }


    private void SetPlayerUnitsFingers(FingerUnit[] fingerUnits, GameObject[] bridgePlayerUnits) {
        for (int i = 0; i < NumBridgeUnits; i++) {
            bridgePlayerUnits[i].GetComponent<UnitPlaced>().fingerUnit = fingerUnits[i];
        }
    }
}