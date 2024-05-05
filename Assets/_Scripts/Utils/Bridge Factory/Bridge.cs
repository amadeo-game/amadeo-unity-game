using System;
using UnityEngine;

public class Bridge {
    private GameObject prefabExample;
    private BridgeTypeSO bridgeTypeSO;
    private GameObject[] bridgeEnvUnits;
    public GameObject[] bridgePlayerUnits { get; private set; }
    private GameObject[] playerBridgeUnitsPlaceHolders;

    private readonly int[] playerUnitsHeights;
    private readonly GameObject envUnitPrefab;
    private readonly GameObject playerUnitPrefab;
    private readonly BridgeSpriteCollection bridgeSpritesCollection;
    private readonly GameObject playerUnitPlaceHolder;

    private readonly int numBridgeUnits;
    private readonly Vector3 prefabLocalScale;

    public Bridge(
        int[] playerUnitsHeights, GameObject playerUnitPrefab, GameObject envUnitPrefab
        , GameObject playerUnitPlaceHolder,
        BridgeSpriteCollection bridgeSpritesCollection
        , BridgeTypeSO bridgeTypeSO,
        GameObject prefabExample) {
        this.playerUnitsHeights = playerUnitsHeights;
        this.playerUnitPrefab = playerUnitPrefab;
        this.playerUnitPlaceHolder = playerUnitPlaceHolder;
        this.envUnitPrefab = envUnitPrefab;
        this.bridgeSpritesCollection = bridgeSpritesCollection;
        prefabLocalScale = envUnitPrefab.transform.localScale;
        numBridgeUnits = playerUnitsHeights.Length;
        this.bridgeTypeSO = bridgeTypeSO;

        this.prefabExample = prefabExample;
    }


    public void BuildPlayerUnits() {
        bridgePlayerUnits = new GameObject[numBridgeUnits];
        playerBridgeUnitsPlaceHolders = new GameObject[numBridgeUnits];

        for (int i = 0; i < numBridgeUnits; i++) {
            var position = envUnitPrefab.transform.position;
            var rotation = envUnitPrefab.transform.rotation;
            bridgePlayerUnits[i] = GameObject.Instantiate(playerUnitPrefab,
                new Vector3(position.x + i * 4, playerUnitsHeights[i], 0),
                rotation);
            playerBridgeUnitsPlaceHolders[i] = GameObject.Instantiate(playerUnitPlaceHolder,
                new Vector3(position.x + i * 4, playerUnitsHeights[i], 0),
                rotation);
        }
    }

    public void ReplacePUnitSprite() {
        for (int i = 0; i < numBridgeUnits; i++) {
            SpriteReplacer.ReplaceSprite(bridgeSpritesCollection.PlayerUnitSprite, bridgePlayerUnits[i]);
            SpriteReplacer.ReplaceSprite(bridgeSpritesCollection.PlayerUnitSprite, playerBridgeUnitsPlaceHolders[i]);
        }
    }

    public void GenerateBridgeEnvironment(GameObject[] bridgeUnits) {
        for (int i = 0; i < bridgeUnits.Length - 1; i++) {
            EnvElevationUnit(bridgeUnits[i].transform, bridgeUnits[i + 1].transform);
        }
    }

    private static void LocationDebugger(Vector3 position) {
        // return small white square game object
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.SetPositionAndRotation(position, Quaternion.identity);
        go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

    private void EnvElevationUnit(Transform leftUnit, Transform rightUnit) {
        // Calculate the position of the new square 
        var position1 = leftUnit.position;
        var position2 = rightUnit.position;

        var heightDifference = (position1.y - position2.y);
        var absHeightDifference = Mathf.Abs(heightDifference);
        int diffSign = Math.Sign(heightDifference);

        Vector2 leftPos = diffSign < 0 ? position1 + new Vector3(2, 0) : position2 + new Vector3(-2, 0);
        Debug.Log("Left Pos: " + leftPos);
        GameObject example =
            GameObject.Instantiate(bridgeTypeSO.BridgeEnvUnitPrefab(Mathf.RoundToInt(absHeightDifference)), leftPos,
                Quaternion.identity);
        SpriteRenderer[] children = example.GetComponentsInChildren<SpriteRenderer>();
        foreach (var child in children) {
            SpriteReplacer.ReplaceSprite(bridgeSpritesCollection.EnvironmentSprites[0], child.gameObject);
        }

        if (diffSign > 0) {
            example.transform.localScale = new Vector3(-1, 1, 1);
        }
    }


    private void EnvConnectingUnit(Vector3 position, Vector3 square1EdgePosition, Vector3 square2EdgePosition) {
        GameObject bridgeEnvUnit = GameObject.Instantiate(envUnitPrefab);


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
        // bridgeEnvUnit.GetComponent<SpriteReplacer>().ReplaceSprite();
        SpriteReplacer.ReplaceSprite(bridgeSpritesCollection.EnvironmentSprites[0], bridgeEnvUnit);
    }
}