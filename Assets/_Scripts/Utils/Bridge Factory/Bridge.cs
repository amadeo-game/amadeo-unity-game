using System;
using System.Linq;
using UnityEngine;

public class Bridge {
    private BridgeTypeSO bridgeTypeSO;
    private const int NumEnvUnits = 6;
    private const int NumPlayerUnits = 5;

    private static readonly int[] PlayerUnitVerticalPos = { -8, -4, 0, 4, 8 };
    private static readonly int[] EnvUnitVerticalPos = { -10, -6, -2, 2, 6, 10 };
    public readonly int[] EnvUnitHeights = new int[NumEnvUnits];
    public GameObject[] bridgePlayerUnits { get; private set; }
    public GameObject[] BridgeEnvUnits { get; private set; }
    public GameObject[] PlayerBridgeUnitsPlaceHolders;

    public readonly int[] PlayerUnitsHeights;
    private readonly GameObject envUnitPrefab;
    private readonly GameObject playerUnitPrefab;
    private readonly BridgeSpriteCollection bridgeSpritesCollection;
    private readonly GameObject playerUnitPlaceHolder;


    private readonly int bridgeRiseXOffset;

    public Bridge(
        int[] playerUnitsHeights, GameObject playerUnitPrefab, GameObject envUnitPrefab
        , GameObject playerUnitPlaceHolder,
        BridgeSpriteCollection bridgeSpritesCollection
        , BridgeTypeSO bridgeTypeSO
        , int bridgeRiseXOffset) {
        this.PlayerUnitsHeights = playerUnitsHeights;
        this.playerUnitPrefab = playerUnitPrefab;
        this.playerUnitPlaceHolder = playerUnitPlaceHolder;
        this.envUnitPrefab = envUnitPrefab;
        this.bridgeSpritesCollection = bridgeSpritesCollection;
        this.bridgeTypeSO = bridgeTypeSO;
        this.bridgeRiseXOffset = bridgeRiseXOffset;
    }


    public void ReplacePUnitSprite() {
        for (int i = 0; i < NumPlayerUnits; i++) {
            SpriteReplacer.ReplaceSprite(bridgeSpritesCollection.PlayerUnitSprite, bridgePlayerUnits[i]);
            SpriteReplacer.ReplaceSprite(bridgeSpritesCollection.PlayerUnitSprite, PlayerBridgeUnitsPlaceHolders[i]);
        }
    }

    public void BuildPlayerUnits() {
        bridgePlayerUnits = new GameObject[NumPlayerUnits];
        PlayerBridgeUnitsPlaceHolders = new GameObject[NumPlayerUnits];

        for (int i = 0; i < PlayerUnitVerticalPos.Length; i++) {
            // var position = envUnitPrefab.transform.position;
            var rotation = envUnitPrefab.transform.rotation;
            bridgePlayerUnits[i] = GameObject.Instantiate(playerUnitPrefab,
                new Vector3(PlayerUnitVerticalPos[i], PlayerUnitsHeights[i] - bridgeRiseXOffset, 0),
                rotation);

            PlayerBridgeUnitsPlaceHolders[i] = GameObject.Instantiate(playerUnitPlaceHolder,
                new Vector3(PlayerUnitVerticalPos[i], PlayerUnitsHeights[i], 0),
                rotation);
            PlayerBridgeUnitsPlaceHolders[i].gameObject.SetActive(false);
        }
    }

    public void GenerateBridgeEnvironment() {
        BridgeEnvUnits = new GameObject[NumEnvUnits];

        (BridgeEnvUnits[0], EnvUnitHeights[0]) = 
            EnvElevationUnit(leftY: 0, PlayerUnitsHeights[0], EnvUnitVerticalPos.First());
        
        for (int i = 0; i < NumPlayerUnits - 1; i++) {
            (BridgeEnvUnits[i + 1], EnvUnitHeights[i+1]) = 
                EnvElevationUnit(leftY: PlayerUnitsHeights[i], PlayerUnitsHeights[i + 1], EnvUnitVerticalPos[i + 1]);
        }

        (BridgeEnvUnits[NumEnvUnits - 1], EnvUnitHeights[NumEnvUnits - 1]) =
            EnvElevationUnit(leftY: PlayerUnitsHeights[NumPlayerUnits - 1], 0, EnvUnitVerticalPos.Last());
    }

    private (GameObject, int) EnvElevationUnit(int leftY, int rightY, int xPos) {
        // Calculate the height difference of the adjacent units 
        var heightDifference = (leftY - rightY);
        var absHeightDifference = Mathf.Abs(heightDifference);
        int diffSign = Math.Sign(heightDifference);
        int height = diffSign < 0
            ? leftY
            : rightY;


        Vector2 leftPos = diffSign < 0
            ? new Vector3(xPos, leftY - bridgeRiseXOffset)
            : new Vector3(xPos, rightY - bridgeRiseXOffset);
        Debug.Log("Left Pos: " + leftPos);
        GameObject envUnit =
            GameObject.Instantiate(bridgeTypeSO.BridgeEnvUnitPrefab(Mathf.RoundToInt(absHeightDifference)), leftPos,
                Quaternion.identity);
        if (diffSign > 0) {
            envUnit.transform.localScale = new Vector3(-1, 1, 1);
        }

        return (envUnit, height);
    }

    public void ReplaceEnvSprites() {
        foreach (var envUnit in BridgeEnvUnits) {
            bool isMirrored = envUnit.transform.localScale.x < 0;
            SpriteRenderer[] children = envUnit.GetComponentsInChildren<SpriteRenderer>();
            foreach (var child in children) {
                SpriteReplacer.ReplaceSprite(bridgeSpritesCollection.EnvironmentSprites[0], child.gameObject,
                    isMirrored);
            }
        }
    }

    public void EnablePlaceHolders() {
        PlayerBridgeUnitsPlaceHolders.ToList().ForEach(x => x.gameObject.SetActive(true));
    }
}