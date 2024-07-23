using System;
using BridgePackage;
public interface IBridgeAPI {
    static event Action BridgeReady;
    static event Action OnGameStart;
    static event Action BridgeCollapsed;
    static event Action FailedSession;
    static event Action BridgeIsComplete;
    static event Action WonSession;

    void BuildBridge(int[] unitHeights, BridgeTypeSO bridgeTypeSO, bool isLeftHand, bool isFlexion, float[] mvcValues, bool[] playableUnits, float[] unitsGrace, float timeDuration);
    void CollapseBridge();

    void CompleteBridge();

    void EnableGameUnits(bool doZeroF);
    
    SessionData GetSessionData();
    void PauseBridge();
}