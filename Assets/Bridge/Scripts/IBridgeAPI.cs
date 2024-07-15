using System;

public interface IBridgeAPI
{
    static event Action BridgeBuilt;
    
    static event Action OnGameStart;
    static event Action BridgeCollapsed;
    static event Action BridgeIsComplete;

    void BuildBridge(int[] unitHeights, BridgeTypeSO bridgeTypeSO = null);
    void CollapseBridge();
    
    void CompleteBridge();
    void PauseBridge();
}
