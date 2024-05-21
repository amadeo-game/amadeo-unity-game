using System;

public interface IBridgeAPI
{
    event Action BridgeBuilt;
    event Action BridgeCollapsed;
    event Action BridgeIsComplete;

    void BuildBridge();
    void CollapseBridge();
    void PauseBridge();
    void ResetBridge();
}
