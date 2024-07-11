using System;
using UnityEngine;

public struct UnitProperties {
    public Vector2 Position;
    public int UnitType;
    public bool IsMirrored;
}

public class BridgeBuilder {
    private static readonly int[] PlayerUnitXPos = { -8, -4, 0, 4, 8 };
    private static readonly int[] EnvUnitXPos = { -10, -6, -2, 2, 6, 10 };

    
    public static Vector2[] GetBridgePlayerPositions(int[] playableUnitsHeights) {
        int length = playableUnitsHeights.Length;
        Vector2[] playerHeights = new Vector2[length];
        
        for (int i = 0; i < length; i++) {
            playerHeights[i] = new Vector2(PlayerUnitXPos[i], playableUnitsHeights[i]);
        }
        return playerHeights;
    }
    
    
    public static UnitProperties[] GetBridgeEnvironmentHeights(int[] playableUnitsHeights) {
        int length = playableUnitsHeights.Length;
        UnitProperties[] envUnits = new UnitProperties[length + 1];
        
        // make a new array that has 0 in beginning then playableUnitsHeights then 0 in the end
        int[] heightsWithEdges = new int[length + 2];
        Array.Fill(heightsWithEdges, 0);
        
        Array.Copy( playableUnitsHeights, 0, heightsWithEdges, 1, length);
        // Debug.Log(string.Join(", ", heightsWithEdges));
        
        
        for (int i = 0; i < heightsWithEdges.Length-1; i++) {
            envUnits[i] = EnvUnit(heightsWithEdges[i], heightsWithEdges[i+1], EnvUnitXPos[i]);
        }
        
        return envUnits;
    }
    



    private static UnitProperties EnvUnit(int leftY, int rightY, int xPos) {
        UnitProperties unit = new UnitProperties();

        // Calculate the height difference of the adjacent units 
        var heightDifference = (leftY - rightY);
        var absHeightDifference = Mathf.Abs(heightDifference);
        float diffSign = Mathf.Sign(heightDifference);

        Vector2 pos = diffSign < 0
            ? new Vector3(xPos, leftY)
            : new Vector3(xPos, rightY);
        unit.Position = pos;
        unit.UnitType = absHeightDifference;
        if (diffSign >= 0) {
            unit.IsMirrored = true;
        }

        return unit;
    }
}
