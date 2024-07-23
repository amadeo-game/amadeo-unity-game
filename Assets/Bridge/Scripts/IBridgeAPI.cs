using System;
using BridgePackage;
public interface IBridgeAPI {

    void BuildBridge();
    void CollapseBridge();

    void CompleteBridge();

    void EnableGameUnits(bool doZeroF);
    
    SessionData GetSessionData();
    void PauseBridge();
}