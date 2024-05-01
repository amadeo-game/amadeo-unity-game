using UnityEngine;

[CreateAssetMenu(fileName = "BridgeCollection", menuName = "Bridge/BridgeCollection")]
public class BridgeCollectionSO : ScriptableObject {
    public BridgeTypeSO[] BridgeTypes;
}
