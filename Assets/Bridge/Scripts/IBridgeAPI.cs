using System;

public interface IBridgeAPI
{
    static event Action BridgeBuilt;
    
    static event Action OnGameStart;
    static event Action BridgeCollapsed;
    static event Action BridgeIsComplete;

    void BuildBridge();
    void BuildBridge(int[] unitHeights, bool activateUnits = false);
    void CollapseBridge();
    void PauseBridge();
    void ResetBridge();
}
