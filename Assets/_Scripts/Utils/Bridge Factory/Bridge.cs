using System;
using UnityEngine;

public class Bridge {
    private BridgeTypeSO bridgeTypeSO;
    public GameObject[] BridgeEnvUnits;
    public GameObject[] bridgePlayerUnits { get; private set; }
    public GameObject[] PlayerBridgeUnitsPlaceHolders;

    public readonly int[] PlayerUnitsHeights;
    private readonly GameObject envUnitPrefab;
    private readonly GameObject playerUnitPrefab;
    private readonly BridgeSpriteCollection bridgeSpritesCollection;
    private readonly GameObject playerUnitPlaceHolder;

    private readonly int numBridgeUnits;
    public Bridge(
        int[] playerUnitsHeights, GameObject playerUnitPrefab, GameObject envUnitPrefab
        , GameObject playerUnitPlaceHolder,
        BridgeSpriteCollection bridgeSpritesCollection
        , BridgeTypeSO bridgeTypeSO) {
        this.PlayerUnitsHeights = playerUnitsHeights;
        this.playerUnitPrefab = playerUnitPrefab;
        this.playerUnitPlaceHolder = playerUnitPlaceHolder;
        this.envUnitPrefab = envUnitPrefab;
        this.bridgeSpritesCollection = bridgeSpritesCollection;
        numBridgeUnits = playerUnitsHeights.Length;
        this.bridgeTypeSO = bridgeTypeSO;
    }


    public void BuildPlayerUnits() {
        bridgePlayerUnits = new GameObject[numBridgeUnits];
        PlayerBridgeUnitsPlaceHolders = new GameObject[numBridgeUnits];

        for (int i = 0; i < numBridgeUnits; i++) {
            var position = envUnitPrefab.transform.position;
            var rotation = envUnitPrefab.transform.rotation;
            bridgePlayerUnits[i] = GameObject.Instantiate(playerUnitPrefab,
                new Vector3(position.x + i * 4, PlayerUnitsHeights[i], 0),
                rotation);
            PlayerBridgeUnitsPlaceHolders[i] = GameObject.Instantiate(playerUnitPlaceHolder,
                new Vector3(position.x + i * 4, PlayerUnitsHeights[i], 0),
                rotation);
        }
    }

    public void ReplacePUnitSprite() {
        for (int i = 0; i < numBridgeUnits; i++) {
            SpriteReplacer.ReplaceSprite(bridgeSpritesCollection.PlayerUnitSprite, bridgePlayerUnits[i]);
            SpriteReplacer.ReplaceSprite(bridgeSpritesCollection.PlayerUnitSprite, PlayerBridgeUnitsPlaceHolders[i]);
        }
    }

    public void GenerateBridgeEnvironment(GameObject[] bridgeUnits) {
        for (int i = 0; i < bridgeUnits.Length - 1; i++) {
            EnvElevationUnit(bridgeUnits[i].transform, bridgeUnits[i + 1].transform);
        }
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
}