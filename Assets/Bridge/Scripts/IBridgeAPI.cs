using System;

public interface IBridgeAPI
{
    static event Action BridgeBuilt;
    
    static event Action OnGameStart;
    static event Action BridgeCollapsed;
    static event Action BridgeIsComplete;

    void BuildBridge();
    void BuildBridge(int[] unitHeights, BridgeCollectionSO collectionSO = null, int bridgeTypeIndex = 0);
    void CollapseBridge();
    
    void CompleteBridge();
    void PauseBridge();
}
