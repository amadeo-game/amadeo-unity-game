using System;
using System.Linq;
using UnityEngine;
using static UnityEngine.GameObject;
using static UnityEngine.Object;

public abstract class Bridge {
    public static void ReplacePUnitSprite(SpriteUnit sprite, GameObject[] bridgePlayerUnits,
        GameObject[] playerBridgeUnitsPlaceHolders) {
        for (int i = 0; i < bridgePlayerUnits.Length; i++) {
            SpriteReplacer.ReplaceSprite(sprite, bridgePlayerUnits[i]);
            SpriteReplacer.ReplaceSprite(sprite, playerBridgeUnitsPlaceHolders[i]);
        }
    }

    public static GameObject[] BuildPlayerUnits(GameObject bridge, Vector2[] playableUnitsPositions,
        GameObject playerUnitPrefab, int bridgeRiseXOffset) {
        var len = playableUnitsPositions.Length;
        GameObject[] bridgePlayerUnits = new GameObject[len];
        
        // debug.log playableUnitsPositions
        
        Debug.Log( string.Join(", ", playableUnitsPositions.Select(x => x.ToString()).ToArray()));
        
        // generate code that offset the y position of each element from the playableUnitsPositions array by bridgeRiseXOffset
        playableUnitsPositions = playableUnitsPositions.Select(pos => pos - new Vector2(0, bridgeRiseXOffset)).ToArray();
        
        Debug.Log( "after offset " + string.Join(", ", playableUnitsPositions.Select(x => x.ToString()).ToArray()));


        for (int i = 0; i < len; i++) {
            var rotation = playerUnitPrefab.transform.rotation;
            bridgePlayerUnits[i] = Instantiate(playerUnitPrefab,
                playableUnitsPositions[i],
                rotation, bridge.transform);
        }

        return bridgePlayerUnits;
    }

    public static GameObject[] BuildPlaceHolderUnits(GameObject bridge, Vector2[] playableUnitsPositions,
        GameObject playerUnitPlaceHolder) {
        var len = playableUnitsPositions.Length;

        GameObject[] playerBridgeUnitsPlaceHolders = new GameObject[len];

        for (int i = 0; i < len; i++) {
            var rotation = playerUnitPlaceHolder.transform.rotation;

            playerBridgeUnitsPlaceHolders[i] = Instantiate(playerUnitPlaceHolder,
                playableUnitsPositions[i],
                rotation, bridge.transform);
            playerBridgeUnitsPlaceHolders[i].gameObject.SetActive(false);
        }

        return playerBridgeUnitsPlaceHolders;
    }

    public static GameObject[] GenerateBridgeEnvironment(UnitProperties[] unitPropertiesArray,
        BridgeTypeSO bridgeTypeSO, GameObject bridge) {
        var len = unitPropertiesArray.Length;

        // len is 6
        GameObject[] bridgeEnvUnits = new GameObject[len];


        for (int i = 0; i < len; i++) {
            UnitProperties unit = unitPropertiesArray[i];
            GameObject unitType = bridgeTypeSO.GetEnvUnitType(unit.UnitType);
            bridgeEnvUnits[i] =
                EnvElevationUnit(position: unit.Position, unitType: unitType, unit.IsMirrored, bridge);
        }

        return bridgeEnvUnits;
    }

    private static GameObject EnvElevationUnit(Vector2 position, GameObject unitType, bool isMirrored,
        GameObject parent) {
        GameObject envUnit =
            Instantiate(unitType, position,
                Quaternion.identity, parent.transform);
        if (isMirrored) {
            envUnit.transform.localScale = new Vector3(-1, 1, 1);
        }

        return envUnit;
    }

    public static void ReplaceEnvSprites(GameObject[] bridgeEnvUnits, SpriteUnit spriteUnit) {
        foreach (var envUnit in bridgeEnvUnits) {
            bool isMirrored = envUnit.transform.localScale.x < 0;
            SpriteRenderer[] children = envUnit.GetComponentsInChildren<SpriteRenderer>();
            foreach (var child in children) {
                SpriteReplacer.ReplaceSprite(spriteUnit, child.gameObject,
                    isMirrored);
            }
        }
    }

    public static GameObject[] GetSequencedBridgeUnits(GameObject[] playableUnits, GameObject[] envUnits) {
        // add each element from each array intermittently, note that one of the arrays is shorter
        
        return envUnits.Zip(playableUnits, (x, y) => new[] {x, y}).SelectMany(x => x).ToArray();
        // GameObject[] bridgeUnits = new GameObject[playableUnits.Length + envUnits.Length];
        // for (int i = 0; i < envUnits.Length; i++) {
        //     bridgeUnits[i * 2] = envUnits[i];
        //     bridgeUnits[i * 2 + 1] = playableUnits[i];
        // }
        //
        // return bridgeUnits;
    }

    public static int[] GetSequencedBridgeHeights(int[] bridgeEnvHeights, int[] playerUnitsHeights) {
        // add each element from each array intermittently, note that one of the arrays is shorter
        
        return bridgeEnvHeights.Zip(playerUnitsHeights, (x, y) => new[] {x, y}).SelectMany(x => x).ToArray();
        
        // int len = Mathf.Max( bridgeEnvHeights.Length, playerUnitsHeights.Length);
        // int[] sequencedBridgeHeights = new int[bridgeEnvHeights.Length + playerUnitsHeights.Length];
        // for (int i = 0; i < len; i++) {
        //     sequencedBridgeHeights[i * 2] = bridgeEnvHeights[i];
        //     sequencedBridgeHeights[i * 2 + 1] = playerUnitsHeights[i];
        // }
        //
        // return sequencedBridgeHeights;
    }
}