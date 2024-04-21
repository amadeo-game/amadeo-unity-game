using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeGenerator : MonoBehaviour {
    [SerializeField, Range(0, 5)] private int bridgeUnitHeight0;
    [SerializeField, Range(0, 5)] private int bridgeUnitHeight1;
    [SerializeField, Range(0, 5)] private int bridgeUnitHeight2;
    [SerializeField, Range(0, 5)] private int bridgeUnitHeight3;
    [SerializeField, Range(0, 5)] private int bridgeUnitHeight4;

    [SerializeField] private GameObject bridgeUnitPrefab;
    private Vector3 prefabLocalScale;

    [Header("Bridge Environment")] [SerializeField]
    Sprite platformSprite;

    [SerializeField] Sprite bridgeSprite;
    
    private GameObject bridgeEnvUnit;
    void Start() {
        prefabLocalScale = bridgeUnitPrefab.transform.localScale;
        GameObject[] bridgeUnits = new GameObject[5];
        int[] bridgeHeights =
            { bridgeUnitHeight0, bridgeUnitHeight1, bridgeUnitHeight2, bridgeUnitHeight3, bridgeUnitHeight4 };

        for (int i = 0, j = 0; j < bridgeHeights.Length; i += 4, j++) {
            bridgeUnits[j] = Instantiate(bridgeUnitPrefab,
                new Vector3(bridgeUnitPrefab.transform.position.x + i, bridgeHeights[j], 0),
                bridgeUnitPrefab.transform.rotation);
            bridgeUnits[j].GetComponent<SpriteReplacer>().ReplaceSprite();
        }

        for (int i = 0; i < bridgeUnits.Length - 1; i++) {
            
            var leftUnit = bridgeUnits[i].transform;
            var rightUnit = bridgeUnits[i + 1].transform;
            bridgeEnvUnit = new GameObject("FirstBridgeEnv", typeof(SpriteRenderer), typeof(SpriteReplacer));
            Debug.Log("Changing sprite of bridge environment");
            bridgeEnvUnit.GetComponent<SpriteRenderer>().sprite = platformSprite;
            bridgeEnvUnit.GetComponent<SpriteReplacer>().newSprite = bridgeSprite;

            // Calculate the position of the new square
            float square1LeftEdgePos = leftUnit.position.x + (leftUnit.localScale.x / 2);
            Debug.Log("Square 1 position: " + leftUnit.position.x + " Square 1 scale: " + prefabLocalScale.x);
            Debug.Log("Square 1 Left Edge Position: " + square1LeftEdgePos);
            float square2RightEdgePos = rightUnit.position.x - (rightUnit.localScale.x / 2);
            Debug.Log("Square 2 position: " + rightUnit.position.x + " Square 2 scale: " + rightUnit.localScale.x);
            Debug.Log("Square 2 Right Edge Position: " + square2RightEdgePos);

            Vector3 square1EdgePosition =
                new Vector3(square1LeftEdgePos + 0.3f, leftUnit.position.y, leftUnit.position.z);
            Vector3 square2EdgePosition =
                new Vector3(square2RightEdgePos - 0.3f, rightUnit.position.y, rightUnit.position.z);
            Vector3 position = (square1EdgePosition + square2EdgePosition) / 2;

            // Calculate the rotation of the new square
            float angle =
                Mathf.Atan2(square2EdgePosition.y - square1EdgePosition.y,
                    square2EdgePosition.x - square1EdgePosition.x) *
                Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            // Calculate the scale of the new square
            float distance = Vector3.Distance(square1EdgePosition, square2EdgePosition);
            Vector3 scale = new Vector3(distance / Mathf.Abs(Mathf.Cos(rotation.eulerAngles.z * Mathf.Deg2Rad)),
                prefabLocalScale.y, prefabLocalScale.z);

            // Set the position, rotation and scale of the new square
            bridgeEnvUnit.transform.position = position;
            bridgeEnvUnit.transform.rotation = rotation;
            bridgeEnvUnit.transform.localScale = scale;
            bridgeEnvUnit.GetComponent<SpriteReplacer>().ReplaceSprite();
        }
    }
}