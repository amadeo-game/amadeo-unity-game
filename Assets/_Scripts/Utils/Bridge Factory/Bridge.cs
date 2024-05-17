using System.Linq;
using UnityEngine;
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
        
        // generate code that offset the y position of each element from the playableUnitsPositions array by bridgeRiseXOffset
        playableUnitsPositions =
            playableUnitsPositions.Select(pos => pos - new Vector2(0, bridgeRiseXOffset)).ToArray();
        

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
        BridgeTypeSO bridgeTypeSO, GameObject bridge, int bridgeRiseXOffset) {
        var len = unitPropertiesArray.Length;

        // len is 6
        GameObject[] bridgeEnvUnits = new GameObject[len];
        for (int i = 0; i < len; i++) {
            UnitProperties unit = unitPropertiesArray[i];
            GameObject unitType = bridgeTypeSO.GetEnvUnitType(unit.UnitType);
            Vector2 position = unit.Position - new Vector2(0, bridgeRiseXOffset);
            bridgeEnvUnits[i] =
                EnvElevationUnit(position: position, unitType: unitType, unit.IsMirrored, bridge);
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
        // add each element from each array intermittently

        return envUnits.Zip(playableUnits, (x, y) => new[] { x, y }).SelectMany(x => x).Append(envUnits.Last())
            .ToArray();
    }

    public static int[] GetSequencedBridgeHeights(int[] bridgeEnvHeights, int[] playerUnitsHeights) {
        // add each element from each array intermittently

        return bridgeEnvHeights.Zip(playerUnitsHeights, (x, y) => new[] { x, y }).SelectMany(x => x)
            .Append(bridgeEnvHeights.Last()).ToArray();
    }
}