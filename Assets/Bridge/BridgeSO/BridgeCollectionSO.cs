using UnityEngine;

[CreateAssetMenu(fileName = "BridgeCollection", menuName = "Bridge/BridgeCollection")]
public class BridgeCollectionSO : ScriptableObject {
    public BridgeTypeSO[] BridgeTypes;
}


// using UnityEngine;
//
//
// [System.Serializable]
// public class BridgeSpriteCollection {
//     private const int MinBridgeEnvSprites = 1;
//
//     public SpriteUnit PlayerUnitSprite;
//     public SpriteUnit[] EnvironmentSprites = new SpriteUnit[MinBridgeEnvSprites];
// }
//
// [System.Serializable]
// public class SpriteUnit {
//     // if the sprite is vertical or horizontal visually from the sprite sheet, needed for sprite replacement
//     public bool IsVertical = true;
//     public Sprite Sprite;
// }