using UnityEngine;

[CreateAssetMenu(fileName = "BridgeType", menuName = "Bridge/BridgeType")]
public class BridgeTypeSO : ScriptableObject {
    [SerializeField] private BridgeSpriteCollection bridgeSpritesCollections;
    [SerializeField] private GameObject[] bridgeEnvUnit0Prefab;
    [SerializeField] private GameObject[] bridgeEnvUnit1Prefab;
    [SerializeField] private GameObject[] bridgeEnvUnit2Prefab;
    [SerializeField] private GameObject[] bridgeEnvUnit3Prefab;
    [SerializeField] private GameObject[] bridgeEnvUnit4Prefab;
    [SerializeField] private GameObject[] bridgeEnvUnit5Prefab;
    
    public BridgeSpriteCollection BridgeSpritesCollections => bridgeSpritesCollections;

    public GameObject BridgeEnvUnitPrefab(int index) {
        int randomIndex;
        switch (index) {
            case 0:
                randomIndex = Random.Range(0, bridgeEnvUnit0Prefab.Length);
                return bridgeEnvUnit0Prefab[randomIndex];
            case 1:
                randomIndex = Random.Range(0, bridgeEnvUnit1Prefab.Length);
                return bridgeEnvUnit1Prefab[randomIndex];
            case 2:
                randomIndex = Random.Range(0, bridgeEnvUnit2Prefab.Length);
                return bridgeEnvUnit2Prefab[randomIndex];
            case 3:
                randomIndex = Random.Range(0, bridgeEnvUnit3Prefab.Length);
                return bridgeEnvUnit3Prefab[randomIndex];
            case 4:
                randomIndex = Random.Range(0, bridgeEnvUnit4Prefab.Length);
                return bridgeEnvUnit4Prefab[randomIndex];
            case 5:
                randomIndex = Random.Range(0, bridgeEnvUnit5Prefab.Length);
                return bridgeEnvUnit5Prefab[randomIndex];
            default:
                Debug.LogError($"BridgeEnvUnitPrefab index {index} is out of range");
                throw new System.ArgumentOutOfRangeException();
        }
    }
}