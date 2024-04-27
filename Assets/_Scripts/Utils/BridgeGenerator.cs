using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(UnitsControl))]
public class BridgeGenerator : MonoBehaviour {
    private const int NumBridgeUnits = 5;

    [SerializeField, Range(0, 5)]
    private int[] playerUnitsHeights = new int[NumBridgeUnits]; // Set this in the Inspector

    [SerializeField] private GameObject bridgeEnvUnitPrefab;
    [SerializeField] private GameObject bridgePlayerUnitPrefab;
    [SerializeField] private GameObject playerUnitPlaceHolder;
    [SerializeField] private BridgeSpritesCollection[] bridgeSpritesCollections;

    private int chosenSpriteCollection = 0;
    private List<int> failedUnits = new List<int>();
    private Bridge bridge;
    private UnitsControl unitsControl;


    void Start() {
        unitsControl = GetComponent<UnitsControl>();
        Build();
    }

    private void Build() {
        bridge = new Bridge(
            playerUnitsHeights,
            bridgePlayerUnitPrefab,
            bridgeEnvUnitPrefab,
            playerUnitPlaceHolder,
            bridgeSpritesCollections[chosenSpriteCollection]);

        bridge.BuildPlayerUnits();
        
        bridge.ReplacePUnitSprite();

        FingerUnit[] fingerUnits =
            { FingerUnit.first, FingerUnit.second, FingerUnit.third, FingerUnit.fourth, FingerUnit.fifth };

        for (int i = 0; i < NumBridgeUnits; i++) {
            bridge.bridgePlayerUnits[i].GetComponent<UnitPlaced>().fingerUnit = fingerUnits[i];
        }

        unitsControl.SetPlayerUnits(bridge.bridgePlayerUnits);

        bridge.GenerateBridgeEnvironment(bridge.bridgePlayerUnits);
    }
}


// public class BridgeGenerator : MonoBehaviour {
//     [SerializeField, Range(0, 5)] private int bridgeUnitHeight0;
//     [SerializeField, Range(0, 5)] private int bridgeUnitHeight1;
//     [SerializeField, Range(0, 5)] private int bridgeUnitHeight2;
//     [SerializeField, Range(0, 5)] private int bridgeUnitHeight3;
//     [SerializeField, Range(0, 5)] private int bridgeUnitHeight4;
//
//     [SerializeField] private GameObject bridgeUnitPrefab;
//     private Vector3 prefabLocalScale;
//
//     [Header("Bridge Environment")] [SerializeField]
//     Sprite platformSprite;
//
//     [SerializeField] Sprite bridgeSprite;
//     
//     private GameObject bridgeEnvUnit;
//     void Start() {
//         prefabLocalScale = bridgeUnitPrefab.transform.localScale;
//         GameObject[] bridgeUnits = new GameObject[5];
//         int[] bridgeHeights =
//             { bridgeUnitHeight0, bridgeUnitHeight1, bridgeUnitHeight2, bridgeUnitHeight3, bridgeUnitHeight4 };
//
//         // Generate the bridge units
//         GenerateBridgeUnits(bridgeHeights, bridgeUnits);
//         // Generate the bridge environment
//         GenerateBridgeEnvironment(bridgeUnits);
//     }
//
//     private void GenerateBridgeEnvironment(GameObject[] bridgeUnits) {
//         for (int i = 0; i < bridgeUnits.Length - 1; i++) {
//             var leftUnit = bridgeUnits[i].transform;
//             var rightUnit = bridgeUnits[i + 1].transform;
//             bridgeEnvUnit = new GameObject("FirstBridgeEnv", typeof(SpriteRenderer), typeof(SpriteReplacer));
//             Debug.Log("Changing sprite of bridge environment");
//             bridgeEnvUnit.GetComponent<SpriteRenderer>().sprite = platformSprite;
//             bridgeEnvUnit.GetComponent<SpriteReplacer>().newSprite = bridgeSprite;
//
//             // Calculate the position of the new square
//             var position1 = leftUnit.position;
//             float square1LeftEdgePos = position1.x + (leftUnit.localScale.x / 2);
//             // Debug.Log("Square 1 position: " + leftUnit.position.x + " Square 1 scale: " + prefabLocalScale.x);
//             // Debug.Log("Square 1 Left Edge Position: " + square1LeftEdgePos);
//             var position2 = rightUnit.position;
//             float square2RightEdgePos = position2.x - (rightUnit.localScale.x / 2);
//             // Debug.Log("Square 2 position: " + rightUnit.position.x + " Square 2 scale: " + rightUnit.localScale.x);
//             // Debug.Log("Square 2 Right Edge Position: " + square2RightEdgePos);
//
//             Vector3 square1EdgePosition =
//                 new Vector3(square1LeftEdgePos + 0.3f, position1.y, position1.z);
//             Vector3 square2EdgePosition =
//                 new Vector3(square2RightEdgePos - 0.3f, position2.y, position2.z);
//             Vector3 position = (square1EdgePosition + square2EdgePosition) / 2;
//
//             // Calculate the rotation of the new square
//             float angle =
//                 Mathf.Atan2(square2EdgePosition.y - square1EdgePosition.y,
//                     square2EdgePosition.x - square1EdgePosition.x) *
//                 Mathf.Rad2Deg;
//             Quaternion rotation = Quaternion.Euler(0, 0, angle);
//
//             // Calculate the scale of the new square
//             float distance = Vector3.Distance(square1EdgePosition, square2EdgePosition);
//             Vector3 scale = new Vector3(distance / Mathf.Abs(Mathf.Cos(rotation.eulerAngles.z * Mathf.Deg2Rad)),
//                 prefabLocalScale.y, prefabLocalScale.z);
//
//             // Set the position, rotation and scale of the new square
//             bridgeEnvUnit.transform.position = position;
//             bridgeEnvUnit.transform.rotation = rotation;
//             bridgeEnvUnit.transform.localScale = scale;
//             bridgeEnvUnit.GetComponent<SpriteReplacer>().ReplaceSprite();
//         }
//     }
//
//     private void GenerateBridgeUnits(int[] bridgeHeights, GameObject[] bridgeUnits) {
//         for (int i = 0, j = 0; j < bridgeHeights.Length; i += 4, j++) {
//             bridgeUnits[j] = Instantiate(bridgeUnitPrefab,
//                 new Vector3(bridgeUnitPrefab.transform.position.x + i, bridgeHeights[j], 0),
//                 bridgeUnitPrefab.transform.rotation);
//             bridgeUnits[j].GetComponent<SpriteReplacer>().ReplaceSprite();
//         }
//     }
// }