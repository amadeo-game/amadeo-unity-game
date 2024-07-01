[System.Serializable]
public class BridgeSpriteCollection {
    private const int MinBridgeEnvSprites = 1;

    public SpriteUnit PlayerUnitSprite;
    public SpriteUnit[] EnvironmentSprites = new SpriteUnit[MinBridgeEnvSprites];
}

