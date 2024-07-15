using BridgePackage;
using UnityEngine;

// Attached to the GameObject GameManager 
public class LevelManager : MonoBehaviour
{
    private DemoBridgeHeights demoBridgeHeights;
    
    private int[] heights = { 0, 0, 0, 0, 0 };
    public int bridgeTypeIndex {get;  set; } = 0;

    [SerializeField] private BridgeAPI bridgeAPI;
    private void Awake() {
        demoBridgeHeights = GetComponent<DemoBridgeHeights>();
    }

    enum LevelType {Demo, Gameplay}
    
    [SerializeField] private LevelType levelType;
    
    [SerializeField] private BridgeCollectionSO bridgeCollectionSO;
    
    

    public void StartLevel() {
        if (levelType == LevelType.Demo) {
            bridgeAPI.BuildBridge(demoBridgeHeights.GetPlayerUnitsHeights(), bridgeCollectionSO.BridgeTypes[bridgeTypeIndex]);
        } else if (levelType == LevelType.Gameplay) {
            bridgeAPI.BuildBridge(heights, bridgeCollectionSO.BridgeTypes[bridgeTypeIndex]);
        }
    }
    [SerializeField] private int bridgeDifficulty;
    
    
}
