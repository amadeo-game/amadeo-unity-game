using System;

public interface IBridgeAPI {
    static event Action BridgeReady;
    static event Action OnGameStart;
    static event Action BridgeCollapsed;
    static event Action BridgeIsComplete;

    void BuildBridge(int[] unitHeights, BridgeTypeSO bridgeTypeSO, bool isLeftHand, bool isFlexion, float[] mvcValues, bool[] playableUnits, float timeDuration);
    void CollapseBridge();

    void CompleteBridge();
    void PauseBridge();
}