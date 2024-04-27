using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class BridgeSpritesCollection
{
    private const int MinBridgeEnvSprites = 1;
    
    public SpriteUnit PlayerUnitSprite;
    public SpriteUnit[] EnvironmentSprites = new SpriteUnit[MinBridgeEnvSprites];
}

[System.Serializable]
public class SpriteUnit {
    
    [FormerlySerializedAs("isVertical")] public bool IsVertical = true;
    [FormerlySerializedAs("sprite")] public Sprite Sprite;
    
}
