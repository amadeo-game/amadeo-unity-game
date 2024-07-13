using UnityEngine;

public class LevelManager : MonoBehaviour
{
    // Create a random height units, and a random bridge collection
    
    [SerializeField] private BridgeCollectionSO bridgeCollectionSO;
    [SerializeField] private int[] unitHeights;
    [SerializeField] private int bridgeTypeIndex;
    [SerializeField] private int bridgeDifficulty;
}
